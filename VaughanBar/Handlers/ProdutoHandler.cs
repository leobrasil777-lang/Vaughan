using VaughanBar.Http;
using VaughanBar.Models;
using VaughanBar.Repositories;

namespace VaughanBar.Handlers
{
    public static class ProdutoHandler
    {
        private static readonly ProdutoRepository _repo = new();

        public static void Listar(RequestContext ctx)
        {
            bool somenteAtivos = ctx.QueryParam("ativos") == "1";
            ctx.ResponderJson(_repo.Listar(somenteAtivos));
        }

        public static void Criar(RequestContext ctx)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            var produto = ctx.LerCorpo<Produto>();
            if (produto == null || string.IsNullOrWhiteSpace(produto.Nome) || produto.Preco <= 0)
            {
                ctx.ResponderErro("Nome e preço (maior que zero) são obrigatórios.");
                return;
            }
            int id = _repo.Inserir(produto);
            ctx.ResponderJson(new { id }, 201);
        }

        public static void Atualizar(RequestContext ctx, int id)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            var produto = ctx.LerCorpo<Produto>();
            if (produto == null) { ctx.ResponderErro("Dados inválidos."); return; }
            produto.Id = id;
            bool ok = _repo.Atualizar(produto);
            if (!ok) { ctx.ResponderErro("Produto não encontrado.", 404); return; }
            ctx.ResponderJson(new { ok = true });
        }

        public static void Remover(RequestContext ctx, int id)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            bool ok = _repo.Remover(id);
            if (!ok) { ctx.ResponderErro("Produto não encontrado.", 404); return; }
            ctx.ResponderJson(new { ok = true });
        }
    }
}
