using Microsoft.Data.SqlClient;

namespace VaughanBar.Data
{
    /// <summary>
    /// Fábrica de conexões ADO.NET. Toda a comunicação com o banco passa por aqui,
    /// usando SqlConnection/SqlCommand diretamente (sem ORM, sem query builder).
    /// </summary>
    public static class ConexaoBanco
    {
        public static SqlConnection Abrir()
        {
            var conn = new SqlConnection(AppConfig.ConnectionString);
            conn.Open();
            return conn;
        }
    }
}
