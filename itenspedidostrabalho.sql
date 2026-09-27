CREATE TABLE ItensPedido (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PedidoId INT NOT NULL,
    ProdutoId INT NOT NULL,
    Quantidade INT NOT NULL,

    FOREIGN KEY (PedidoId)
        REFERENCES Pedidos(Id),

    FOREIGN KEY (ProdutoId)
        REFERENCES Produtos(Id)
);
