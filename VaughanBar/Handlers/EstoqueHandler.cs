using VaughanBar.Http;
using VaughanBar.Repositories;

namespace VaughanBar.Handlers
{
    public class MovimentoRequest
    {
        public int ProdutoId { get; set; }
        public string Tipo { get; set; } = ""; // Entrada | Saida
        public int Quantidade { get; set; }
        public string? Observacao { get; set; }
    }

    public static class EstoqueHandler
    {
        private static readonly EstoqueRepository _repo = new();

        public static void Historico(RequestContext ctx)
        {
            int? produtoId = ctx.QueryParamInt("produtoId");
            ctx.ResponderJson(_repo.Historico(produtoId));
        }

        public static void EstoqueBaixo(RequestContext ctx)
            => ctx.ResponderJson(_repo.ProdutosComEstoqueBaixo());

        public static void RegistrarMovimento(RequestContext ctx)
        {
            var usuario = ctx.UsuarioAutenticado();
            if (usuario == null) { ctx.ResponderErro("Não autenticado.", 401); return; }

            var dados = ctx.LerCorpo<MovimentoRequest>();
            if (dados == null || dados.ProdutoId <= 0 || dados.Quantidade <= 0)
            {
                ctx.ResponderErro("Informe produto, tipo e quantidade válidos.");
                return;
            }
            try
            {
                _repo.RegistrarMovimento(dados.ProdutoId, dados.Tipo, dados.Quantidade, dados.Observacao);
                ctx.ResponderJson(new { ok = true });
            }
            catch (Exception ex)
            {
                ctx.ResponderErro(ex.Message);
            }
        }
    }
}
