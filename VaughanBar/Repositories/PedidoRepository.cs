using Microsoft.Data.SqlClient;
using VaughanBar.Data;
using VaughanBar.Models;

namespace VaughanBar.Repositories
{
    public class PedidoRepository
    {
        private readonly ProdutoRepository _produtoRepo = new();

        public List<Pedido> Listar(string? status = null, int? mesaId = null)
        {
            var lista = new List<Pedido>();
            using var conn = ConexaoBanco.Abrir();
            string sql = @"
                SELECT p.Id, p.ClienteId, c.Nome, p.MesaId, m.Numero, p.UsuarioId, u.Nome,
                       p.DataPedido, p.Status
                FROM Pedidos p
                LEFT JOIN Clientes c ON c.Id = p.ClienteId
                LEFT JOIN Mesas m ON m.Id = p.MesaId
                LEFT JOIN Usuarios u ON u.Id = p.UsuarioId
                WHERE 1=1";
            if (status != null) sql += " AND p.Status = @status";
            if (mesaId != null) sql += " AND p.MesaId = @mesaId";
            sql += " ORDER BY p.DataPedido DESC";

            using var cmd = new SqlCommand(sql, conn);
            if (status != null) cmd.Parameters.AddWithValue("@status", status);
            if (mesaId != null) cmd.Parameters.AddWithValue("@mesaId", mesaId.Value);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    lista.Add(LerCabecalho(reader));
            }

            // Carrega itens e total de cada pedido (lista costuma ser pequena: comandas em aberto)
            foreach (var pedido in lista)
                CarregarItens(conn, pedido);

            return lista;
        }

        public Pedido? BuscarPorId(int id)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(@"
                SELECT p.Id, p.ClienteId, c.Nome, p.MesaId, m.Numero, p.UsuarioId, u.Nome,
                       p.DataPedido, p.Status
                FROM Pedidos p
                LEFT JOIN Clientes c ON c.Id = p.ClienteId
                LEFT JOIN Mesas m ON m.Id = p.MesaId
                LEFT JOIN Usuarios u ON u.Id = p.UsuarioId
                WHERE p.Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            var pedido = LerCabecalho(reader);
            reader.Close();
            CarregarItens(conn, pedido);
            return pedido;
        }

