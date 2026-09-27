--consultar produto mais vendido--
SELECT pr.Nome AS Produto,
       SUM(ip.Quantidade) AS TotalVendido
FROM ItensPedido ip
JOIN Produtos pr ON ip.ProdutoId = pr.Id
GROUP BY pr.Nome
ORDER BY TotalVendido DESC;
