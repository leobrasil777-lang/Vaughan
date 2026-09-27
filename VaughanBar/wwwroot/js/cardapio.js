$(function () {
  iniciarShell('cardapio');
  const modalProduto = new bootstrap.Modal(document.getElementById('modalProduto'));
  let produtoEmEdicao = null;

  carregarProdutos();

  $('#btn-novo-produto').on('click', function () {
    limparFormulario();
    $('#tituloModalProduto').text('Novo produto');
    $('#campo-estoque-inicial').show();
    modalProduto.show();
  });

  $('#btn-salvar-produto').on('click', function () {
    const id = $('#produto-id').val();
    const nome = $('#produto-nome').val().trim();
    const preco = parseFloat($('#produto-preco').val());
    const categoria = $('#produto-categoria').val().trim();
    const estoqueMinimo = parseInt($('#produto-estoque-minimo').val()) || 0;

    if (!nome || !preco || preco <= 0) {
      mostrarMensagem('Informe nome e preço válidos.', 'danger');
      return;
    }

    const ativo = id && produtoEmEdicao ? produtoEmEdicao.ativo : true;
    const payload = { nome, preco, categoria, estoqueMinimo, ativo };

    if (id) {
      VaughanApi.put('/api/produtos/' + id, payload)
        .done(function () {
          modalProduto.hide();
          carregarProdutos();
        })
        .fail(function (xhr) { mostrarMensagem(mensagemErro(xhr, 'Não foi possível salvar.'), 'danger'); });
    } else {
      payload.quantidadeEstoque = parseInt($('#produto-estoque').val()) || 0;
      VaughanApi.post('/api/produtos', payload)
        .done(function () {
          modalProduto.hide();
          carregarProdutos();
        })
        .fail(function (xhr) { mostrarMensagem(mensagemErro(xhr, 'Não foi possível salvar.'), 'danger'); });
    }
  });

  function carregarProdutos() {
    const tbody = $('#tbody-produtos');
    tbody.html('<tr><td colspan="6" class="text-center py-4 table-state">Carregando...</td></tr>');

    VaughanApi.get('/api/produtos')
      .done(function (produtos) {
        tbody.empty();
        if (produtos.length === 0) {
          tbody.append('<tr><td colspan="6" class="text-center py-4 table-state">Nenhum produto cadastrado.</td></tr>');
          return;
        }

        produtos.forEach(function (p) {
          const estoqueBaixo = p.quantidadeEstoque <= p.estoqueMinimo;
          const estoque = '<div class="product-stock">' +
            '<span>' + p.quantidadeEstoque + '</span>' +
            (estoqueBaixo ? '<span class="stock-low-label">Estoque baixo</span>' : '') +
            '</div>';
          const status = p.ativo
            ? '<span class="product-status product-status-active">Ativo</span>'
            : '<span class="product-status product-status-inactive">Inativo</span>';
          const acaoDisponivel = p.ativo
            ? '<button type="button" class="btn btn-sm vb-btn-dark btn-desativar">Desativar</button>'
            : '<button type="button" class="btn btn-sm vb-btn-outline btn-reativar">Reativar</button>';
          const linha = $(
            '<tr>' +
              '<td class="product-name">' + escaparHtml(p.nome) + '</td>' +
              '<td>' + escaparHtml(p.categoria || '-') + '</td>' +
              '<td class="text-nowrap">' + formatarMoeda(p.preco) + '</td>' +
              '<td>' + estoque + '</td>' +
              '<td class="text-center">' + status + '</td>' +
              '<td><div class="product-actions">' +
                '<button type="button" class="btn btn-sm vb-btn-outline btn-editar">Editar</button>' +
                acaoDisponivel +
              '</div></td>' +
            '</tr>'
          );

          linha.find('.btn-editar').on('click', function () {
            abrirEdicao(p);
          });
          linha.find('.btn-desativar').on('click', function () {
            desativarProduto(p);
          });
          linha.find('.btn-reativar').on('click', function () {
            reativarProduto(p);
          });
          tbody.append(linha);
        });
      })
      .fail(function (xhr) {
        tbody.html('<tr><td colspan="6" class="text-center py-4 table-state">Não foi possível carregar os produtos.</td></tr>');
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível carregar os produtos.'), 'danger');
      });
  }

  function abrirEdicao(p) {
    produtoEmEdicao = p;
    $('#produto-id').val(p.id);
    $('#produto-nome').val(p.nome);
    $('#produto-categoria').val(p.categoria || '');
    $('#produto-preco').val(p.preco);
    $('#produto-estoque-minimo').val(p.estoqueMinimo);
    $('#campo-estoque-inicial').hide();
    $('#tituloModalProduto').text('Editar produto');
    modalProduto.show();
  }

  function desativarProduto(p) {
    if (!confirm('Desativar este produto?')) return;

    VaughanApi.del('/api/produtos/' + p.id)
      .done(carregarProdutos)
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível desativar o produto.'), 'danger');
      });
  }

  function reativarProduto(p) {
    const payload = {
      nome: p.nome,
      preco: p.preco,
      categoria: p.categoria,
      quantidadeEstoque: p.quantidadeEstoque,
      estoqueMinimo: p.estoqueMinimo,
      ativo: true
    };

    VaughanApi.put('/api/produtos/' + p.id, payload)
      .done(carregarProdutos)
      .fail(function (xhr) {
        mostrarMensagem(mensagemErro(xhr, 'Não foi possível reativar o produto.'), 'danger');
      });
  }

  function limparFormulario() {
    produtoEmEdicao = null;
    $('#produto-id').val('');
    $('#produto-nome').val('');
    $('#produto-categoria').val('');
    $('#produto-preco').val('');
    $('#produto-estoque').val(0);
    $('#produto-estoque-minimo').val(5);
  }

  function escaparHtml(valor) {
    return String(valor == null ? '' : valor)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  function mostrarMensagem(texto, tipo) {
    const alerta = $('<div>').addClass('vb-alert vb-alert-' + tipo).text(texto);
    $('#msg-area').empty().append(alerta);
    setTimeout(function () {
      $('#msg-area').empty();
    }, 4000);
  }
});
