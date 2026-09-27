create table Usuarios (
Id INT IDENTITY(1,1) primary key,
Nome_usuario VARCHAR (100) NOT NULL,
Login VARCHAR(100) NOT NULL UNIQUE,
Senha_usuario VARCHAR (100) NOT NULL,
Cargo VARCHAR (20) NOT NULL CHECK (Cargo = 'Garcom' OR Cargo = 'Gerente'),
Ativo BIT NOT NULL DEFAULT 1
)
;
