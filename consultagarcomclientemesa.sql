use VaughanBar;
--listar pedidos com garcom clientes e mesas--
SELECT p.Id AS Pedido,
       c.Nome AS Cliente,
       m.Numero AS Mesa,
       u.Nome_usuario AS Garcom,
       p.Status,
       p.DataPedido
FROM Pedidos p
LEFT JOIN Clientes c ON p.ClienteId = c.Id
LEFT JOIN Mesas m ON p.MesaId = m.Id
LEFT JOIN Usuarios u ON p.UsuarioId = u.Id;