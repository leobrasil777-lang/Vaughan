-- consulta itens de cada pedido com preço total--
SELECT ip.PedidoId,
       pr.Nome AS Produto,
       ip.Quantidade,
       ip.PrecoUnitario,
       (ip.Quantidade * ip.PrecoUnitario) AS TotalItem
FROM ItensPedido ip
JOIN Produtos pr ON ip.ProdutoId = pr.Id;
