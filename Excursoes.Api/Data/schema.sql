-- Estrutura do banco. Rode como root (ou outro usuário com permissão de criar tabelas):
--   Workbench: File > Open SQL Script > raio
--   PowerShell: Get-Content Excursoes.Api/Data/schema.sql | mysql -u root -p
-- A API conecta com um usuário que NÃO pode criar nem apagar tabelas (veja README).

USE excursoes;

CREATE TABLE Usuarios (
  Id         INT AUTO_INCREMENT PRIMARY KEY,
  Tipo       VARCHAR(20)  NOT NULL,          -- 'Admin' ou 'Passageiro'
  Nome       VARCHAR(100) NOT NULL,
  Email      VARCHAR(150) NOT NULL UNIQUE,
  SenhaHash  VARCHAR(100) NOT NULL,          -- só o hash BCrypt, nunca a senha
  CriadoEm   DATETIME     NOT NULL
);

CREATE TABLE Viagens (
  Id           INT AUTO_INCREMENT PRIMARY KEY,
  AdminId      INT           NOT NULL,
  Titulo       VARCHAR(150)  NOT NULL,
  Destino      VARCHAR(150)  NOT NULL,
  Descricao    VARCHAR(4000) NOT NULL DEFAULT '',
  DataSaida    DATE          NOT NULL,
  DataRetorno  DATE          NOT NULL,
  Valor        DECIMAL(10,2) NOT NULL,
  Vagas        INT           NOT NULL,
  MaxParcelas  INT           NOT NULL,
  Versao       CHAR(36)      NOT NULL,       -- muda a cada alteração: protege a última vaga
  FOREIGN KEY (AdminId) REFERENCES Usuarios(Id)
);

CREATE TABLE ViagemImagens (
  Id        INT AUTO_INCREMENT PRIMARY KEY,
  ViagemId  INT          NOT NULL,
  Url       VARCHAR(500) NOT NULL,
  FOREIGN KEY (ViagemId) REFERENCES Viagens(Id) ON DELETE CASCADE
);

CREATE TABLE Compras (
  Id            INT AUTO_INCREMENT PRIMARY KEY,
  PassageiroId  INT           NOT NULL,
  ViagemId      INT           NOT NULL,
  Metodo        VARCHAR(20)   NOT NULL,
  ValorTotal    DECIMAL(10,2) NOT NULL,
  Status        VARCHAR(20)   NOT NULL,      -- 'Pendente', 'Pago', 'Cancelado'
  CriadoEm      DATETIME      NOT NULL,
  FOREIGN KEY (PassageiroId) REFERENCES Usuarios(Id),
  FOREIGN KEY (ViagemId)     REFERENCES Viagens(Id)   -- sem CASCADE: viagem com compra não pode ser apagada
);

CREATE TABLE Parcelas (
  Id          INT AUTO_INCREMENT PRIMARY KEY,
  CompraId    INT           NOT NULL,
  Numero      INT           NOT NULL,
  Valor       DECIMAL(10,2) NOT NULL,
  Vencimento  DATE          NOT NULL,
  Paga        BOOLEAN       NOT NULL DEFAULT FALSE,
  PagoEm      DATETIME      NULL,
  FOREIGN KEY (CompraId) REFERENCES Compras(Id) ON DELETE CASCADE
);
