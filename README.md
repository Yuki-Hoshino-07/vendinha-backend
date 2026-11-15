# Vendinha

API para cadastrar clientes e controlar compras fiadas de uma vendinha.
Desenvolvida em C# com ASP.NET Core 8, Entity Framework Core e SQLite.

## Como executar no Visual Studio

1. Tenha o Visual Studio com a carga de trabalho **Desenvolvimento para ASP.NET e Web** e o SDK do **.NET 8** (ou um SDK mais recente que compile projetos .NET 8). O runtime do .NET 8 também precisa estar instalado.
2. Abra o arquivo `Vendinha.sln`.
3. Aguarde a restauração dos pacotes NuGet. Na primeira vez, é necessária conexão com a internet.
4. Confira se `Vendinha.Api` é o projeto de inicialização e se o perfil selecionado é `Vendinha.Api`.
5. Pressione **F5** ou **Ctrl+F5**.
6. O navegador abrirá o Swagger em `http://localhost:5080/swagger`.

No Swagger, expanda uma operação, clique em **Try it out**, preencha os dados e clique em **Execute**. Os exemplos estão em [docs/API.md](docs/API.md). O projeto é um backend: o Swagger permite testar as operações, mas não é uma tela de atendimento da loja.

O banco `vendinha.db` é criado automaticamente na pasta `Vendinha.Api` ao executar pelo perfil do projeto. Não é necessário instalar um servidor de banco de dados. Os dados continuam salvos após encerrar a aplicação.

### Pelo terminal, como alternativa

Na pasta da solução:

```sh
dotnet restore Vendinha.sln
dotnet build Vendinha.sln
dotnet run --project Vendinha.Api --launch-profile Vendinha.Api
```

Abra `http://localhost:5080/swagger`. Para encerrar, pressione `Ctrl+C` no terminal.

## Funcionalidades

- Cadastro, consulta, atualização e exclusão de clientes.
- Validação dos dígitos verificadores do CPF e bloqueio de CPF duplicado.
- Nome, CPF e data de nascimento obrigatórios; e-mail opcional, validado quando preenchido.
- Idade calculada a cada consulta.
- Busca por trecho do nome e paginação fixa de dez clientes.
- Clientes ordenados do maior saldo em aberto para o menor.
- Cadastro e histórico de dívidas por cliente.
- Registro de pagamento e bloqueio de uma segunda dívida em aberto.

## Organização

```text
Vendinha.sln
Vendinha.Api/
  Controllers/       Rotas de clientes e dívidas
  Data/              Configuração do Entity Framework e das tabelas
  Dtos/              Dados recebidos e devolvidos pela API
  Models/            Classes Cliente e Divida
  Validation/        Validação do CPF
  Properties/        Perfil de execução do Visual Studio
  Program.cs         Inicialização da aplicação
  appsettings.json   Conexão com o banco e configuração de logs
database/
  schema.sql         Criação das tabelas e dos índices
docs/
  API.md             Rotas, exemplos e regras
  VERIFICACAO.md     Roteiro de conferência
requisicoes.http     Exemplos para executar no Visual Studio
```

## Banco de dados

A conexão está em `Vendinha.Api/appsettings.json`. O caminho relativo do arquivo é resolvido a partir da pasta de execução.

O script [database/schema.sql](database/schema.sql) cria a mesma estrutura usada pela aplicação. É possível executá-lo em um editor SQLite sobre um banco vazio e apontar a conexão para esse arquivo. Se preferir usar o terminal do SQLite, entre na pasta `Vendinha.Api`, abra `sqlite3 vendinha.db` e execute:

```sql
.read ../database/schema.sql
```

O comando `.read` pertence ao terminal do SQLite, não à linguagem SQL. O uso desse terminal é opcional, pois a aplicação cria o banco com `EnsureCreated()`.

Há duas tabelas: `Clientes` e `Dividas`, ligadas por `ClienteId`. O CPF possui índice único. Um índice único parcial permite somente uma dívida não paga por cliente. Ao excluir um cliente, suas dívidas também são excluídas.

Os valores são armazenados em centavos inteiros para preservar os centavos nas somas e na ordenação. Na API, os valores são números decimais em reais, como `35.90`.

`EnsureCreated()` cria um banco novo, mas não atualiza automaticamente a estrutura de um banco que já existe. Este projeto não utiliza migrations.

## Decisões adotadas

- `totalDividas` representa a soma das dívidas **não pagas**. Dívidas pagas permanecem no histórico e não entram no saldo nem na ordenação.
- Uma dívida nasce não paga, com data de criação gerada pela aplicação. O pagamento quita o valor integral e registra a data do servidor em UTC.
- Repetir o pagamento de uma dívida já paga mantém a data original.
- Datas de nascimento usam `AAAA-MM-DD`. A idade considera a data local do servidor e não é armazenada no banco.
- O contrato JSON está descrito em `docs/API.md`, usando os campos do enunciado, que não apresenta um exemplo separado de JSON.
- Não há autenticação, pagamento parcial, juros ou interface de loja, pois essas funcionalidades não fazem parte do escopo.

## Conferência

Consulte [docs/VERIFICACAO.md](docs/VERIFICACAO.md) e use `requisicoes.http` ou o Swagger. O arquivo `.http` pode ser aberto no Visual Studio com suporte ao editor de requisições HTTP; o Swagger funciona independentemente desse editor.

Se a porta 5080 estiver ocupada, altere `applicationUrl` em `Properties/launchSettings.json` e use a nova porta nos exemplos.

## Referências

- [Validação de modelos no ASP.NET Core](https://learn.microsoft.com/aspnet/core/mvc/models/validation?view=aspnetcore-8.0).
- [Provedor SQLite do Entity Framework Core](https://learn.microsoft.com/ef/core/providers/sqlite/).
