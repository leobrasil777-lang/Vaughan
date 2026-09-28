using VaughanBar.Http;
using VaughanBar.Repositories;

namespace VaughanBar.Handlers
{
    public class AbrirComandaRequest
    {
        public int? ClienteId { get; set; }
        public int? MesaId { get; set; }
    }

    public class AdicionarItemRequest
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public static class PedidoHandler
    {
        private static readonly PedidoRepository _repo = new();

        public static void Listar(RequestContext ctx)
        {
            string? status = ctx.QueryParam("status");
            int? mesaId = ctx.QueryParamInt("mesaId");
            ctx.ResponderJson(_repo.Listar(status, mesaId));
        }

        public static void BuscarPorId(RequestContext ctx, int id)
        {
            var pedido = _repo.BuscarPorId(id);
            if (pedido == null) { ctx.ResponderErro("Comanda não encontrada.", 404); return; }
            ctx.ResponderJson(pedido);
        }

        public static void Abrir(RequestContext ctx)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            var dados = ctx.LerCorpo<AbrirComandaRequest>() ?? new AbrirComandaRequest();
            try
            {
                int id = _repo.AbrirComanda(dados.ClienteId, dados.MesaId, usuario.Id);
                ctx.ResponderJson(new { id }, 201);
            }
            catch (Exception ex)
            {
                ctx.ResponderErro(ex.Message);
            }
        }

        public static void AdicionarItem(RequestContext ctx, int pedidoId)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            var dados = ctx.LerCorpo<AdicionarItemRequest>();
            if (dados == null || dados.ProdutoId <= 0 || dados.Quantidade <= 0)
            {
                ctx.ResponderErro("Informe produto e quantidade válidos.");
                return;
            }
            try
            {
                _repo.AdicionarItem(pedidoId, dados.ProdutoId, dados.Quantidade);
                ctx.ResponderJson(new { ok = true });
            }
            catch (Exception ex)
            {
                ctx.ResponderErro(ex.Message);
            }
        }

        public static void RemoverItem(RequestContext ctx, int itemId)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            try
            {
                _repo.RemoverItem(itemId);
                ctx.ResponderJson(new { ok = true });
            }
            catch (Exception ex)
            {
                ctx.ResponderErro(ex.Message);
            }
        }

        public static void Fechar(RequestContext ctx, int pedidoId)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            try
            {
                _repo.FecharComanda(pedidoId);
                ctx.ResponderJson(new { ok = true });
            }
            catch (Exception ex)
            {
                ctx.ResponderErro(ex.Message);
            }
        }

        public static void Cancelar(RequestContext ctx, int pedidoId)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            try
            {
                _repo.CancelarComanda(pedidoId);
                ctx.ResponderJson(new { ok = true });
            }
            catch (InvalidOperationException ex)
            {
                ctx.ResponderErro(ex.Message);
            }
        }
    }
}
