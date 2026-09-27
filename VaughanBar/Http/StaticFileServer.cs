using System.Net;
using VaughanBar.Data;

namespace VaughanBar.Http
{
    public static class StaticFileServer
    {
        private static readonly Dictionary<string, string> MimeTypes = new()
        {
            [".html"] = "text/html; charset=utf-8",
            [".css"] = "text/css; charset=utf-8",
            [".js"] = "application/javascript; charset=utf-8",
            [".json"] = "application/json; charset=utf-8",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".svg"] = "image/svg+xml",
            [".ico"] = "image/x-icon"
        };

        public static bool TentarServir(HttpListenerContext ctx)
        {
            string path = ctx.Request.Url?.AbsolutePath ?? "/";
            if (path == "/") path = "/index.html";

            // Impede path traversal (../)
            string relativo = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string caminhoCompleto = Path.GetFullPath(Path.Combine(AppConfig.WwwRootPath, relativo));
            if (!caminhoCompleto.StartsWith(Path.GetFullPath(AppConfig.WwwRootPath)))
                return false;

            if (!File.Exists(caminhoCompleto))
                return false;

            string ext = Path.GetExtension(caminhoCompleto).ToLowerInvariant();
            ctx.Response.ContentType = MimeTypes.TryGetValue(ext, out var mime) ? mime : "application/octet-stream";

            byte[] bytes = File.ReadAllBytes(caminhoCompleto);
            ctx.Response.ContentLength64 = bytes.Length;
            ctx.Response.StatusCode = 200;
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            ctx.Response.OutputStream.Close();
            return true;
        }
    }
}
