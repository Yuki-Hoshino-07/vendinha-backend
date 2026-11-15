# Documentação da API

Endereço local: `http://localhost:5080`. Requisições com corpo usam `Content-Type: application/json`.

## Rotas

| Método | Rota | Operação |
|---|---|---|
| GET | `/api/clientes?nome=ana&pagina=1` | Buscar e listar clientes |
| GET | `/api/clientes/{id}` | Consultar um cliente |
| POST | `/api/clientes` | Cadastrar cliente |
| PUT | `/api/clientes/{id}` | Atualizar todos os dados cadastrais |
| DELETE | `/api/clientes/{id}` | Excluir cliente e suas dívidas |
| GET | `/api/clientes/{clienteId}/dividas` | Consultar histórico de dívidas |
| POST | `/api/clientes/{clienteId}/dividas` | Cadastrar dívida |
| PATCH | `/api/clientes/{clienteId}/dividas/{dividaId}/pagamento` | Marcar dívida como paga |

## Clientes

Corpo para `POST` e `PUT`:

```json
{
  "nomeCompleto": "Ana Souza",
  "cpf": "12345678909",
  "dataNascimento": "2000-05-10",
  "email": "ana@example.com"
}
```

Dados ilustrativos para testes. O CPF também pode ser enviado como `123.456.789-09`; o banco e as respostas usam somente os onze números. A validação verifica o formato e os dígitos, sem consultar a Receita Federal ou confirmar a existência de uma pessoa.

| Campo | Regra |
|---|---|
| `nomeCompleto` | Obrigatório, não pode ser vazio ou só espaços; até 150 caracteres |
| `cpf` | Obrigatório, onze dígitos válidos, com ou sem a máscara padrão; único |
| `dataNascimento` | Obrigatória, válida, diferente de `0001-01-01` e não futura |
| `email` | Opcional; até 254 caracteres e formato válido se preenchido |

E-mail omitido, `null`, vazio ou formado somente por espaços é armazenado como `null`. No `PUT`, envie novamente nome, CPF e nascimento; omitir o e-mail remove o e-mail anterior.

Resposta de um cliente (idade ilustrativa para 15/11/2026):

```json
{
  "id": 1,
  "nomeCompleto": "Ana Souza",
  "cpf": "12345678909",
  "dataNascimento": "2000-05-10",
  "idade": 26,
  "email": "ana@example.com",
  "totalDividas": 35.90
}
```

`idade` é recalculada em cada resposta. `totalDividas` soma somente valores ainda não pagos.

### Listagem e busca

`GET /api/clientes?nome=ana&pagina=1`

```json
{
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalClientes": 1,
  "totalPaginas": 1,
  "clientes": [
    {
      "id": 1,
      "nomeCompleto": "Ana Souza",
      "cpf": "12345678909",
      "dataNascimento": "2000-05-10",
      "idade": 26,
      "email": "ana@example.com",
      "totalDividas": 35.90
    }
  ]
}
```

- `nome` é opcional e procura texto contido no nome, removendo espaços nas extremidades da busca.
- Maiúsculas e minúsculas de A a Z são equivalentes. Acentos são preservados; o tratamento de maiúsculas acentuadas segue o `lower()` padrão do SQLite, sem normalização Unicode adicional.
- `pagina` começa em 1; o tamanho é sempre 10. Valores de página inválidos retornam 400.
- O filtro é aplicado antes da contagem e da paginação.
- A ordenação é pelo saldo não pago, em ordem decrescente. Empates são resolvidos pelo menor ID.
- Página além da última retorna `clientes: []`. Sem resultados, `totalClientes` e `totalPaginas` são zero.

## Dívidas

Corpo para `POST /api/clientes/1/dividas`:

```json
{
  "valor": 35.90
}
```

Valor obrigatório entre 0,01 e 999.999.999,99, com no máximo duas casas decimais. No JSON, use ponto como separador decimal. A aplicação define a situação e as datas; esses campos não são recebidos do cliente.

Resposta:

```json
{
  "id": 1,
  "clienteId": 1,
  "valor": 35.90,
  "paga": false,
  "dataCriacao": "2026-11-15T15:00:00Z",
  "dataPagamento": null
}
```

Uma segunda dívida para o mesmo cliente só pode ser cadastrada depois do pagamento da anterior. Vários registros pagos são permitidos.

`GET /api/clientes/1/dividas` retorna uma lista desses objetos, da data de criação mais recente para a mais antiga, incluindo dívidas pagas. Cliente existente sem dívidas retorna `[]`; cliente inexistente retorna 404.

### Pagamento

Envie `PATCH /api/clientes/1/dividas/1/pagamento` **sem corpo**. A resposta é a dívida com `paga: true` e `dataPagamento` preenchida em UTC. A dívida precisa pertencer ao cliente indicado na rota.

O pagamento é integral. Repetir a chamada retorna a dívida paga sem alterar a data registrada. Não há rota para desfazer pagamento.

## Respostas HTTP

| Código | Significado |
|---|---|
| 200 | Consulta, atualização ou pagamento concluído |
| 201 | Cliente ou dívida criado; inclui cabeçalho `Location` |
| 204 | Cliente excluído; sem corpo na resposta |
| 400 | Dados inválidos, obrigatórios ausentes ou JSON incorreto |
| 404 | Cliente inexistente ou dívida não encontrada para o cliente |
| 409 | CPF duplicado ou cliente já possui dívida em aberto |

Conflitos e recursos não encontrados retornam uma mensagem:

```json
{
  "mensagem": "Este cliente já possui uma dívida em aberto."
}
```

As validações de negócio também retornam 400 com um objeto `mensagem`, explicando o primeiro problema encontrado. Erros de leitura do JSON, tipos incompatíveis e datas com formato inválido usam a resposta padrão do ASP.NET Core, com `status`, `title` e `errors`. As rotas e os campos de sucesso permanecem os mesmos da primeira versão.

## Exclusão

`DELETE /api/clientes/1` remove o cliente **e todo o histórico de dívidas associado**, inclusive dívidas em aberto. Não há recuperação pela API. Essa decisão permite o CRUD completo; o enunciado não define restrição para exclusão de clientes com dívida.
