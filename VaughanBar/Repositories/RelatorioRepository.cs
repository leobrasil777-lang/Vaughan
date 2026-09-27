using Microsoft.Data.SqlClient;
using VaughanBar.Data;

namespace VaughanBar.Repositories
{
    public class ResumoVendas
    {
        public decimal TotalVendido { get; set; }
        public int QuantidadeComandas { get; set; }
        public decimal TicketMedio { get; set; }
    }

    public class ProdutoVendido
    {
        public string Nome { get; set; } = "";
        public int QuantidadeVendida { get; set; }
        public decimal TotalVendido { get; set; }
    }

    public class VendaPorUsuario
    {
        public string UsuarioNome { get; set; } = "";
        public int QuantidadeComandas { get; set; }
        public decimal TotalVendido { get; set; }
    }

    public class RelatorioRepository
    {
        public ResumoVendas Resumo(DateTime inicio, DateTime fim)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(@"
                SELECT
                    ISNULL(SUM(ip.Quantidade * ip.PrecoUnitario), 0) AS Total,
                    COUNT(DISTINCT p.Id) AS QtdComandas
                FROM Pedidos p
                JOIN ItensPedido ip ON ip.PedidoId = p.Id
                WHERE p.Status = 'Fechado' AND p.DataPedido BETWEEN @inicio AND @fim", conn);
            cmd.Parameters.AddWithValue("@inicio", inicio);
            cmd.Parameters.AddWithValue("@fim", fim);
            using var reader = cmd.ExecuteReader();
            reader.Read();
            var total = reader.GetDecimal(0);
            var qtd = reader.GetInt32(1);
            return new ResumoVendas
            {
                TotalVendido = total,
                QuantidadeComandas = qtd,
                TicketMedio = qtd > 0 ? Math.Round(total / qtd, 2) : 0
            };
        }

        public List<ProdutoVendido> ProdutosMaisVendidos(DateTime inicio, DateTime fim, int top = 10)
        {
            var lista = new List<ProdutoVendido>();
            using var conn = ConexaoBanco.Abrir();
            string sql = $@"
                SELECT TOP {top} pr.Nome,
                       SUM(ip.Quantidade) AS Qtd,
                       SUM(ip.Quantidade * ip.PrecoUnitario) AS Total
                FROM ItensPedido ip
                JOIN Produtos pr ON pr.Id = ip.ProdutoId
                JOIN Pedidos p ON p.Id = ip.PedidoId
                WHERE p.Status = 'Fechado' AND p.DataPedido BETWEEN @inicio AND @fim
                GROUP BY pr.Nome
                ORDER BY Qtd DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@inicio", inicio);
            cmd.Parameters.AddWithValue("@fim", fim);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new ProdutoVendido
                {
                    Nome = reader.GetString(0),
                    QuantidadeVendida = reader.GetInt32(1),
                    TotalVendido = reader.GetDecimal(2)
                });
            }
            return lista;
        }

        public List<VendaPorUsuario> VendasPorUsuario(DateTime inicio, DateTime fim)
        {
            var lista = new List<VendaPorUsuario>();
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(@"
                SELECT ISNULL(u.Nome, 'Sem garçom vinculado') AS Nome,
                       COUNT(DISTINCT p.Id) AS QtdComandas,
                       SUM(ip.Quantidade * ip.PrecoUnitario) AS Total
                FROM Pedidos p
                JOIN ItensPedido ip ON ip.PedidoId = p.Id
                LEFT JOIN Usuarios u ON u.Id = p.UsuarioId
                WHERE p.Status = 'Fechado' AND p.DataPedido BETWEEN @inicio AND @fim
                GROUP BY u.Nome
                ORDER BY Total DESC", conn);
            cmd.Parameters.AddWithValue("@inicio", inicio);
            cmd.Parameters.AddWithValue("@fim", fim);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new VendaPorUsuario
                {
                    UsuarioNome = reader.GetString(0),
                    QuantidadeComandas = reader.GetInt32(1),
                    TotalVendido = reader.GetDecimal(2)
                });
            }
            return lista;
        }
    }
}
