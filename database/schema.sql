PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS "Clientes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Clientes" PRIMARY KEY AUTOINCREMENT,
    "NomeCompleto" TEXT NOT NULL,
    "Cpf" TEXT NOT NULL,
    "DataNascimento" TEXT NOT NULL,
    "Email" TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Clientes_Cpf" ON "Clientes" ("Cpf");

CREATE TABLE IF NOT EXISTS "Dividas" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Dividas" PRIMARY KEY AUTOINCREMENT,
    "ClienteId" INTEGER NOT NULL,
    "ValorCentavos" INTEGER NOT NULL,
    "Paga" INTEGER NOT NULL,
    "DataCriacao" TEXT NOT NULL,
    "DataPagamento" TEXT NULL,
    CONSTRAINT "CK_Dividas_Valor" CHECK ("ValorCentavos" > 0),
    CONSTRAINT "CK_Dividas_Pagamento" CHECK (
        ("Paga" = 0 AND "DataPagamento" IS NULL) OR
        ("Paga" = 1 AND "DataPagamento" IS NOT NULL)
    ),
    CONSTRAINT "FK_Dividas_Clientes_ClienteId" FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Dividas_ClienteId"
    ON "Dividas" ("ClienteId") WHERE "Paga" = 0;
