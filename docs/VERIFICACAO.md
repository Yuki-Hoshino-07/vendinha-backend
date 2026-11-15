# Roteiro de verificação

Execute a API e use o Swagger ou `requisicoes.http`. Use apenas dados fictícios. Anote os IDs retornados ao criar registros.

| Conferência | Resultado esperado |
|---|---|
| Cadastrar cliente com nome, CPF válido e nascimento | 201, com ID e idade calculada |
| Cadastrar cliente sem e-mail | 201, e-mail `null` |
| Omitir nome, CPF ou nascimento | 400 |
| Enviar CPF `11111111111` ou dígito incorreto | 400 |
| Enviar nascimento futuro ou e-mail inválido | 400 |
| Repetir CPF usando outra formatação | 409 |
| Atualizar cliente com seus próprios dados | 200 |
| Atualizar para o CPF de outro cliente | 409 |
| Consultar, atualizar ou excluir ID inexistente | 404 |
| Cadastrar dívida de 35,90 | 201, `paga: false`, data de pagamento nula |
| Enviar dívida com zero, negativo ou três casas decimais | 400 |
| Cadastrar outra dívida enquanto há uma em aberto | 409 |
| Consultar cliente após cadastrar dívida | `totalDividas: 35.90` |
| Pagar a dívida | 200, `paga: true`, data de pagamento preenchida |
| Consultar cliente após pagar | `totalDividas: 0` |
| Repetir pagamento | Mesma data de pagamento |
| Pagar dívida usando outro cliente na rota | 404 |
| Cadastrar nova dívida após pagar a anterior | 201 |
| Listar histórico | Dívida paga e dívida nova aparecem |
| Cadastrar mais de dez clientes e consultar páginas 1 e 2 | Dez na primeira; restantes na segunda, sem repetir IDs |
| Comparar clientes com valores pendentes diferentes | Maior saldo aparece primeiro |
| Buscar parte do nome | Apenas nomes que contêm o texto |
| Buscar texto inexistente ou página além da última | Lista vazia |
| Informar página zero | 400 |
| Reiniciar a aplicação | Cadastros e pagamentos permanecem |
| Excluir cliente | 204; cliente e dívidas deixam de existir |
| Criar banco pelo script SQL e executar a API sobre ele | Mesmas operações e restrições |

Na conferência da versão com SQL direto, a solução compilou sem avisos ou erros. Foram aprovadas 143 verificações: as 100 de API e banco, mais 43 de parâmetros, datas, transações e compatibilidade. Isso inclui requisições simultâneas para CPF duplicado e dívida em aberto, persistência após reiniciar e uso do banco criado pelo script SQL.

Também foram conferidos nomes com apóstrofos e texto semelhante a SQL, buscas literais por `%` e `_`, idade antes e depois do aniversário, valores monetários mínimo e máximo e leitura e atualização de uma cópia do banco da versão anterior.

Para verificar o rollback, foi simulada uma falha durante o pagamento em um banco separado de testes. Uma escrita parcial feita por um trigger foi desfeita, e a dívida permaneceu aberta e sem data de pagamento. Após remover a falha simulada, o pagamento voltou a funcionar. Esse trigger não faz parte do schema da aplicação.
