using VaughanBar.Http;
using VaughanBar.Models;
using VaughanBar.Repositories;

namespace VaughanBar.Handlers
{
    public static class ClienteHandler
    {
        private static readonly ClienteRepository _repo = new();

        public static void Listar(RequestContext ctx) => ctx.ResponderJson(_repo.Listar());

        public static void Criar(RequestContext ctx)
        {
            var cliente = ctx.LerCorpo<Cliente>();
            if (cliente == null || string.IsNullOrWhiteSpace(cliente.Nome))
            {
                ctx.ResponderErro("Nome é obrigatório.");
                return;
            }
            int id = _repo.Inserir(cliente);
            ctx.ResponderJson(new { id }, 201);
        }

        public static void Atualizar(RequestContext ctx, int id)
        {
            var cliente = ctx.LerCorpo<Cliente>();
            if (cliente == null) { ctx.ResponderErro("Dados inválidos."); return; }
            cliente.Id = id;
            bool ok = _repo.Atualizar(cliente);
            if (!ok) { ctx.ResponderErro("Cliente não encontrado.", 404); return; }
            ctx.ResponderJson(new { ok = true });
        }

        public static void Remover(RequestContext ctx, int id)
        {
            bool ok = _repo.Remover(id);
            if (!ok) { ctx.ResponderErro("Cliente não encontrado.", 404); return; }
            ctx.ResponderJson(new { ok = true });
        }
    }
}
