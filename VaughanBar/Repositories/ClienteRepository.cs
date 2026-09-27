using Microsoft.Data.SqlClient;
using VaughanBar.Data;
using VaughanBar.Models;

namespace VaughanBar.Repositories
{
    public class ClienteRepository
    {
        public List<Cliente> Listar()
        {
            var lista = new List<Cliente>();
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("SELECT Id, Nome, Telefone FROM Clientes ORDER BY Nome", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Cliente
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Telefone = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }
            return lista;
        }

        public int Inserir(Cliente c)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(
                "INSERT INTO Clientes (Nome, Telefone) OUTPUT INSERTED.Id VALUES (@nome, @tel)", conn);
            cmd.Parameters.AddWithValue("@nome", c.Nome);
            cmd.Parameters.AddWithValue("@tel", (object?)c.Telefone ?? DBNull.Value);
            return (int)cmd.ExecuteScalar();
        }

        public bool Atualizar(Cliente c)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("UPDATE Clientes SET Nome=@nome, Telefone=@tel WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@nome", c.Nome);
            cmd.Parameters.AddWithValue("@tel", (object?)c.Telefone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", c.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remover(int id)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("DELETE FROM Clientes WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
