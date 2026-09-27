namespace VaughanBar.Data
{
    /// <summary>
    /// Configurações centrais da aplicação.
    /// Ajuste ConnectionString conforme o seu SQL Server local.
    /// </summary>
    public static class AppConfig
    {
        // Exemplo para SQL Server local com autenticação do Windows:
        //   "Server=localhost;Database=VaughanBar;Trusted_Connection=True;TrustServerCertificate=True;"
        // Exemplo com usuário/senha do SQL Server:
        //   "Server=localhost;Database=VaughanBar;User Id=sa;Password=SuaSenha123;TrustServerCertificate=True;"
        public static string ConnectionString { get; set; } =
            "Server=localhost;Database=VaughanBar;Trusted_Connection=True;TrustServerCertificate=True;";

        // Prefixo em que o HttpListener vai escutar.
        // "http://localhost:5050/" funciona sem precisar de privilégio de administrador.
        public static string UrlPrefix { get; set; } = "http://localhost:5050/";

        // Pasta com os arquivos estáticos do front-end (HTML/CSS/JS)
        public static string WwwRootPath { get; set; } =
            Path.Combine(AppContext.BaseDirectory, "wwwroot");
    }
}
