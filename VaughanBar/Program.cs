using VaughanBar.Data;
using VaughanBar.Http;

// Permite sobrescrever a string de conexão e a porta via argumentos de linha de comando, ex:
//   dotnet run -- "Server=localhost;Database=VaughanBar;User Id=sa;Password=Senha123;TrustServerCertificate=True;" http://localhost:5050/
if (args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]))
    AppConfig.ConnectionString = args[0];
if (args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]))
    AppConfig.UrlPrefix = args[1];

Console.WriteLine("=== VAUGHAN BAR - Sistema de Gestão ===");
Console.WriteLine($"Connection string: {AppConfig.ConnectionString}");

var server = new HttpServer();
server.Start();
