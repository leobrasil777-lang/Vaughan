using System.Net;
using VaughanBar.Data;

namespace VaughanBar.Http
{
    /// <summary>
    /// Servidor HTTP construído diretamente sobre System.Net.HttpListener (BCL do .NET),
    /// sem nenhum framework web. Cada requisição é atendida em uma thread do ThreadPool.
    /// </summary>
    public class HttpServer
    {
        private readonly HttpListener _listener = new();

        public void Start()
        {
            _listener.Prefixes.Add(AppConfig.UrlPrefix);
            _listener.Start();
            Console.WriteLine($"Vaughan Bar rodando em {AppConfig.UrlPrefix}");
            Console.WriteLine("Pressione Ctrl+C para encerrar.");

            while (true)
            {
                HttpListenerContext context = _listener.GetContext(); // bloqueia até chegar requisição
                ThreadPool.QueueUserWorkItem(_ => ProcessarRequisicao(context));
            }
        }

        private void ProcessarRequisicao(HttpListenerContext httpContext)
        {
            try
            {
                // CORS liberado (útil se o front-end for aberto de outra origem/porta em testes)
                httpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
                httpContext.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, X-Auth-Token");
                httpContext.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

                if (httpContext.Request.HttpMethod == "OPTIONS")
                {
                    httpContext.Response.StatusCode = 204;
                    httpContext.Response.OutputStream.Close();
                    return;
                }

                var ctx = new RequestContext(httpContext);

                bool tratadoComoApi = Router.TratarApi(ctx);
                if (tratadoComoApi) return;

                bool servido = StaticFileServer.TentarServir(httpContext);
                if (!servido)
                {
                    httpContext.Response.StatusCode = 404;
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes("404 - Não encontrado");
                    httpContext.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    httpContext.Response.OutputStream.Close();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    httpContext.Response.StatusCode = 500;
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Erro interno: " + ex.Message);
                    httpContext.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    httpContext.Response.OutputStream.Close();
                }
                catch { /* conexão já pode ter sido fechada pelo cliente */ }
            }
        }
    }
}
