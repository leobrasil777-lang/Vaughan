using Microsoft.Data.SqlClient;
using VaughanBar.Data;
using VaughanBar.Models;

namespace VaughanBar.Repositories
{
    public class ProdutoRepository
    {
        public List<Produto> Listar(bool somenteAtivos = false)
        {
            var lista = new List<Produto>();
            using var conn = ConexaoBanco.Abrir();
            string sql = "SELECT Id, Nome, Preco, Categoria, QuantidadeEstoque, EstoqueMinimo, Ativo FROM Produtos";
            if (somenteAtivos) sql += " WHERE Ativo = 1";
            sql += " ORDER BY Categoria, Nome";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(Ler(reader));
            return lista;
        }

        public Produto? BuscarPorId(int id)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(
                "SELECT Id, Nome, Preco, Categoria, QuantidadeEstoque, EstoqueMinimo, Ativo FROM Produtos WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Ler(reader) : null;
        }

        public int Inserir(Produto p)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(
                @"INSERT INTO Produtos (Nome, Preco, Categoria, QuantidadeEstoque, EstoqueMinimo, Ativo)
                  OUTPUT INSERTED.Id
                  VALUES (@nome, @preco, @categoria, @qtd, @min, @ativo)", conn);
            cmd.Parameters.AddWithValue("@nome", p.Nome);
            cmd.Parameters.AddWithValue("@preco", p.Preco);
            cmd.Parameters.AddWithValue("@categoria", (object?)p.Categoria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@qtd", p.QuantidadeEstoque);
            cmd.Parameters.AddWithValue("@min", p.EstoqueMinimo);
            cmd.Parameters.AddWithValue("@ativo", p.Ativo);
            return (int)cmd.ExecuteScalar();
        }

        public bool Atualizar(Produto p)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(
                @"UPDATE Produtos SET Nome=@nome, Preco=@preco, Categoria=@categoria,
                  EstoqueMinimo=@min, Ativo=@ativo WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@nome", p.Nome);
            cmd.Parameters.AddWithValue("@preco", p.Preco);
            cmd.Parameters.AddWithValue("@categoria", (object?)p.Categoria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@min", p.EstoqueMinimo);
            cmd.Parameters.AddWithValue("@ativo", p.Ativo);
            cmd.Parameters.AddWithValue("@id", p.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remover(int id)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("UPDATE Produtos SET Ativo = 0 WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        /// <summary>Ajusta o estoque somando (ou subtraindo) uma quantidade. Usa transação própria da conexão chamadora se fornecida.</summary>
        public void AjustarEstoque(SqlConnection conn, SqlTransaction? tx, int produtoId, int delta)
        {
            using var cmd = new SqlCommand(
                "UPDATE Produtos SET QuantidadeEstoque = QuantidadeEstoque + @delta WHERE Id=@id", conn, tx);
            cmd.Parameters.AddWithValue("@delta", delta);
            cmd.Parameters.AddWithValue("@id", produtoId);
            cmd.ExecuteNonQuery();
        }

        private static Produto Ler(SqlDataReader reader) => new()
        {
            Id = reader.GetInt32(0),
            Nome = reader.GetString(1),
            Preco = reader.GetDecimal(2),
            Categoria = reader.IsDBNull(3) ? null : reader.GetString(3),
            QuantidadeEstoque = reader.GetInt32(4),
            EstoqueMinimo = reader.GetInt32(5),
            Ativo = reader.GetBoolean(6)
        };
    }
}
