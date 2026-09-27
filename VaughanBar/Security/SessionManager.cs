using System.Collections.Concurrent;
using VaughanBar.Models;

namespace VaughanBar.Security
{
    /// <summary>
    /// Controle de sessões simples, em memória, via token opaco (Guid).
    /// O front-end guarda o token (localStorage) e o envia no header "X-Auth-Token".
    /// </summary>
    public static class SessionManager
    {
        private static readonly ConcurrentDictionary<string, Usuario> _sessoes = new();

        public static string CriarSessao(Usuario usuario)
        {
            string token = Guid.NewGuid().ToString("N");
            _sessoes[token] = usuario;
            return token;
        }

        public static Usuario? ObterUsuario(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            _sessoes.TryGetValue(token, out var usuario);
            return usuario;
        }

        public static void Encerrar(string? token)
        {
            if (string.IsNullOrEmpty(token)) return;
            _sessoes.TryRemove(token, out _);
        }
    }
}
