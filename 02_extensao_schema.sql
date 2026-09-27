/* =========================================================================
   VAUGHAN BAR - EXTENSÃO DO SCHEMA ORIGINAL
   -------------------------------------------------------------------------
   Execute DEPOIS de:
     1) databasetrabalho.sql   (CREATE DATABASE VaughanBar)
     2) clientes_bar.sql       (tabela Clientes)
     3) produtostrabalho.sql   (tabela Produtos)
     4) pedidostrabalho.sql    (tabela Pedidos)
     5) itenspedidostrabalho.sql (tabela ItensPedido)
     6) os inserts de dados de exemplo do repositório (opcional)

   Este script ADICIONA o que falta para os módulos:
     - Usuários / Login (com cargo Garçom ou Gerente)
     - Mesas / Comandas
     - Estoque (quantidade em Produtos + histórico de movimentações)
     - Vínculo de Pedido com Mesa, Usuário e Status (para comandas/relatórios)
   Nenhuma tabela existente é apagada ou recriada.
   ========================================================================= */

USE VaughanBar;
GO

-- =========================================================
-- USUÁRIOS (login e permissões)
-- =========================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome VARCHAR(100) NOT NULL,
        Login VARCHAR(50) NOT NULL UNIQUE,
        Senha VARCHAR(100) NOT NULL, -- texto puro: uso acadêmico/demo, ver README
        Cargo VARCHAR(20) NOT NULL CHECK (Cargo IN ('Garcom','Gerente')),
        Ativo BIT NOT NULL DEFAULT 1
    );
END
GO

-- =========================================================
-- MESAS
-- =========================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Mesas')
BEGIN
    CREATE TABLE Mesas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Numero INT NOT NULL UNIQUE,
        Capacidade INT NOT NULL DEFAULT 4,
        Status VARCHAR(20) NOT NULL DEFAULT 'Livre' CHECK (Status IN ('Livre','Ocupada'))
    );
END
GO

-- =========================================================
-- ESTOQUE: colunas novas em Produtos
-- =========================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Produtos') AND name = 'QuantidadeEstoque')
    ALTER TABLE Produtos ADD QuantidadeEstoque INT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Produtos') AND name = 'EstoqueMinimo')
    ALTER TABLE Produtos ADD EstoqueMinimo INT NOT NULL DEFAULT 5;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Produtos') AND name = 'Ativo')
    ALTER TABLE Produtos ADD Ativo BIT NOT NULL DEFAULT 1;
GO

-- =========================================================
-- HISTÓRICO DE MOVIMENTAÇÕES DE ESTOQUE (entradas/saídas manuais)
-- =========================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MovimentosEstoque')
BEGIN
    CREATE TABLE MovimentosEstoque (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProdutoId INT NOT NULL,
        TipoMovimento VARCHAR(10) NOT NULL CHECK (TipoMovimento IN ('Entrada','Saida')),
        Quantidade INT NOT NULL,
        DataMovimento DATETIME NOT NULL DEFAULT GETDATE(),
        Observacao VARCHAR(200) NULL,
        CONSTRAINT FK_MovimentosEstoque_Produtos FOREIGN KEY (ProdutoId) REFERENCES Produtos(Id)
    );
END
GO

-- =========================================================
-- PEDIDOS: vínculo com Mesa, Usuário (garçom) e Status da comanda
-- =========================================================
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

-- =========================================================
-- ITENSPEDIDO: guarda o preço praticado no momento da venda
-- =========================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ItensPedido') AND name = 'PrecoUnitario')
    ALTER TABLE ItensPedido ADD PrecoUnitario DECIMAL(10,2) NOT NULL DEFAULT 0;
GO

-- =========================================================
-- DADOS INICIAIS (idempotente)
-- =========================================================
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Login = 'admin')
    INSERT INTO Usuarios (Nome, Login, Senha, Cargo) VALUES ('Administrador', 'admin', 'admin123', 'Gerente');
GO

IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Login = 'garcom1')
    INSERT INTO Usuarios (Nome, Login, Senha, Cargo) VALUES ('João Garçom', 'garcom1', '123456', 'Garcom');
GO

IF NOT EXISTS (SELECT 1 FROM Mesas)
    INSERT INTO Mesas (Numero, Capacidade) VALUES (1,4),(2,4),(3,2),(4,6),(5,4),(6,2);
GO

-- =========================================================
-- BACKFILL (opcional): corrige PrecoUnitario = 0 em itens de
-- pedido inseridos ANTES desta coluna existir (ex: dados de
-- exemplo do repositório original, insertdadosintespedido.sql).
-- Só afeta linhas ainda zeradas, então é seguro rodar de novo.
-- =========================================================
UPDATE ip
SET ip.PrecoUnitario = p.Preco
FROM ItensPedido ip
JOIN Produtos p ON p.Id = ip.ProdutoId
WHERE ip.PrecoUnitario = 0;
GO
