$(function () {
  // Se já está logado, vai direto para as mesas
  if (VaughanApi.token()) {
    window.location.href = 'mesas.html';
    return;
  }

  $('#form-login').on('submit', function (e) {
    e.preventDefault();
    $('#login-erro').addClass('d-none');

    const login = $('#login').val().trim();
    const senha = $('#senha').val();

    if (!login || !senha) {
      $('#login-erro').removeClass('d-none').text('Informe usuário e senha.');
      return;
    }

    VaughanApi.post('/api/login', { login: login, senha: senha })
      .done(function (resposta) {
        localStorage.setItem('vb_token', resposta.token);
        localStorage.setItem('vb_usuario', JSON.stringify(resposta.usuario));
        window.location.href = 'mesas.html';
      })
      .fail(function (xhr) {
        $('#login-erro').removeClass('d-none').text(mensagemErro(xhr, 'Não foi possível entrar.'));
      });
  });
});
