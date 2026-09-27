/* Carrega a sidebar comum, valida sessão e marca o link ativo. */
function iniciarShell(paginaAtual) {
  if (!VaughanApi.token()) {
    window.location.href = 'index.html';
    return;
  }

  $('#sidebar-placeholder').load('partials/sidebar.html', function () {
    const usuario = JSON.parse(localStorage.getItem('vb_usuario') || '{}');
    $('#sidebar-usuario-nome').text(usuario.nome || '');
    $('#sidebar-usuario-cargo').text(usuario.cargo || '');

    if (usuario.cargo !== 'Gerente') {
      $('#nav-usuarios').remove();
    }

    $('.sidebar-nav a[data-page="' + paginaAtual + '"]').addClass('active');

    $('#btn-logout').on('click', function () {
      VaughanApi.post('/api/logout').always(function () {
        localStorage.removeItem('vb_token');
        localStorage.removeItem('vb_usuario');
        window.location.href = 'index.html';
      });
    });
  });
}
