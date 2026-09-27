/* =========================================================
   DADOS INICIAIS (idempotente)
   Depende de: Usuarios (01) e Mesas (02) já criadas.
   ========================================================= */
USE VaughanBar;
GO

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Login = 'admin')
    INSERT INTO Usuarios (Nome, Login, Senha, Cargo) VALUES ('Administrador', 'admin', 'admin123', 'Gerente');
GO

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Login = 'garcom1')
    INSERT INTO Usuarios (Nome, Login, Senha, Cargo) VALUES ('João Garçom', 'garcom1', '123456', 'Garcom');
GO

IF NOT EXISTS (SELECT 1 FROM Mesas)
    INSERT INTO Mesas (Numero, Capacidade) VALUES (1,4),(2,4),(3,2),(4,6),(5,4),(6,2);
GO
