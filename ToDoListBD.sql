CREATE DATABASE ToDoList

USE ToDoList

CREATE TABLE utilizador(
    ID int IDENTITY(1,1),
    nome VARCHAR(100) NOT NULL,
    username VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    hashPassword VARCHAR(255) NOT NULL,
    fotografia VARCHAR(255),
	linguagem VARCHAR(5) NOT NULL DEFAULT('en-US'),
	PRIMARY KEY(ID)
);

CREATE TABLE tarefa(
    ID int IDENTITY(1,1),
    titulo VARCHAR(100) NOT NULL,
    descricao VARCHAR(100) NOT NULL,
    dataInicio DATE NOT NULL DEFAULT(GETDATE()),
    dataFim DATE NOT NULL DEFAULT(GETDATE()),
    horaInicio TIME NOT NULL DEFAULT(GETDATE()),
    horaFim TIME NOT NULL DEFAULT(GETDATE()),
    NivelImportancia INT NOT NULL,
    Estado INT NOT NULL
	PRIMARY KEY(ID)
);

CREATE TABLE alerta(
    ID int IDENTITY(1,1),
    mensagem VARCHAR(100) NOT NULL,
    data_hora DATETIME NOT NULL DEFAULT(getdate()),
    desligado BIT NOT NULL DEFAULT(0),
    tipoEmail BIT NOT NULL DEFAULT(0),
    tipoWindows BIT NOT NULL DEFAULT(1),
	PRIMARY KEY(ID)
);

CREATE TABLE listaPersonalizada(
    ID int IDENTITY(1,1),
    nomeLista VARCHAR(55) NOT NULL ,
	PRIMARY KEY(ID)
);

CREATE TABLE diaTarefa(
    ID int IDENTITY(1,1),
    dataDia DATE NOT NULL,
	ativo BIT NOT NULL DEFAULT(1),
	PRIMARY KEY(ID)
);

CREATE TABLE periodicidade(
    ID int IDENTITY(1,1),
    diasSemana VARCHAR(255) NOT NULL,
	tipo INT NOT NULL,
	PRIMARY KEY(ID)
);

CREATE TABLE Possuir(
    ID_utilizador int NOT NULL,
    ID_tarefa int NOT NULL,
	PRIMARY KEY(ID_tarefa),
	FOREIGN KEY (ID_tarefa) REFERENCES tarefa(ID),
	FOREIGN KEY (ID_utilizador) REFERENCES utilizador(ID)
);

CREATE TABLE Conter(
    ID_utilizador int NOT NULL,
    ID_listaPersonalizada int NOT NULL,
	PRIMARY KEY(ID_listaPersonalizada),
	FOREIGN KEY (ID_listaPersonalizada) REFERENCES listaPersonalizada(ID),
	FOREIGN KEY (ID_utilizador) REFERENCES utilizador(ID)
);

CREATE TABLE Tarefa_Alerta(
    ID_tarefa int NOT NULL,
    ID_alerta int NOT NULL,
	PRIMARY KEY(ID_alerta),
	FOREIGN KEY (ID_alerta) REFERENCES alerta(ID),
	FOREIGN KEY (ID_tarefa) REFERENCES tarefa(ID)
);

CREATE TABLE Tarefa_DiaTarefa(
    ID_tarefa int NOT NULL,
    ID_diaTarefa int NOT NULL,
	PRIMARY KEY(ID_diaTarefa),
	FOREIGN KEY (ID_tarefa) REFERENCES tarefa(ID),
	FOREIGN KEY (ID_diaTarefa) REFERENCES diaTarefa(ID)
);

CREATE TABLE Tarefa_Periodicidade(
    ID_tarefa int NOT NULL,
    ID_periodicidade int NOT NULL,
	PRIMARY KEY(ID_tarefa),
	FOREIGN KEY (ID_tarefa) REFERENCES tarefa(ID),
	FOREIGN KEY (ID_periodicidade) REFERENCES periodicidade(ID)
);

CREATE TABLE ListaPersonalizada_Tarefa(
    ID_tarefa int NOT NULL,
    ID_listaPersonalizada int NOT NULL,
	PRIMARY KEY(ID_tarefa),
	FOREIGN KEY (ID_tarefa) REFERENCES tarefa(ID),
	FOREIGN KEY (ID_listaPersonalizada) REFERENCES listaPersonalizada(ID)
);
