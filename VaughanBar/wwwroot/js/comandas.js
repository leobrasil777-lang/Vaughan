$(function () {
  iniciarShell('comandas');

  const modalVerElemento = document.getElementById('modalVerComanda');
  const modalNovaElemento = document.getElementById('modalNovaComanda');
  const modalVer = new bootstrap.Modal(modalVerElemento);
  const modalNova = new bootstrap.Modal(modalNovaElemento);
  let comandas = [];

  carregarComandas();
  carregarMesas();
  carregarClientes();

  $('#busca-comandas').on('input', function () {
    renderizarComandas($(this).val());
  });

  $('#btn-nova-comanda').on('click', function () {
    modalNova.show();
  });

  $('#btn-confirmar-nova-comanda').on('click', function () {
    const mesaId = $('#nova-comanda-mesa').val();
    const clienteId = $('#nova-comanda-cliente').val();
    const payload = {};

    if (mesaId) payload.mesaId = parseInt(mesaId);
    if (clienteId) payload.clienteId = parseInt(clienteId);

    const botao = $(this).prop('disabled', true);

    VaughanApi.post('/api/pedidos', payload)
      .done(function (resposta) {
        $(modalNovaElemento).one('hidden.bs.modal', function () {
          abrirDetalhesComanda(resposta.id);
        });
        modalNova.hide();
        carregarComandas();
      })
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível abrir a comanda.'), 'danger');
      })
      .always(function () {
        botao.prop('disabled', false);
      });
  });

  $(modalNovaElemento).on('hidden.bs.modal', function () {
    $('#nova-comanda-mesa').val('');
    $('#nova-comanda-cliente').val('');
  });

  function carregarComandas() {
    const tbody = $('#tbody-comandas');
    tbody.html('<tr><td colspan="6" class="text-center py-4 table-state">Carregando...</td></tr>');

    return VaughanApi.get('/api/pedidos')
      .done(function (pedidos) {
        comandas = Array.isArray(pedidos) ? pedidos : [];
        renderizarComandas($('#busca-comandas').val());
      })
      .fail(function (xhr) {
        comandas = [];
        tbody.html('<tr><td colspan="6" class="text-center py-4 table-state">Não foi possível carregar as comandas.</td></tr>');
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar as comandas.'), 'danger');
      });
  }

  function renderizarComandas(termoBusca) {
    const termo = normalizarTexto(termoBusca);
    const filtradas = comandas.filter(function (pedido) {
      if (!termo) return true;

      const mesa = pedido.mesaNumero ? 'Mesa ' + pedido.mesaNumero : 'Balcão';
      const cliente = pedido.clienteNome || 'Não informado';
      const conteudo = [
        pedido.id,
        '#' + pedido.id,
        pedido.mesaNumero || '',
        mesa,
        cliente
      ].join(' ');

      return normalizarTexto(conteudo).includes(termo);
    });

    const tbody = $('#tbody-comandas').empty();

    if (filtradas.length === 0) {
      const mensagem = termo ? 'Nenhuma comanda encontrada para esta busca.' : 'Nenhuma comanda cadastrada.';
      tbody.append($('<tr>').append(
        $('<td>').attr('colspan', 6).addClass('text-center py-4 table-state').text(mensagem)
      ));
      return;
    }

    filtradas.forEach(function (pedido) {
      const mesa = pedido.mesaNumero ? 'Mesa ' + pedido.mesaNumero : 'Balcão';
      const cliente = pedido.clienteNome || 'Não informado';
      const linha = $('<tr>');
      const botaoVer = $('<button>', {
        type: 'button',
        class: 'btn btn-sm btn-dark btn-ver-comanda',
        text: 'Ver'
      }).on('click', function () {
        abrirDetalhesComanda(pedido.id);
      });

      linha.append($('<td>').addClass('comanda-id').text('#' + pedido.id));
      linha.append($('<td>').text(mesa));
      linha.append($('<td>').text(cliente));
      linha.append($('<td>').append(criarStatus(pedido.status)));
      linha.append($('<td>').addClass('comanda-value').text(formatarMoeda(pedido.total)));
      linha.append($('<td>').append(botaoVer));
      tbody.append(linha);
    });
  }

  function abrirDetalhesComanda(id) {
    $('#titulo-modal-comanda').text('Comanda #' + id);
    $('#detalhe-mesa, #detalhe-cliente').text('-');
    $('#detalhe-status').empty();
    $('#detalhe-total').text(formatarMoeda(0));
    $('#tbody-detalhe-itens').html(
      '<tr><td colspan="4" class="text-center py-4 table-state">Carregando...</td></tr>'
    );
    modalVer.show();

    VaughanApi.get('/api/pedidos/' + id)
      .done(function (pedido) {
        $('#titulo-modal-comanda').text('Comanda #' + pedido.id);
        $('#detalhe-mesa').text(pedido.mesaNumero ? 'Mesa ' + pedido.mesaNumero : 'Balcão');
        $('#detalhe-cliente').text(pedido.clienteNome || 'Não informado');
        $('#detalhe-status').empty().append(criarStatus(pedido.status));
        $('#detalhe-total').text(formatarMoeda(pedido.total));
        renderizarItens(pedido.itens || []);
      })
      .fail(function (xhr) {
        $('#tbody-detalhe-itens').html(
          '<tr><td colspan="4" class="text-center py-4 table-state">Não foi possível carregar esta comanda.</td></tr>'
        );
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar esta comanda.'), 'danger');
      });
  }

  function renderizarItens(itens) {
    const tbody = $('#tbody-detalhe-itens').empty();

    if (itens.length === 0) {
      tbody.append($('<tr>').append(
        $('<td>').attr('colspan', 4).addClass('text-center py-4 table-state').text('Nenhum item lançado.')
      ));
      return;
    }

    itens.forEach(function (item) {
      const linha = $('<tr>');
      linha.append($('<td>').text(item.produtoNome || 'Item'));
      linha.append($('<td>').text(item.quantidade));
      linha.append($('<td>').text(formatarMoeda(item.precoUnitario)));
      linha.append($('<td>').text(formatarMoeda(item.subtotal)));
      tbody.append(linha);
    });
  }

  function carregarMesas() {
    VaughanApi.get('/api/mesas')
      .done(function (mesas) {
        const select = $('#nova-comanda-mesa');
        select.find('option:not(:first)').remove();
        mesas.forEach(function (mesa) {
          select.append($('<option>').val(mesa.id).text('Mesa ' + mesa.numero));
        });
      })
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar as mesas.'), 'danger');
      });
  }

  function carregarClientes() {
    VaughanApi.get('/api/clientes')
      .done(function (clientes) {
        const select = $('#nova-comanda-cliente');
        select.find('option:not(:first)').remove();
        clientes.forEach(function (cliente) {
          select.append($('<option>').val(cliente.id).text(cliente.nome));
        });
      })
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar os clientes.'), 'danger');
      });
  }

  function criarStatus(status) {
    let classe = 'status-cancelado';
    if (status === 'Aberto') classe = 'status-aberto';
    if (status === 'Fechado') classe = 'status-fechado';

    return $('<span>').addClass('status-tag ' + classe).text(status || 'Não informado');
  }

  function normalizarTexto(valor) {
    return String(valor || '')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }

  function mostrarMensagem(texto, tipo) {
    const alerta = $('<div>').addClass('vb-alert vb-alert-' + tipo).text(texto);
    $('#msg-area').empty().append(alerta);
    setTimeout(function () {
      $('#msg-area').empty();
    }, 4000);
  }
});
