--valor total por pedido--
SELECT p.Id AS Pedido,
       SUM(ip.Quantidade * ip.PrecoUnitario) AS ValorTotal
FROM Pedidos p
JOIN ItensPedido ip ON p.Id = ip.PedidoId
GROUP BY p.Id;