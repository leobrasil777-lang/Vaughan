/* =========================================================
   TABELA: Pedidos (ALTERAÇÃO)
   Vínculo com Mesa, Usuário (garçom) e Status da comanda.
   Depende de: Usuarios (01) e Mesas (02) já criadas.
   ========================================================= */
USE VaughanBar;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pedidos') AND name = 'MesaId')
    ALTER TABLE Pedidos ADD MesaId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pedidos') AND name = 'UsuarioId')
    ALTER TABLE Pedidos ADD UsuarioId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pedidos') AND name = 'Status')
    ALTER TABLE Pedidos ADD Status VARCHAR(20) NOT NULL DEFAULT 'Aberto' CHECK (Status IN ('Aberto','Fechado','Cancelado'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Pedidos_Mesas')
    ALTER TABLE Pedidos ADD CONSTRAINT FK_Pedidos_Mesas FOREIGN KEY (MesaId) REFERENCES Mesas(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Pedidos_Usuarios')
    ALTER TABLE Pedidos ADD CONSTRAINT FK_Pedidos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id);
GO

-- Cliente passa a ser opcional (comanda de balcão sem cadastro)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pedidos') AND name = 'ClienteId' AND is_nullable = 0)
    ALTER TABLE Pedidos ALTER COLUMN ClienteId INT NULL;
GO
