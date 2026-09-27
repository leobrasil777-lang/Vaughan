using VaughanBar.Http;
using VaughanBar.Models;
using VaughanBar.Repositories;

namespace VaughanBar.Handlers
{
    public static class UsuarioHandler
    {
        private static readonly UsuarioRepository _repo = new();

        private static bool ExigirGerente(RequestContext ctx, out Usuario? usuario)
        {
            usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return false; }
            if (usuario.Cargo != "Gerente") { ctx.ResponderErro("Acesso restrito ao Gerente.", 403); return false; }
            return true;
        }

        public static void Listar(RequestContext ctx)
        {
            if (!ExigirGerente(ctx, out _)) return;
            ctx.ResponderJson(_repo.Listar());
        }

        public static void Criar(RequestContext ctx)
        {
            if (!ExigirGerente(ctx, out _)) return;

            var usuario = ctx.LerCorpo<Usuario>();
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.Nome) ||
                string.IsNullOrWhiteSpace(usuario.Login) || string.IsNullOrWhiteSpace(usuario.Senha) ||
                (usuario.Cargo != "Garcom" && usuario.Cargo != "Gerente"))
            {
                ctx.ResponderErro("Nome, login, senha e cargo (Garcom/Gerente) são obrigatórios.");
                return;
            }
            int id = _repo.Inserir(usuario);
            ctx.ResponderJson(new { id }, 201);
        }

        public static void Atualizar(RequestContext ctx, int id)
        {
            if (!ExigirGerente(ctx, out _)) return;

            var usuario = ctx.LerCorpo<Usuario>();
            if (usuario == null) { ctx.ResponderErro("Dados inválidos."); return; }
            usuario.Id = id;
            bool ok = _repo.Atualizar(usuario);
            if (!ok) { ctx.ResponderErro("Usuário não encontrado.", 404); return; }
            ctx.ResponderJson(new { ok = true });
        }

        public static void Remover(RequestContext ctx, int id)
        {
            if (!ExigirGerente(ctx, out _)) return;

            bool ok = _repo.Remover(id);
            if (!ok) { ctx.ResponderErro("Usuário não encontrado.", 404); return; }
            ctx.ResponderJson(new { ok = true });
        }
    }
}
