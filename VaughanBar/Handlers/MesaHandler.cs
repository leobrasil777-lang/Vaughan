using VaughanBar.Http;
using VaughanBar.Models;
using VaughanBar.Repositories;

namespace VaughanBar.Handlers
{
    public class StatusMesaRequest
    {
        public string Status { get; set; } = "";
    }

    public static class MesaHandler
    {
        private static readonly MesaRepository _repo = new();

        public static void Listar(RequestContext ctx) => ctx.ResponderJson(_repo.Listar());

        public static void Criar(RequestContext ctx)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            var mesa = ctx.LerCorpo<Mesa>();
            if (mesa == null || mesa.Numero <= 0) { ctx.ResponderErro("Número da mesa é obrigatório."); return; }
            int id = _repo.Inserir(mesa);
            ctx.ResponderJson(new { id }, 201);
        }

        public static void AtualizarStatus(RequestContext ctx, int id)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            var dados = ctx.LerCorpo<StatusMesaRequest>();
            if (dados == null || (dados.Status != "Livre" && dados.Status != "Ocupada"))
            {
                ctx.ResponderErro("Status deve ser 'Livre' ou 'Ocupada'.");
                return;
            }
            bool ok = _repo.AtualizarStatus(id, dados.Status);
            if (!ok) { ctx.ResponderErro("Mesa não encontrada.", 404); return; }
            ctx.ResponderJson(new { ok = true });
        }

        public static void Remover(RequestContext ctx, int id)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            bool ok = _repo.Remover(id);
            if (!ok) { ctx.ResponderErro("Mesa não encontrada.", 404); return; }
            ctx.ResponderJson(new { ok = true });
        }
    }
}
