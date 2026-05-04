# Retorno para seleção de rota no aplicativo do motorista

## O que é

Melhoria no aplicativo de rastreio para permitir que o motorista saia da visualização de uma execução consultada e volte para a seleção de rotas.

Quando a rota exibida não estiver em andamento, o app passa a mostrar a ação para escolher outra rota.

## Como usar

Ao consultar uma execução finalizada, o motorista pode tocar em **Voltar para selecionar outra rota** ou no botão fixo **Escolher outra rota**.

O aplicativo limpa a execução visualizada, limpa a seleção de rota, veículo e checklist, recarrega a lista de rotas e volta para o fluxo inicial.

## Por que

Antes, depois de abrir uma execução finalizada para consulta, a tela permanecia presa naquela execução. Isso impedia o motorista de voltar para selecionar outra rota ou consultar outra execução.

A mudança separa a rota ativa da rota apenas consultada: rotas em andamento continuam protegidas contra troca acidental, mas execuções finalizadas podem ser fechadas.

## Guias de uso

- Se houver uma rota em andamento, o motorista deve encerrá-la antes de trocar de rota.
- Se a rota estiver apenas em consulta, use **Escolher outra rota** para retornar à preparação.
- A ação também limpa o contador do chat e o trajeto carregado da execução consultada.
