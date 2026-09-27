using VaughanBar.Http;
using VaughanBar.Repositories;

namespace VaughanBar.Handlers
{
    public static class RelatorioHandler
    {
        private static readonly RelatorioRepository _repo = new();

        public static void Vendas(RequestContext ctx)
        {
            var (inicio, fim) = ObterPeriodo(ctx);
            var resumo = _repo.Resumo(inicio, fim);
            var produtos = _repo.ProdutosMaisVendidos(inicio, fim);
            var usuarios = _repo.VendasPorUsuario(inicio, fim);

            ctx.ResponderJson(new
            {
                periodo = new { inicio, fim },
                resumo,
                produtosMaisVendidos = produtos,
                vendasPorUsuario = usuarios
            });
        }

        private static (DateTime inicio, DateTime fim) ObterPeriodo(RequestContext ctx)
        {
            string? inicioStr = ctx.QueryParam("inicio");
            string? fimStr = ctx.QueryParam("fim");

            DateTime inicio = DateTime.TryParse(inicioStr, out var i) ? i.Date : DateTime.Today.AddDays(-30);
            DateTime fim = DateTime.TryParse(fimStr, out var f) ? f.Date.AddDays(1).AddSeconds(-1) : DateTime.Today.AddDays(1).AddSeconds(-1);
            return (inicio, fim);
        }
    }
}
