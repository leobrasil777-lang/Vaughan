$(function () {
  iniciarShell('estoque');
  let produtosAtivos = [];

  carregarProdutosSelect();
  carregarHistorico();

  $('#select-categoria').on('change', function () {
    renderizarProdutosSelect();
  });

  $('#btn-registrar').on('click', function () {
    const produtoId = $('#select-produto').val();
    const tipo = $('#select-tipo').val();
    const quantidade = parseInt($('#input-quantidade').val());
    const observacao = $('#input-observacao').val().trim();

    if (!produtoId || !quantidade || quantidade <= 0) {
      mostrarMensagem('Selecione um produto e uma quantidade válida.', 'danger');
      return;
    }

    VaughanApi.post('/api/estoque/movimento', { produtoId: parseInt(produtoId), tipo, quantidade, observacao })
      .done(function () {
        $('#input-quantidade').val(1);
        $('#input-observacao').val('');
        carregarHistorico();
        carregarProdutosSelect();
      })
      .fail(function (xhr) { mostrarMensagem(mensagemErro(xhr, 'Não foi possível registrar.'), 'danger'); });
  });

  function carregarProdutosSelect() {
    const categoriaAnterior = $('#select-categoria').val();
    const produtoAnterior = $('#select-produto').val();

    VaughanApi.get('/api/produtos?ativos=1')
      .done(function (produtos) {
        produtosAtivos = Array.isArray(produtos) ? produtos : [];
        preencherCategorias(categoriaAnterior);
        renderizarProdutosSelect(produtoAnterior);
      })
      .fail(function (xhr) {
        produtosAtivos = [];
        $('#select-categoria').find('option:not(:first)').remove();
        $('#select-produto').empty().append(
          $('<option>').val('').text('Não foi possível carregar os produtos').prop('disabled', true)
        );
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar os produtos.'), 'danger');
      });
  }

  function preencherCategorias(categoriaSelecionada) {
    const select = $('#select-categoria');
    const categorias = [...new Set(produtosAtivos.map(categoriaDoProduto))]
      .sort(function (a, b) {
        return a.localeCompare(b, 'pt-BR', { sensitivity: 'base' });
      });

    select.find('option:not(:first)').remove();
    categorias.forEach(function (categoria) {
      select.append($('<option>').val(categoria).text(categoria));
    });

    if (categoriaSelecionada && categorias.includes(categoriaSelecionada)) {
      select.val(categoriaSelecionada);
    } else {
      select.val('');
    }
  }

  function renderizarProdutosSelect(produtoSelecionado) {
    const categoria = $('#select-categoria').val();
    const produtosFiltrados = produtosAtivos.filter(function (produto) {
      return !categoria || categoriaDoProduto(produto) === categoria;
    });
    const select = $('#select-produto').empty();

    if (produtosFiltrados.length === 0) {
      select.append($('<option>').val('').text('Nenhum produto disponível').prop('disabled', true));
      return;
    }

    produtosFiltrados.forEach(function (produto) {
      const rotulo = produto.nome + ', estoque atual: ' + produto.quantidadeEstoque;
      select.append($('<option>').val(produto.id).text(rotulo));
    });

    if (produtoSelecionado && select.find('option[value="' + produtoSelecionado + '"]').length) {
      select.val(produtoSelecionado);
    }
  }

  function categoriaDoProduto(produto) {
    const categoria = String(produto.categoria || '').trim();
    return categoria || 'Sem categoria';
  }

  function carregarHistorico() {
    const tbody = $('#tbody-historico');
    tbody.html('<tr><td colspan="5" class="text-center py-4 table-state">Carregando...</td></tr>');

    VaughanApi.get('/api/estoque/historico')
      .done(function (movimentos) {
        tbody.empty();
        if (movimentos.length === 0) {
          tbody.append('<tr><td colspan="5" class="text-center py-4 table-state">Nenhuma movimentação registrada.</td></tr>');
          return;
        }

        movimentos.forEach(function (movimento) {
          const entrada = movimento.tipoMovimento === 'Entrada';
          const saida = movimento.tipoMovimento === 'Saida' || movimento.tipoMovimento === 'Saída';
          const tipoTexto = saida ? 'Saída' : movimento.tipoMovimento;
          const tipoClasse = entrada ? 'movement-entry' : (saida ? 'movement-exit' : 'movement-neutral');
          const linha = $('<tr>');

          linha.append($('<td>').addClass('text-nowrap').text(
            new Date(movimento.dataMovimento).toLocaleString('pt-BR')
          ));
          linha.append($('<td>').addClass('movement-product').text(movimento.produtoNome || '-'));
          linha.append($('<td>').append(
            $('<span>').addClass('movement-type ' + tipoClasse).text(tipoTexto || '-')
          ));
          linha.append($('<td>').addClass('text-end').text(movimento.quantidade));
          linha.append($('<td>').text(movimento.observacao || '-'));
          tbody.append(linha);
        });
      })
      .fail(function (xhr) {
        tbody.html('<tr><td colspan="5" class="text-center py-4 table-state">Não foi possível carregar o histórico.</td></tr>');
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar o histórico.'), 'danger');
      });
  }

  function mostrarMensagem(texto, tipo) {
    const alerta = $('<div>').addClass('vb-alert vb-alert-' + tipo).text(texto);
    $('#msg-area').empty().append(alerta);
    setTimeout(function () {
      $('#msg-area').empty();
    }, 4000);
  }
});
