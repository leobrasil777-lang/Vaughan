/* Wrapper simples sobre $.ajax (jQuery) para falar com a API do Vaughan Bar. */
const VaughanApi = (function () {
  function token() {
    return localStorage.getItem('vb_token');
  }

  function request(method, url, dados) {
    return $.ajax({
      url: url,
      method: method,
      contentType: 'application/json',
      headers: token() ? { 'X-Auth-Token': token() } : {},
      data: dados !== undefined ? JSON.stringify(dados) : undefined
    }).fail(function (xhr) {
      if (xhr.status === 401) {
        localStorage.removeItem('vb_token');
        localStorage.removeItem('vb_usuario');
        if (!window.location.pathname.endsWith('index.html') && window.location.pathname !== '/') {
          window.location.href = 'index.html';
        }
      }
    });
  }

  return {
    get: (url) => request('GET', url),
    post: (url, dados) => request('POST', url, dados),
    put: (url, dados) => request('PUT', url, dados),
    del: (url) => request('DELETE', url),
    token: token
  };
})();

/* Extrai a mensagem de erro de uma resposta com falha (jqXHR) */
function mensagemErro(xhr, padrao) {
  try {
    const corpo = JSON.parse(xhr.responseText);
    return corpo.erro || padrao;
  } catch (e) {
    return padrao;
  }
}

/* Formata número como moeda BRL */
function formatarMoeda(valor) {
  return (valor || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}
