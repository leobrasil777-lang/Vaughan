create table Mesas (
Id INT IDENTITY(1,1) primary key,
Numero INT NOT NULL UNIQUE,
Capacidade INT NOT NULL DEFAULT 4,
Status VARCHAR(20) NOT NULL DEFAULT 'Livre'
CHECK (status = 'Livre' or status= 'Ocupada')
);