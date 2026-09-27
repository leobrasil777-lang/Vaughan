using System.Net;
using System.Text;
using System.Text.Json;
using VaughanBar.Models;
using VaughanBar.Security;

namespace VaughanBar.Http
{
    /// <summary>
    /// Encapsula um HttpListenerContext com utilitários para ler JSON do corpo,
    /// query string, segmentos de rota e escrever respostas JSON padronizadas.
    /// </summary>
    public class RequestContext
    {
        public HttpListenerContext Raw { get; }
        public HttpListenerRequest Request => Raw.Request;
        public HttpListenerResponse Response => Raw.Response;
        public string[] Segmentos { get; }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public RequestContext(HttpListenerContext ctx)
        {
            Raw = ctx;
            var path = ctx.Request.Url?.AbsolutePath ?? "/";
            Segmentos = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        public T? LerCorpo<T>()
        {
            using var reader = new StreamReader(Request.InputStream, Request.ContentEncoding ?? Encoding.UTF8);
            string body = reader.ReadToEnd();
            if (string.IsNullOrWhiteSpace(body)) return default;
            return JsonSerializer.Deserialize<T>(body, JsonOpts);
        }

        public string? QueryParam(string nome) => Request.QueryString[nome];

        public int? QueryParamInt(string nome)
        {
            var v = QueryParam(nome);
            return int.TryParse(v, out var i) ? i : null;
        }

        public Usuario? UsuarioAutenticado()
        {
            string? token = Request.Headers["X-Auth-Token"];
            return SessionManager.ObterUsuario(token);
        }

        public void ResponderJson(object dados, int statusCode = 200)
        {
            Response.StatusCode = statusCode;
            Response.ContentType = "application/json; charset=utf-8";
            string json = JsonSerializer.Serialize(dados, JsonOpts);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            Response.ContentLength64 = bytes.Length;
            Response.OutputStream.Write(bytes, 0, bytes.Length);
            Response.OutputStream.Close();
        }

        public void ResponderErro(string mensagem, int statusCode = 400)
            => ResponderJson(new { erro = mensagem }, statusCode);
    }
}
