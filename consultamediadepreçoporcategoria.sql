--Média de preços por categoria de produto--
SELECT Categoria,
       AVG(Preco) AS PrecoMedio
FROM Produtos
GROUP BY Categoria;