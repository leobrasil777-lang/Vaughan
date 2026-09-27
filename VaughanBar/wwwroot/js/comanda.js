$(function () {
  iniciarShell('mesas');

  const params = new URLSearchParams(window.location.search);
  const pedidoId = params.get('id');

  if (!pedidoId) {
    window.location.href = 'mesas.html';
    return;
  }

  $('#titulo-comanda-id').text('#' + pedidoId);
  carregarComanda();
  carregarProdutos();

  $('#btn-add-item').on('click', function () {
    const produtoId = $('#select-produto').val();
    const quantidade = parseInt($('#input-quantidade').val());

    if (!produtoId || !quantidade || quantidade <= 0) {
      mostrarMensagem('Selecione um produto e uma quantidade válida.', 'danger');
      return;
    }

    VaughanApi.post(`/api/pedidos/${pedidoId}/itens`, { produtoId: parseInt(produtoId), quantidade: quantidade })
      .done(function () {
        $('#input-quantidade').val(1);
        carregarComanda();
        carregarProdutos();
      })
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível adicionar o item.'), 'danger');
      });
  });

  $('#btn-fechar').on('click', function () {
    if (!confirm('Fechar esta comanda? A mesa será liberada.')) return;
    VaughanApi.post(`/api/pedidos/${pedidoId}/fechar`)
      .done(function () { window.location.href = 'mesas.html'; })
      .fail(function (xhr) { mostrarMensagem(mensagemErro(xhr, 'Não foi possível fechar a comanda.'), 'danger'); });
  });

  $('#btn-cancelar').on('click', function () {
    if (!confirm('Cancelar esta comanda? Esta ação não pode ser desfeita.')) return;
    VaughanApi.post(`/api/pedidos/${pedidoId}/cancelar`)
      .done(function () { window.location.href = 'mesas.html'; })
      .fail(function (xhr) { mostrarMensagem(mensagemErro(xhr, 'Não foi possível cancelar a comanda.'), 'danger'); });
  });

  function carregarComanda() {
    VaughanApi.get(`/api/pedidos/${pedidoId}`).done(function (p) {
      $('#info-mesa').text(p.mesaNumero ? 'Mesa ' + p.mesaNumero : 'Balcão (sem mesa)');
      $('#info-cliente').text(p.clienteNome || 'Não informado');
      $('#info-garcom').text(p.usuarioNome || '-');
      $('#info-status').text(p.status);
      $('#info-data').text(new Date(p.dataPedido).toLocaleString('pt-BR'));
      $('#total-comanda').text(formatarMoeda(p.total));

      const bloqueada = p.status !== 'Aberto';
      $('#btn-add-item, #btn-fechar, #btn-cancelar').prop('disabled', bloqueada);

      const tbody = $('#tbody-itens').empty();
      if (p.itens.length === 0) {
        tbody.append('<tr><td colspan="5" class="text-center py-3" style="color: var(--vb-cream-dim);">Nenhum item lançado ainda.</td></tr>');
        return;
      }
      p.itens.forEach(function (item) {
        const linha = $(`
          <tr>
            <td>${item.produtoNome}</td>
            <td>${item.quantidade}</td>
            <td>${formatarMoeda(item.precoUnitario)}</td>
            <td>${formatarMoeda(item.subtotal)}</td>
            <td>${bloqueada ? '' : '<button class="btn btn-sm btn-outline-amber">Remover</button>'}</td>
          </tr>
        `);
        linha.find('button').on('click', function () {
          VaughanApi.del(`/api/itens/${item.id}`)
            .done(function () { carregarComanda(); carregarProdutos(); })
            .fail(function (xhr) { mostrarMensagem(mensagemErro(xhr, 'Não foi possível remover o item.'), 'danger'); });
        });
        tbody.append(linha);
      });
    });
  }

  function carregarProdutos() {
    VaughanApi.get('/api/produtos?ativos=1').done(function (produtos) {
      const select = $('#select-produto').empty();
      produtos.forEach(function (p) {
        const rotulo = `${p.nome} (${formatarMoeda(p.preco)}) - estoque: ${p.quantidadeEstoque}`;
        select.append($('<option>').val(p.id).text(rotulo).prop('disabled', p.quantidadeEstoque <= 0));
      });
    });
  }

  function mostrarMensagem(texto, tipo) {
    $('#msg-area').html(`<div class="vb-alert vb-alert-${tipo}">${texto}</div>`);
    setTimeout(() => $('#msg-area').empty(), 4000);
  }
});
