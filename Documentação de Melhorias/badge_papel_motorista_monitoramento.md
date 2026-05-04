# Badge de papel do motorista no monitoramento

## O que é

Melhoria no painel web de monitoramento de rotas para identificar se o motorista exibido na execução é o motorista principal ou secundário da rota.

O sistema passa a retornar o papel do motorista junto com os dados de monitoramento e exibe uma badge ao lado do nome.

## Como usar

Ao abrir o painel de monitoramento, o nome do motorista aparece acompanhado de uma badge:

- **Principal**, quando o motorista da execução é o motorista principal cadastrado na rota.
- **Secundário**, quando o motorista da execução é o motorista secundário cadastrado na rota.
- **Motorista**, quando não for possível classificar pelo vínculo da rota.

## Por que

Rotas podem ter motorista principal e secundário. No acompanhamento operacional, a equipe precisa saber rapidamente qual vínculo o motorista em execução possui naquela rota, sem abrir o cadastro.

## Guias de uso

A badge aparece nos seguintes pontos:

- resumo da rota na lista inferior;
- barra lateral da rota selecionada;
- popup do veículo no mapa;
- detalhes da rota selecionada.

Para manter a exibição correta, o monitoramento compara o `MotoristaId` da execução com `MotoristaId` e `MotoristaSecundarioId` cadastrados na rota.
