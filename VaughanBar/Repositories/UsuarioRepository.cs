using Microsoft.Data.SqlClient;
using VaughanBar.Data;
using VaughanBar.Models;

namespace VaughanBar.Repositories
{
    public class UsuarioRepository
    {
        public Usuario? Autenticar(string login, string senha)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(
                "SELECT Id, Nome, Login, Cargo, Ativo FROM Usuarios WHERE Login = @login AND Senha = @senha AND Ativo = 1",
                conn);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@senha", senha);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Login = reader.GetString(2),
                    Cargo = reader.GetString(3),
                    Ativo = reader.GetBoolean(4)
                };
            }
            return null;
        }

        public List<Usuario> Listar()
        {
            var lista = new List<Usuario>();
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("SELECT Id, Nome, Login, Cargo, Ativo FROM Usuarios ORDER BY Nome", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Login = reader.GetString(2),
                    Cargo = reader.GetString(3),
                    Ativo = reader.GetBoolean(4)
                });
            }
            return lista;
        }

        public int Inserir(Usuario u)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand(
                @"INSERT INTO Usuarios (Nome, Login, Senha, Cargo, Ativo)
                  OUTPUT INSERTED.Id
                  VALUES (@nome, @login, @senha, @cargo, @ativo)", conn);
            cmd.Parameters.AddWithValue("@nome", u.Nome);
            cmd.Parameters.AddWithValue("@login", u.Login);
            cmd.Parameters.AddWithValue("@senha", u.Senha ?? "");
            cmd.Parameters.AddWithValue("@cargo", u.Cargo);
            cmd.Parameters.AddWithValue("@ativo", u.Ativo);
            return (int)cmd.ExecuteScalar();
        }

        public bool Atualizar(Usuario u)
        {
            using var conn = ConexaoBanco.Abrir();
            string sql = string.IsNullOrEmpty(u.Senha)
                ? "UPDATE Usuarios SET Nome=@nome, Login=@login, Cargo=@cargo, Ativo=@ativo WHERE Id=@id"
                : "UPDATE Usuarios SET Nome=@nome, Login=@login, Cargo=@cargo, Ativo=@ativo, Senha=@senha WHERE Id=@id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", u.Nome);
            cmd.Parameters.AddWithValue("@login", u.Login);
            cmd.Parameters.AddWithValue("@cargo", u.Cargo);
            cmd.Parameters.AddWithValue("@ativo", u.Ativo);
            cmd.Parameters.AddWithValue("@id", u.Id);
            if (!string.IsNullOrEmpty(u.Senha))
                cmd.Parameters.AddWithValue("@senha", u.Senha);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remover(int id)
        {
            using var conn = ConexaoBanco.Abrir();
            using var cmd = new SqlCommand("DELETE FROM Usuarios WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
