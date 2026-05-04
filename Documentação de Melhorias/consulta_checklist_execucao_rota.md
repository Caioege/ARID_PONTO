# Consulta do checklist preenchido na execução da rota

## O que é

Melhoria que permite ao operador consultar o checklist preenchido pelo motorista antes de iniciar uma rota.

A consulta fica disponível em dois pontos:

- Painel de monitoramento, quando uma rota estiver selecionada.
- Histórico de execuções dentro do cadastro da rota.

## Como usar

No painel de monitoramento:

1. Selecione uma rota no mapa ou na lista de rotas em acompanhamento.
2. Clique no botão **Checklist** no painel lateral da rota.
3. Consulte a data/hora do preenchimento, motorista, veículo e itens marcados.

No histórico de execução:

1. Abra o cadastro da rota.
2. Acesse a aba **Histórico**.
3. Clique no botão **Checklist** da execução desejada.

## Por que

O checklist é uma evidência operacional importante antes do início da rota. A consulta pelo operador facilita auditoria, conferência de segurança do veículo e validação de que o motorista realizou a etapa exigida antes de sair.

## Guias de uso

O botão utiliza a execução da rota como referência. O sistema localiza o `ChecklistExecucaoId` vinculado à execução e exibe os itens cadastrados para o veículo, indicando quais foram marcados pelo motorista.

Quando uma execução não possuir checklist vinculado, o botão fica indisponível no monitoramento ou sinalizado como **Sem checklist** no histórico.
