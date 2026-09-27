using VaughanBar.Handlers;

namespace VaughanBar.Http
{
    /// <summary>
    /// Roteamento manual e explícito das rotas de API.
    /// Nenhum framework de rotas é usado: apenas comparação de método HTTP + segmentos de path.
    /// </summary>
    public static class Router
    {
        public static bool TratarApi(RequestContext ctx)
        {
            var seg = ctx.Segmentos; // ex: ["api","produtos","5"]
            string metodo = ctx.Request.HttpMethod.ToUpperInvariant();

            if (seg.Length == 0 || seg[0] != "api") return false;

            try
            {
                // ---------- AUTH ----------
                if (Match(seg, "api", "login") && metodo == "POST") { AuthHandler.Login(ctx); return true; }
                if (Match(seg, "api", "logout") && metodo == "POST") { AuthHandler.Logout(ctx); return true; }
                if (Match(seg, "api", "eu") && metodo == "GET") { AuthHandler.Eu(ctx); return true; }

                // ---------- PRODUTOS (Cardápio / Estoque base) ----------
                if (Match(seg, "api", "produtos") && metodo == "GET") { ProdutoHandler.Listar(ctx); return true; }
                if (Match(seg, "api", "produtos") && metodo == "POST") { ProdutoHandler.Criar(ctx); return true; }
                if (MatchComId(seg, "api", "produtos", out int idProd) && metodo == "PUT") { ProdutoHandler.Atualizar(ctx, idProd); return true; }
                if (MatchComId(seg, "api", "produtos", out idProd) && metodo == "DELETE") { ProdutoHandler.Remover(ctx, idProd); return true; }

                // ---------- MESAS ----------
                if (Match(seg, "api", "mesas") && metodo == "GET") { MesaHandler.Listar(ctx); return true; }
                if (Match(seg, "api", "mesas") && metodo == "POST") { MesaHandler.Criar(ctx); return true; }
                if (seg.Length == 4 && seg[0] == "api" && seg[1] == "mesas" && seg[3] == "status" &&
                    int.TryParse(seg[2], out int idMesaStatus) && metodo == "PUT")
                {
                    MesaHandler.AtualizarStatus(ctx, idMesaStatus);
                    return true;
                }
                if (MatchComId(seg, "api", "mesas", out int idMesa) && metodo == "DELETE") { MesaHandler.Remover(ctx, idMesa); return true; }

                // ---------- CLIENTES ----------
                if (Match(seg, "api", "clientes") && metodo == "GET") { ClienteHandler.Listar(ctx); return true; }
                if (Match(seg, "api", "clientes") && metodo == "POST") { ClienteHandler.Criar(ctx); return true; }
                if (MatchComId(seg, "api", "clientes", out int idCli) && metodo == "PUT") { ClienteHandler.Atualizar(ctx, idCli); return true; }
                if (MatchComId(seg, "api", "clientes", out idCli) && metodo == "DELETE") { ClienteHandler.Remover(ctx, idCli); return true; }

                // ---------- PEDIDOS / COMANDAS ----------
                if (Match(seg, "api", "pedidos") && metodo == "GET") { PedidoHandler.Listar(ctx); return true; }
                if (Match(seg, "api", "pedidos") && metodo == "POST") { PedidoHandler.Abrir(ctx); return true; }
                if (MatchComId(seg, "api", "pedidos", out int idPed) && metodo == "GET") { PedidoHandler.BuscarPorId(ctx, idPed); return true; }
                if (seg.Length == 4 && seg[0] == "api" && seg[1] == "pedidos" && seg[3] == "itens" && metodo == "POST")
                {
                    PedidoHandler.AdicionarItem(ctx, int.Parse(seg[2]));
                    return true;
                }
                if (seg.Length == 3 && seg[0] == "api" && seg[1] == "itens" && metodo == "DELETE")
                {
                    PedidoHandler.RemoverItem(ctx, int.Parse(seg[2]));
                    return true;
                }
                if (seg.Length == 4 && seg[0] == "api" && seg[1] == "pedidos" && seg[3] == "fechar" && metodo == "POST")
                {
                    PedidoHandler.Fechar(ctx, int.Parse(seg[2]));
                    return true;
                }
                if (seg.Length == 4 && seg[0] == "api" && seg[1] == "pedidos" && seg[3] == "cancelar" && metodo == "POST")
                {
                    PedidoHandler.Cancelar(ctx, int.Parse(seg[2]));
                    return true;
                }

                // ---------- ESTOQUE ----------
                if (Match(seg, "api", "estoque", "historico") && metodo == "GET") { EstoqueHandler.Historico(ctx); return true; }
                if (Match(seg, "api", "estoque", "baixo") && metodo == "GET") { EstoqueHandler.EstoqueBaixo(ctx); return true; }
                if (Match(seg, "api", "estoque", "movimento") && metodo == "POST") { EstoqueHandler.RegistrarMovimento(ctx); return true; }

                // ---------- RELATÓRIOS ----------
                if (Match(seg, "api", "relatorios", "vendas") && metodo == "GET") { RelatorioHandler.Vendas(ctx); return true; }

                // ---------- USUÁRIOS ----------
                if (Match(seg, "api", "usuarios") && metodo == "GET") { UsuarioHandler.Listar(ctx); return true; }
                if (Match(seg, "api", "usuarios") && metodo == "POST") { UsuarioHandler.Criar(ctx); return true; }
                if (MatchComId(seg, "api", "usuarios", out int idUsr) && metodo == "PUT") { UsuarioHandler.Atualizar(ctx, idUsr); return true; }
                if (MatchComId(seg, "api", "usuarios", out idUsr) && metodo == "DELETE") { UsuarioHandler.Remover(ctx, idUsr); return true; }

                ctx.ResponderErro("Rota de API não encontrada.", 404);
                return true;
            }
            catch (Exception ex)
            {
                ctx.ResponderErro("Erro interno: " + ex.Message, 500);
                return true;
            }
        }

        private static bool Match(string[] seg, params string[] esperado)
        {
            if (seg.Length != esperado.Length) return false;
            for (int i = 0; i < esperado.Length; i++)
                if (!string.Equals(seg[i], esperado[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            return true;
        }

        private static bool MatchComId(string[] seg, string a, string b, out int id)
        {
            id = 0;
            if (seg.Length != 3 || seg[0] != a || seg[1] != b) return false;
            return int.TryParse(seg[2], out id);
        }
    }
}
