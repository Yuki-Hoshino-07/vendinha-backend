# Vendinha

API para cadastro de clientes e controle de compras fiadas, feita em C# com ASP.NET Core 8 e SQLite.

## Executar no Visual Studio

É necessário ter o Visual Studio com a carga de trabalho **Desenvolvimento para ASP.NET e Web**, o SDK e o runtime do .NET 8.

1. Abra `Vendinha.sln`.
2. Aguarde a restauração dos pacotes NuGet.
3. Selecione `Vendinha.Api` como projeto de inicialização.
4. Execute com **F5** ou **Ctrl+F5**.
5. Acesse `http://localhost:5080/swagger` para testar as operações.

No Swagger, escolha uma operação, clique em **Try it out**, preencha os dados e clique em **Execute**. A primeira restauração dos pacotes precisa de internet.

Também é possível executar pelo terminal, na pasta da solução:

```sh
dotnet run --project Vendinha.Api --launch-profile Vendinha.Api
```

## Banco de dados

O arquivo `vendinha.db` é criado automaticamente na pasta `Vendinha.Api` ao executar pelo perfil do projeto. Não é preciso instalar um servidor de banco.

O script de criação está em [database/schema.sql](database/schema.sql) e é executado pela aplicação na inicialização. A conexão pode ser alterada em `Vendinha.Api/appsettings.json`.

## Organização

- `Controllers`: rotas e respostas HTTP.
- `Negocio`: validações e regras de clientes e dívidas.
- `Dados`: conexões, comandos SQL parametrizados e transações.
- `Models`: classes Cliente e Divida.
- `Dtos`: dados de entrada e saída da API.

## Regras principais

O CPF deve ser válido e único. Cada cliente pode ter apenas uma dívida em aberto. Ao pagar, a dívida permanece no histórico e deixa de fazer parte do total devido.

A listagem permite buscar parte do nome, traz dez clientes por página e ordena pelo maior valor em aberto. A idade é calculada pela data de nascimento. Excluir um cliente também exclui suas dívidas.

## Documentação

- [Rotas, exemplos e regras da API](docs/API.md)
- [Roteiro de verificação](docs/VERIFICACAO.md)
- [Exemplos de requisições HTTP](requisicoes.http)
