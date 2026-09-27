using Microsoft.Data.SqlClient;
using VaughanBar.Data;
using VaughanBar.Models;

namespace VaughanBar.Repositories
{
    public class MesaRepository
    {
        public List<Mesa> Listar()
        {
            var lista = new List<Mesa>();
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("SELECT Id, Numero, Capacidade, Status FROM Mesas ORDER BY Numero", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Mesa
                {
                    Id = reader.GetInt32(0),
                    Numero = reader.GetInt32(1),
                    Capacidade = reader.GetInt32(2),
                    Status = reader.GetString(3)
                });
            }
            return lista;
        }

        public Mesa? BuscarPorId(int id)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("SELECT Id, Numero, Capacidade, Status FROM Mesas WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new Mesa
            {
                Id = reader.GetInt32(0),
                Numero = reader.GetInt32(1),
                Capacidade = reader.GetInt32(2),
                Status = reader.GetString(3)
            };
        }

        public int Inserir(Mesa m)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(
                "INSERT INTO Mesas (Numero, Capacidade, Status) OUTPUT INSERTED.Id VALUES (@num, @cap, @status)", conn);
            cmd.Parameters.AddWithValue("@num", m.Numero);
            cmd.Parameters.AddWithValue("@cap", m.Capacidade);
            cmd.Parameters.AddWithValue("@status", m.Status);
            return (int)cmd.ExecuteScalar();
        }

        public bool AtualizarStatus(int id, string status)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("UPDATE Mesas SET Status=@status WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remover(int id)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("DELETE FROM Mesas WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
