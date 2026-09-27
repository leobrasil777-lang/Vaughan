using VaughanBar.Http;
using VaughanBar.Repositories;
using VaughanBar.Security;

namespace VaughanBar.Handlers
{
    public class LoginRequest
    {
        public string Login { get; set; } = "";
        public string Senha { get; set; } = "";
    }

    public static class AuthHandler
    {
        private static readonly UsuarioRepository _repo = new();

        public static void Login(RequestContext ctx)
        {
            var dados = ctx.LerCorpo<LoginRequest>();
            if (dados == null || string.IsNullOrWhiteSpace(dados.Login) || string.IsNullOrWhiteSpace(dados.Senha))
            {
                ctx.ResponderErro("Informe login e senha.");
                return;
            }

            var usuario = _repo.Autenticar(dados.Login.Trim(), dados.Senha);
            if (usuario == null)
            {
                ctx.ResponderErro("Login ou senha inválidos.", 401);
                return;
            }

            string token = SessionManager.CriarSessao(usuario);
            ctx.ResponderJson(new
            {
                token,
                usuario = new { usuario.Id, usuario.Nome, usuario.Login, usuario.Cargo }
            });
        }

        public static void Logout(RequestContext ctx)
        {
            string? token = ctx.Request.Headers["X-Auth-Token"];
            SessionManager.Encerrar(token);
            ctx.ResponderJson(new { ok = true });
        }

        public static void Eu(RequestContext ctx)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }
            ctx.ResponderJson(new { usuario.Id, usuario.Nome, usuario.Login, usuario.Cargo });
        }
    }
}
