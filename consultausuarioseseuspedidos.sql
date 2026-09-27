--consulta usuarios e quantidade de pedidos realizados por cada um --
SELECT u.Nome_usuario AS Garcom,
       COUNT(p.Id) AS TotalPedidos
FROM Pedidos p
JOIN Usuarios u ON p.UsuarioId = u.Id
GROUP BY u.Nome_usuario;
