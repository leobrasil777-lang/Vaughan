/* =========================================================
   TABELA: ItensPedido (ALTERAÇÃO)
   Guarda o preço praticado no momento da venda.
   ========================================================= */
USE VaughanBar;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ItensPedido') AND name = 'PrecoUnitario')
    ALTER TABLE ItensPedido ADD PrecoUnitario DECIMAL(10,2) NOT NULL DEFAULT 0;
GO
