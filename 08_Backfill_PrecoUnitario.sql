/* =========================================================
   BACKFILL (opcional): ItensPedido.PrecoUnitario
   Corrige PrecoUnitario = 0 em itens de pedido inseridos ANTES
   dessa coluna existir (ex: dados de exemplo do repositório
   original, insertdadosintespedido.sql), copiando o preço
   atual do produto. Só afeta linhas ainda zeradas.
   Depende de: ItensPedido já ter a coluna PrecoUnitario (06).
   ========================================================= */
USE VaughanBar;
GO

UPDATE ip
SET ip.PrecoUnitario = p.Preco
FROM ItensPedido ip
JOIN Produtos p ON p.Id = ip.ProdutoId
WHERE ip.PrecoUnitario = 0;
GO