        /// <summary>Abre uma nova comanda vinculada (opcionalmente) a uma mesa, cliente e usuário/garçom.</summary>
        public int AbrirComanda(int? clienteId, int? mesaId, int? usuarioId)
        {
            using var conn = ConexaoBanco.Abrir();
            using var tx = conn.BeginTransaction();
            try
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Pedidos (ClienteId, MesaId, UsuarioId, DataPedido, Status)
                    OUTPUT INSERTED.Id
                    VALUES (@clienteId, @mesaId, @usuarioId, @data, 'Aberto')", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@clienteId", (object?)clienteId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mesaId", (object?)mesaId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@usuarioId", (object?)usuarioId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@data", DateTime.Now);
                    int novoId = (int)cmd.ExecuteScalar();

                    if (mesaId != null)
                    {
                        using var cmdMesa = new SqlCommand("UPDATE Mesas SET Status='Ocupada' WHERE Id=@id", conn, tx);
                        cmdMesa.Parameters.AddWithValue("@id", mesaId.Value);
                        cmdMesa.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return novoId;
                }
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>Adiciona um item à comanda e dá baixa no estoque do produto.</summary>
        public void AdicionarItem(int pedidoId, int produtoId, int quantidade)
        {
            if (quantidade <= 0) throw new ArgumentException("Quantidade deve ser maior que zero.");

            using var conn = ConexaoBanco.Abrir();
            using var tx = conn.BeginTransaction();
            try
            {
                using (var cmdPedido = new SqlCommand(
                    "SELECT Status FROM Pedidos WITH (UPDLOCK, HOLDLOCK) WHERE Id=@id", conn, tx))
                {
                    cmdPedido.Parameters.AddWithValue("@id", pedidoId);
                    var status = cmdPedido.ExecuteScalar() as string;
                    if (status == null) throw new InvalidOperationException("Comanda não encontrada.");
                    if (status != "Aberto") throw new InvalidOperationException("A comanda não está aberta.");
                }

                // Confere estoque disponível e pega o preço atual do produto
                decimal preco;
                int estoqueAtual;
                using (var cmdProd = new SqlCommand(
                    "SELECT Preco, QuantidadeEstoque FROM Produtos WITH (UPDLOCK, HOLDLOCK) WHERE Id=@id AND Ativo=1", conn, tx))
                {
                    cmdProd.Parameters.AddWithValue("@id", produtoId);
                    using var reader = cmdProd.ExecuteReader();
                    if (!reader.Read()) throw new InvalidOperationException("Produto não encontrado ou inativo.");
                    preco = reader.GetDecimal(0);
                    estoqueAtual = reader.GetInt32(1);
                }

                if (estoqueAtual < quantidade)
                    throw new InvalidOperationException($"Estoque insuficiente (disponível: {estoqueAtual}).");

                using (var cmdItem = new SqlCommand(@"
                    INSERT INTO ItensPedido (PedidoId, ProdutoId, Quantidade, PrecoUnitario)
                    VALUES (@pedidoId, @produtoId, @quantidade, @preco)", conn, tx))
                {
                    cmdItem.Parameters.AddWithValue("@pedidoId", pedidoId);
                    cmdItem.Parameters.AddWithValue("@produtoId", produtoId);
                    cmdItem.Parameters.AddWithValue("@quantidade", quantidade);
                    cmdItem.Parameters.AddWithValue("@preco", preco);
                    cmdItem.ExecuteNonQuery();
                }

                _produtoRepo.AjustarEstoque(conn, tx, produtoId, -quantidade);

                using (var cmdMov = new SqlCommand(@"
                    INSERT INTO MovimentosEstoque (ProdutoId, TipoMovimento, Quantidade, DataMovimento, Observacao)
                    VALUES (@produtoId, 'Saida', @quantidade, @data, @obs)", conn, tx))
                {
                    cmdMov.Parameters.AddWithValue("@produtoId", produtoId);
                    cmdMov.Parameters.AddWithValue("@quantidade", quantidade);
                    cmdMov.Parameters.AddWithValue("@data", DateTime.Now);
                    cmdMov.Parameters.AddWithValue("@obs", $"Venda - Pedido #{pedidoId}");
                    cmdMov.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>Remove um item da comanda e devolve a quantidade ao estoque.</summary>
        public void RemoverItem(int itemId)
        {
            using var conn = ConexaoBanco.Abrir();
            using var tx = conn.BeginTransaction();
            try
            {
                int produtoId, quantidade;
                using (var cmdSel = new SqlCommand(
                    @"SELECT ip.ProdutoId, ip.Quantidade
                      FROM ItensPedido ip
                      JOIN Pedidos p WITH (UPDLOCK, HOLDLOCK) ON p.Id=ip.PedidoId
                      WHERE ip.Id=@id AND p.Status='Aberto'", conn, tx))
                {
                    cmdSel.Parameters.AddWithValue("@id", itemId);
                    using var reader = cmdSel.ExecuteReader();
                    if (!reader.Read()) throw new InvalidOperationException("Item não encontrado em comanda aberta.");
                    produtoId = reader.GetInt32(0);
                    quantidade = reader.GetInt32(1);
                }

                using (var cmdDel = new SqlCommand("DELETE FROM ItensPedido WHERE Id=@id", conn, tx))
                {
                    cmdDel.Parameters.AddWithValue("@id", itemId);
                    cmdDel.ExecuteNonQuery();
                }

                _produtoRepo.AjustarEstoque(conn, tx, produtoId, quantidade);

                using (var cmdMov = new SqlCommand(@"
                    INSERT INTO MovimentosEstoque (ProdutoId, TipoMovimento, Quantidade, DataMovimento, Observacao)
                    VALUES (@produtoId, 'Entrada', @quantidade, @data, 'Estorno de item removido da comanda')", conn, tx))
                {
                    cmdMov.Parameters.AddWithValue("@produtoId", produtoId);
                    cmdMov.Parameters.AddWithValue("@quantidade", quantidade);
                    cmdMov.Parameters.AddWithValue("@data", DateTime.Now);
                    cmdMov.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>Fecha a comanda (marca como Fechado) e libera a mesa vinculada, se houver.</summary>
        public void FecharComanda(int pedidoId)
        {
            using var conn = ConexaoBanco.Abrir();
            using var tx = conn.BeginTransaction();
            try
            {
                int? mesaId = null;
                using (var cmdSel = new SqlCommand("SELECT MesaId FROM Pedidos WHERE Id=@id", conn, tx))
                {
                    cmdSel.Parameters.AddWithValue("@id", pedidoId);
                    var result = cmdSel.ExecuteScalar();
                    if (result != null && result != DBNull.Value) mesaId = (int)result;
                }

                using (var cmdUpd = new SqlCommand("UPDATE Pedidos SET Status='Fechado' WHERE Id=@id", conn, tx))
                {
                    cmdUpd.Parameters.AddWithValue("@id", pedidoId);
                    cmdUpd.ExecuteNonQuery();
                }

                if (mesaId != null)
                {
                    using var cmdMesa = new SqlCommand("UPDATE Mesas SET Status='Livre' WHERE Id=@id", conn, tx);
                    cmdMesa.Parameters.AddWithValue("@id", mesaId.Value);
                    cmdMesa.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>Cancela uma comanda aberta e estorna seus itens em uma única transação.</summary>
        public void CancelarComanda(int pedidoId)
        {
            using var conn = ConexaoBanco.Abrir();
            using var tx = conn.BeginTransaction();
            try
            {
                int? mesaId;
                using (var cmdPedido = new SqlCommand(
                    "SELECT MesaId, Status FROM Pedidos WITH (UPDLOCK, HOLDLOCK) WHERE Id=@id", conn, tx))
                {
                    cmdPedido.Parameters.AddWithValue("@id", pedidoId);
                    using var reader = cmdPedido.ExecuteReader();
                    if (!reader.Read()) throw new InvalidOperationException("Comanda não encontrada.");
                    mesaId = reader.IsDBNull(0) ? null : reader.GetInt32(0);
                    if (reader.GetString(1) != "Aberto")
                        throw new InvalidOperationException("Somente uma comanda aberta pode ser cancelada.");
                }

                // Registra um estorno por produto, inclusive quando houver vários itens do mesmo produto.
                using (var cmdEstorno = new SqlCommand(@"
                    INSERT INTO MovimentosEstoque (ProdutoId, TipoMovimento, Quantidade, DataMovimento, Observacao)
                    SELECT ProdutoId, 'Entrada', SUM(Quantidade), GETDATE(), @observacao
                    FROM ItensPedido WHERE PedidoId=@id GROUP BY ProdutoId;

                    UPDATE p SET QuantidadeEstoque = p.QuantidadeEstoque + i.Quantidade
                    FROM Produtos p
                    JOIN (SELECT ProdutoId, SUM(Quantidade) AS Quantidade
                          FROM ItensPedido WHERE PedidoId=@id GROUP BY ProdutoId) i ON i.ProdutoId=p.Id;", conn, tx))
                {
                    cmdEstorno.Parameters.AddWithValue("@id", pedidoId);
                    cmdEstorno.Parameters.AddWithValue("@observacao", $"Estorno de cancelamento - Pedido #{pedidoId}");
                    cmdEstorno.ExecuteNonQuery();
                }

                using (var cmdStatus = new SqlCommand(
                    "UPDATE Pedidos SET Status='Cancelado' WHERE Id=@id AND Status='Aberto'", conn, tx))
                {
                    cmdStatus.Parameters.AddWithValue("@id", pedidoId);
                    if (cmdStatus.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("Não foi possível cancelar a comanda.");
                }

                if (mesaId != null)
                {
                    using var cmdMesa = new SqlCommand(@"
                        UPDATE Mesas SET Status='Livre'
                        WHERE Id=@mesaId
                          AND NOT EXISTS (
                            SELECT 1 FROM Pedidos
                            WHERE MesaId=@mesaId AND Status='Aberto')", conn, tx);
                    cmdMesa.Parameters.AddWithValue("@mesaId", mesaId.Value);
                    cmdMesa.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private static Pedido LerCabecalho(SqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            ClienteId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
            ClienteNome = reader.IsDBNull(2) ? null : reader.GetString(2),
            MesaId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            MesaNumero = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            UsuarioId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            UsuarioNome = reader.IsDBNull(6) ? null : reader.GetString(6),
            DataPedido = reader.GetDateTime(7),
            Status = reader.GetString(8)
        };

        private static void CarregarItens(SqlConnection conn, Pedido pedido)
        {
            using var cmd = new SqlCommand(@"
                SELECT ip.Id, ip.ProdutoId, pr.Nome, ip.Quantidade, ip.PrecoUnitario
                FROM ItensPedido ip
                JOIN Produtos pr ON pr.Id = ip.ProdutoId
                WHERE ip.PedidoId = @id", conn);
            cmd.Parameters.AddWithValue("@id", pedido.Id);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pedido.Itens.Add(new ItemPedido
                {
                    Id = reader.GetInt32(0),
                    PedidoId = pedido.Id,
                    ProdutoId = reader.GetInt32(1),
                    ProdutoNome = reader.GetString(2),
                    Quantidade = reader.GetInt32(3),
                    PrecoUnitario = reader.GetDecimal(4)
                });
            }
            pedido.Total = pedido.Itens.Sum(i => i.Subtotal);
        }
    }
}
