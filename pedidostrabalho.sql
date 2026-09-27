CREATE TABLE Pedidos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClienteId INT NOT NULL,
    DataPedido DATETIME NOT NULL,

    FOREIGN KEY (ClienteId)
        REFERENCES Clientes(Id)
);

go