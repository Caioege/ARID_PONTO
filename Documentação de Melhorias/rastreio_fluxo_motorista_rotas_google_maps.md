# Rastreio: novo fluxo do motorista e rota completa no Google Maps

## O que e

Melhoria no fluxo do aplicativo de rastreio usado pelo motorista para reduzir a confusao entre checklist, preparacao da rota e execucao das paradas.

O aplicativo passa a destacar a jornada como uma sequencia operacional:

1. escolher a rota;
2. escolher o veiculo;
3. concluir a checklist;
4. iniciar a rota;
5. executar origem, paradas e destino;
6. encerrar a rota.

Tambem foi incluido suporte para observacoes cadastradas nos pontos de parada e para abrir o caminho completo no Google Maps.

## Como usar

No cadastro da rota, cada parada pode receber uma observacao para o motorista. Essa observacao e diferente da observacao digitada pelo motorista durante a execucao.

No aplicativo:

- a checklist mostra as etapas de preparacao;
- a selecao de rota aparece em lista direta, sem depender apenas de uma folha inferior;
- a rota em andamento mostra o proximo ponto e o progresso da execucao;
- cada parada exibe a orientacao cadastrada, quando existir;
- o botao "Abrir caminho completo no Google Maps" monta o trajeto com unidade de origem, paradas e unidade de destino, usando as coordenadas disponiveis.

## Por que

O fluxo anterior exigia alternar entre seletores e abas sem deixar claro qual era o proximo passo. Isso gerava duvida para o motorista principalmente antes de iniciar a rota e durante a execucao das paradas.

A nova organizacao deixa o estado atual visivel e reduz a dependencia de controles escondidos.

## Guias de uso

Para que o caminho completo seja aberto corretamente no Google Maps, origem, paradas e destino precisam ter latitude e longitude cadastradas.

Se algum ponto nao tiver coordenadas, ele nao entra no link do Google Maps. O aplicativo ainda permite abrir pontos individuais quando houver coordenada disponivel.

A observacao cadastrada da parada deve ser usada para instrucoes fixas, como referencia de acesso, portaria, ponto de encontro ou cuidado operacional. A observacao da execucao continua sendo o campo que o motorista preenche ao registrar se a parada foi realizada.
