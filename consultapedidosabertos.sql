--consultando pedidos abertos e fechados-- 
SELECT Status,
       COUNT(*) AS Total
FROM Pedidos
GROUP BY Status;