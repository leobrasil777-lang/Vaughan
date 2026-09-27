$(function () {
  iniciarShell('usuarios');
  const modalUsuario = new bootstrap.Modal(document.getElementById('modalUsuario'));

  carregarUsuarios();

  $('#btn-novo-usuario').on('click', function () {
    limparFormulario();
    $('#tituloModalUsuario').text('Novo usuário');
    $('#dica-senha').text('');
    modalUsuario.show();
  });

  $('#btn-salvar-usuario').on('click', function () {
    const id = $('#usuario-id').val();
    const nome = $('#usuario-nome').val().trim();
    const login = $('#usuario-login').val().trim();
    const senha = $('#usuario-senha').val();
    const cargo = $('#usuario-cargo').val();
    const ativo = $('#usuario-ativo').is(':checked');

    if (!nome || !login || (!id && !senha)) {
      mostrarMensagem('Nome, login e senha são obrigatórios para um novo usuário.', 'danger');
      return;
    }

    const payload = { nome, login, cargo, ativo };
    if (senha) payload.senha = senha;

    const requisicao = id ? VaughanApi.put('/api/usuarios/' + id, payload) : VaughanApi.post('/api/usuarios', payload);
    requisicao
      .done(function () {
        modalUsuario.hide();
        carregarUsuarios();
      })
      .fail(function (xhr) { mostrarMensagem(mensagemErro(xhr, 'Não foi possível salvar.'), 'danger'); });
  });

  function carregarUsuarios() {
    const tbody = $('#tbody-usuarios');
    tbody.html('<tr><td colspan="5" class="text-center py-4 usuarios-table-state">Carregando...</td></tr>');

    VaughanApi.get('/api/usuarios')
      .done(function (usuarios) {
        tbody.empty();
        if (usuarios.length === 0) {
          adicionarEstadoTabela(tbody, 'Nenhum usuário cadastrado.');
          return;
        }

        usuarios.forEach(function (usuario) {
          const linha = $('<tr>');
          const status = $('<span>')
            .addClass('user-status ' + (usuario.ativo ? 'user-status-active' : 'user-status-inactive'))
            .text(usuario.ativo ? 'Ativo' : 'Inativo');
          const acoes = $('<div>').addClass('user-actions');
          const botaoEditar = $('<button>', {
            type: 'button',
            class: 'btn btn-sm vb-btn-outline btn-editar',
            text: 'Editar'
          }).on('click', function () {
            abrirEdicao(usuario);
          });

          acoes.append(botaoEditar);
          if (usuario.ativo) {
            acoes.append(
              $('<button>', {
                type: 'button',
                class: 'btn btn-sm vb-btn-dark btn-remover',
                text: 'Remover'
              }).on('click', function () {
                removerUsuario(usuario);
              })
            );
          } else {
            acoes.append(
              $('<button>', {
                type: 'button',
                class: 'btn btn-sm vb-btn-outline btn-reativar',
                text: 'Reativar'
              }).on('click', function () {
                reativarUsuario(usuario);
              })
            );
          }

          linha.append($('<td>').addClass('user-name').text(usuario.nome));
          linha.append($('<td>').text(usuario.login));
          linha.append($('<td>').text(usuario.cargo === 'Garcom' ? 'Garçom' : usuario.cargo));
          linha.append($('<td>').addClass('text-center').append(status));
          linha.append($('<td>').append(acoes));
          tbody.append(linha);
        });
      })
      .fail(function (xhr) {
        adicionarEstadoTabela(tbody.empty(), 'Não foi possível carregar os usuários.');
        mostrarMensagem(mensagemErro(xhr, 'Acesso restrito ao Gerente.'), 'danger');
      });
  }

  function removerUsuario(usuario) {
    if (!confirm('Remover este usuário?')) return;

    VaughanApi.del('/api/usuarios/' + usuario.id)
      .done(carregarUsuarios)
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível remover.'), 'danger');
      });
  }

  function reativarUsuario(usuario) {
    const payload = {
      nome: usuario.nome,
      login: usuario.login,
      cargo: usuario.cargo,
      ativo: true
    };

    VaughanApi.put('/api/usuarios/' + usuario.id, payload)
      .done(carregarUsuarios)
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível reativar o usuário.'), 'danger');
      });
  }

  function abrirEdicao(u) {
    $('#usuario-id').val(u.id);
    $('#usuario-nome').val(u.nome);
    $('#usuario-login').val(u.login);
    $('#usuario-senha').val('');
    $('#usuario-cargo').val(u.cargo);
    $('#usuario-ativo').prop('checked', u.ativo);
    $('#dica-senha').text('(deixe em branco para manter a atual)');
    $('#tituloModalUsuario').text('Editar usuário');
    modalUsuario.show();
  }

  function limparFormulario() {
    $('#usuario-id').val('');
    $('#usuario-nome').val('');
    $('#usuario-login').val('');
    $('#usuario-senha').val('');
    $('#usuario-cargo').val('Garcom');
    $('#usuario-ativo').prop('checked', true);
  }

  function adicionarEstadoTabela(tbody, mensagem) {
    tbody.append(
      $('<tr>').append(
        $('<td>')
          .attr('colspan', 5)
          .addClass('text-center py-4 usuarios-table-state')
          .text(mensagem)
      )
    );
  }

  function mostrarMensagem(texto, tipo) {
    const alerta = $('<div>').addClass('vb-alert vb-alert-' + tipo).text(texto);
    $('#msg-area').empty().append(alerta);
    setTimeout(function () {
      $('#msg-area').empty();
    }, 4000);
  }
});
