$(function () {
  iniciarShell('mesas');

  const modalAbrirComanda = bootstrap.Modal.getOrCreateInstance(document.getElementById('modalAbrirComanda'));
  const modalNovaMesa = bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNovaMesa'));

  let mesas = [];
  let filtroStatus = 'Todas';
  let termoBusca = '';
  let temporizadorMensagem;

  carregarMesas();
  carregarClientesNoSelect();

  $('#btn-nova-mesa').on('click', function () {
    limparFormularioNovaMesa();
    modalNovaMesa.show();
    document.getElementById('modalNovaMesa').addEventListener('shown.bs.modal', function focarNumero() {
      $('#nova-mesa-numero').trigger('focus');
    }, { once: true });
  });

  $('#busca-mesas').on('input', function () {
    termoBusca = $(this).val().trim();
    renderizarMesas();
  });

  $('.mesa-filter-button').on('click', function () {
    filtroStatus = $(this).data('status');
    $('.mesa-filter-button').removeClass('active').attr('aria-pressed', 'false');
    $(this).addClass('active').attr('aria-pressed', 'true');
    renderizarMesas();
  });

  $('#btn-salvar-mesa').on('click', function () {
    const numero = Number($('#nova-mesa-numero').val());
    const capacidade = Number($('#nova-mesa-capacidade').val());

    if (!Number.isInteger(numero) || numero <= 0 || !Number.isInteger(capacidade) || capacidade <= 0) {
      mostrarMensagem('Informe o número e a capacidade da mesa usando valores inteiros maiores que zero.', 'danger');
      return;
    }

    const botao = $(this).prop('disabled', true).text('Salvando...');
    const payload = {
      numero: numero,
      capacidade: capacidade,
      status: 'Livre'
    };

    VaughanApi.post('/api/mesas', payload)
      .done(function () {
        modalNovaMesa.hide();
        limparFormularioNovaMesa();
        mostrarMensagem('Mesa cadastrada com sucesso.', 'success');
        carregarMesas();
      })
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível cadastrar a mesa.'), 'danger');
      })
      .always(function () {
        botao.prop('disabled', false).text('Salvar');
      });
  });

  $('#btn-confirmar-abrir').on('click', function () {
    const mesaId = $('#modal-mesa-id').val();
    const clienteId = $('#modal-cliente').val();

    if (!mesaId) {
      mostrarMensagem('Selecione uma mesa livre para abrir a comanda.', 'danger');
      return;
    }

    const payload = { mesaId: parseInt(mesaId, 10) };
    if (clienteId) payload.clienteId = parseInt(clienteId, 10);

    VaughanApi.post('/api/pedidos', payload)
      .done(function (resp) {
        window.location.href = 'comanda.html?id=' + resp.id;
      })
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível abrir a comanda.'), 'danger');
      });
  });

  function carregarClientesNoSelect() {
    VaughanApi.get('/api/clientes')
      .done(function (clientes) {
        const select = $('#modal-cliente').empty();
        select.append($('<option>').val('').text('Sem cliente cadastrado'));
        clientes.forEach(function (cliente) {
          select.append($('<option>').val(cliente.id).text(cliente.nome));
        });
      })
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar os clientes.'), 'danger');
      });
  }

  function carregarMesas() {
    VaughanApi.get('/api/mesas')
      .done(function (dados) {
        mesas = Array.isArray(dados) ? dados : [];
        atualizarResumos();
        renderizarMesas();
      })
      .fail(function (xhr) {
        mesas = [];
        atualizarResumos();
        renderizarMesas();
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar as mesas.'), 'danger');
      });
  }

  function atualizarResumos() {
    const livres = mesas.filter(function (mesa) { return mesa.status === 'Livre'; }).length;
    const ocupadas = mesas.filter(function (mesa) { return mesa.status === 'Ocupada'; }).length;

    $('#resumo-total-mesas').text(mesas.length);
    $('#resumo-mesas-livres').text(livres);
    $('#resumo-mesas-ocupadas').text(ocupadas);
  }

  function renderizarMesas() {
    const grid = $('#grid-mesas').empty();
    const filtradas = mesas.filter(function (mesa) {
      const correspondeStatus = filtroStatus === 'Todas' || mesa.status === filtroStatus;
      const correspondeBusca = String(mesa.numero).toLocaleLowerCase('pt-BR').includes(termoBusca.toLocaleLowerCase('pt-BR'));
      return correspondeStatus && correspondeBusca;
    });

    if (filtradas.length === 0) {
      grid.append($('<div>').addClass('col-12').append($('<p>').addClass('mesas-empty-state mb-0').text('Nenhuma mesa encontrada.')));
      return;
    }

    filtradas.forEach(function (mesa) {
      const ocupada = mesa.status === 'Ocupada';
      const coluna = $('<div>').addClass('col-12 col-sm-6 col-lg-4');
      const card = $('<article>').addClass('mesa-card ' + (ocupada ? 'mesa-ocupada' : 'mesa-livre'));
      const cabecalho = $('<div>').addClass('mesa-card-header');
      const titulo = $('<h3>').addClass('mesa-numero mb-0').text('Mesa ' + mesa.numero);
      const status = $('<span>')
        .addClass('mesa-status-badge ' + (ocupada ? 'status-ocupada' : 'status-livre'))
        .text(ocupada ? 'Ocupada' : 'Livre');
      const botao = $('<button>')
        .attr('type', 'button')
        .addClass('btn vb-btn-dark mesa-action-button')
        .text(ocupada ? 'Ver comanda' : 'Abrir comanda');

      botao.on('click', function () {
        if (ocupada) {
          abrirComandaDaMesa(mesa.id);
          return;
        }

        $('#modal-mesa-id').val(mesa.id);
        $('#modal-cliente').val('');
        $('#tituloModalAbrir').text('Abrir comanda - Mesa ' + mesa.numero);
        modalAbrirComanda.show();
      });

      cabecalho.append(titulo, status);
      card.append(cabecalho, botao);
      coluna.append(card);
      grid.append(coluna);
    });
  }

  function abrirComandaDaMesa(mesaId) {
    VaughanApi.get('/api/pedidos?status=Aberto&mesaId=' + encodeURIComponent(mesaId))
      .done(function (pedidos) {
        if (Array.isArray(pedidos) && pedidos.length > 0) {
          window.location.href = 'comanda.html?id=' + pedidos[0].id;
          return;
        }

        mostrarMensagem('Mesa marcada como ocupada, mas nenhuma comanda em aberto foi encontrada.', 'danger');
      })
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível localizar a comanda desta mesa.'), 'danger');
      });
  }

  function limparFormularioNovaMesa() {
    $('#nova-mesa-numero, #nova-mesa-capacidade').val('');
  }

  function mostrarMensagem(texto, tipo) {
    clearTimeout(temporizadorMensagem);
    const alerta = $('<div>').addClass('vb-alert vb-alert-' + tipo).attr('role', 'alert').text(texto);
    $('#msg-area').empty().append(alerta);
    temporizadorMensagem = setTimeout(function () {
      $('#msg-area').empty();
    }, 4000);
  }
});
