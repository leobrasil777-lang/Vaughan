create table movimentosestoque (
Id INT IDENTITY(1,1) primary key,
ProdutoId INT NOT NULL,
TipoMovimento VARCHAR(10) NOT NULL CHECK (TipoMovimento = 'Entrada' or TipoMovimento = 'Saida'),
Quantidade INT NOT NULL,
DataMovimento DATETIME NOT NULL DEFAULT GETDATE(),
Observacao VARCHAR(200) NULL,
 CONSTRAINT FK_MovimentosEstoque_Produtos FOREIGN KEY (ProdutoId) REFERENCES Produtos(Id)
);