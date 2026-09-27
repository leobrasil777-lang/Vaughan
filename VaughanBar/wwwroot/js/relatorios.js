$(function () {
  iniciarShell('relatorios');

  const hoje = new Date();
  const trintaDiasAtras = new Date(hoje);
  trintaDiasAtras.setDate(hoje.getDate() - 30);

  $('#filtro-inicio').val(trintaDiasAtras.toISOString().slice(0, 10));
  $('#filtro-fim').val(hoje.toISOString().slice(0, 10));

  carregarRelatorio();

  $('#btn-filtrar').on('click', carregarRelatorio);

  function carregarRelatorio() {
    const inicio = $('#filtro-inicio').val();
    const fim = $('#filtro-fim').val();
    definirEstadoTabelas('Carregando...');

    VaughanApi.get(`/api/relatorios/vendas?inicio=${inicio}&fim=${fim}`)
      .done(function (dados) {
        $('#resumo-total').text(formatarMoeda(dados.resumo.totalVendido));
        $('#resumo-qtd').text(dados.resumo.quantidadeComandas);
        $('#resumo-ticket').text(formatarMoeda(dados.resumo.ticketMedio));

        renderizarProdutos(dados.produtosMaisVendidos);
        renderizarUsuarios(dados.vendasPorUsuario);
      })
      .fail(function () {
        definirEstadoTabelas('Não foi possível carregar os dados.');
      });
  }

  function renderizarProdutos(produtos) {
    const tbody = $('#tbody-produtos').empty();
    if (produtos.length === 0) {
      adicionarEstadoTabela(tbody, 'Sem dados no período.');
      return;
    }

    produtos.forEach(function (produto) {
      const linha = $('<tr>');
      linha.append($('<td>').addClass('relatorio-primary-cell').text(produto.nome));
      linha.append($('<td>').addClass('text-end').text(produto.quantidadeVendida));
      linha.append($('<td>').addClass('text-end text-nowrap').text(formatarMoeda(produto.totalVendido)));
      tbody.append(linha);
    });
  }

  function renderizarUsuarios(usuarios) {
    const tbody = $('#tbody-usuarios').empty();
    if (usuarios.length === 0) {
      adicionarEstadoTabela(tbody, 'Sem dados no período.');
      return;
    }

    usuarios.forEach(function (usuario) {
      const linha = $('<tr>');
      linha.append($('<td>').addClass('relatorio-primary-cell').text(usuario.usuarioNome));
      linha.append($('<td>').addClass('text-end').text(usuario.quantidadeComandas));
      linha.append($('<td>').addClass('text-end text-nowrap').text(formatarMoeda(usuario.totalVendido)));
      tbody.append(linha);
    });
  }

  function definirEstadoTabelas(mensagem) {
    adicionarEstadoTabela($('#tbody-produtos').empty(), mensagem);
    adicionarEstadoTabela($('#tbody-usuarios').empty(), mensagem);
  }

  function adicionarEstadoTabela(tbody, mensagem) {
    tbody.append(
      $('<tr>').append(
        $('<td>')
          .attr('colspan', 3)
          .addClass('text-center py-4 relatorio-table-state')
          .text(mensagem)
      )
    );
  }
});
