using Microsoft.Data.SqlClient;
using VaughanBar.Data;
using VaughanBar.Models;

namespace VaughanBar.Repositories
{
    public class EstoqueRepository
    {
        private readonly ProdutoRepository _produtoRepo = new();

        /// <summary>Registra uma movimentação manual de estoque (entrada de fornecedor, perda, ajuste, etc.).</summary>
        public void RegistrarMovimento(int produtoId, string tipo, int quantidade, string? observacao)
        {
            if (quantidade <= 0) throw new ArgumentException("Quantidade deve ser maior que zero.");
            tipo = tipo.Trim();
            if (tipo != "Entrada" && tipo != "Saida")
                throw new ArgumentException("Tipo de movimento deve ser 'Entrada' ou 'Saida'.");

            using var conn = ConexaoBanco.Abrir();
            using var tx = conn.BeginTransaction();
            try
            {
                if (tipo == "Saida")
                {
                    using var cmdCheck = new SqlCommand("SELECT QuantidadeEstoque FROM Produtos WHERE Id=@id", conn, tx);
                    cmdCheck.Parameters.AddWithValue("@id", produtoId);
                    var atual = (int)(cmdCheck.ExecuteScalar() ?? 0);
                    if (atual < quantidade)
                        throw new InvalidOperationException($"Estoque insuficiente (disponível: {atual}).");
                }

                using (var cmd = new SqlCommand(@"
                    INSERT INTO MovimentosEstoque (ProdutoId, TipoMovimento, Quantidade, DataMovimento, Observacao)
                    VALUES (@produtoId, @tipo, @quantidade, @data, @obs)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@produtoId", produtoId);
                    cmd.Parameters.AddWithValue("@tipo", tipo);
                    cmd.Parameters.AddWithValue("@quantidade", quantidade);
                    cmd.Parameters.AddWithValue("@data", DateTime.Now);
                    cmd.Parameters.AddWithValue("@obs", (object?)observacao ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                int delta = tipo == "Entrada" ? quantidade : -quantidade;
                _produtoRepo.AjustarEstoque(conn, tx, produtoId, delta);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<MovimentoEstoque> Historico(int? produtoId = null, int top = 100)
        {
            var lista = new List<MovimentoEstoque>();
            using var conn = ConexaoBanco.Abrir();
            string sql = $@"
                SELECT TOP {top} m.Id, m.ProdutoId, p.Nome, m.TipoMovimento, m.Quantidade, m.DataMovimento, m.Observacao
                FROM MovimentosEstoque m
                JOIN Produtos p ON p.Id = m.ProdutoId
                WHERE (@produtoId IS NULL OR m.ProdutoId = @produtoId)
                ORDER BY m.DataMovimento DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@produtoId", (object?)produtoId ?? DBNull.Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new MovimentoEstoque
                {
                    Id = reader.GetInt32(0),
                    ProdutoId = reader.GetInt32(1),
                    ProdutoNome = reader.GetString(2),
                    TipoMovimento = reader.GetString(3),
                    Quantidade = reader.GetInt32(4),
                    DataMovimento = reader.GetDateTime(5),
                    Observacao = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return lista;
        }

        /// <summary>Produtos cuja quantidade em estoque está abaixo (ou igual) do mínimo definido.</summary>
        public List<Produto> ProdutosComEstoqueBaixo()
        {
            var lista = new List<Produto>();
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(@"
                SELECT Id, Nome, Preco, Categoria, QuantidadeEstoque, EstoqueMinimo, Ativo
                FROM Produtos
                WHERE Ativo = 1 AND QuantidadeEstoque <= EstoqueMinimo
                ORDER BY QuantidadeEstoque ASC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Produto
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Preco = reader.GetDecimal(2),
                    Categoria = reader.IsDBNull(3) ? null : reader.GetString(3),
                    QuantidadeEstoque = reader.GetInt32(4),
                    EstoqueMinimo = reader.GetInt32(5),
                    Ativo = reader.GetBoolean(6)
                });
            }
            return lista;
        }
    }
}
