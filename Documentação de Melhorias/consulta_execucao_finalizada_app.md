# Consulta de execução finalizada no aplicativo

## O que é
Esta melhoria permite que o motorista consulte, no aplicativo, a execução de uma rota finalizada no dia atual.

Quando a rota já foi executada, o aplicativo deixa de tratar a seleção apenas como bloqueada e passa a oferecer uma consulta da execução finalizada, com trajeto no mapa, pontos registrados e histórico do chat.

## Como usar
1. Acesse o aplicativo como motorista.
2. Abra **Checklist** e selecione uma rota do dia.
3. Se a rota já foi finalizada hoje, a etapa fica bloqueada para nova execução.
4. Acesse **Rotas**.
5. Toque em **Consultar execução finalizada**.
6. Consulte o mapa do trajeto executado, os pontos da rota e o chat.
7. Ao encerrar uma rota em andamento, confira o resumo da execução exibido pelo aplicativo.

## Por que
Depois de finalizar a rota, o motorista ainda pode precisar conferir o trajeto executado, validar pontos, consultar observações e ler mensagens trocadas durante a execução.

Para rotas recorrentes, a consulta é liberada quando já existe execução finalizada no dia. Para rotas não recorrentes, a consulta é liberada quando a rota planejada para a data atual foi executada e finalizada.

## Guias de uso
- A consulta usa a execução finalizada do dia atual vinculada ao motorista.
- Rotas recorrentes finalizadas hoje ficam disponíveis para consulta da execução do dia.
- Rotas não recorrentes ficam disponíveis para consulta quando a `DataParaExecucao` é a data atual e a execução foi finalizada hoje.
- O modo consulta não permite registrar pontos, confirmar paradas, pausar ou encerrar rota.
- A tela deixa claro quando a execução está em modo histórico, diferenciando a consulta de uma rota ainda em andamento.
- O chat continua disponível para leitura; o backend mantém a regra de bloquear envio quando a execução está finalizada.
- O mapa exibe o trajeto real registrado em `rotaexecucaolocalizacao` para a execução selecionada.
- O mapa interno do aplicativo é exibido somente no modo consulta/histórico de execução finalizada.
- Durante uma rota em andamento, o aplicativo não carrega o mapa interno; o motorista continua usando o botão **Abrir caminho completo no Google Maps** para navegação.
- Ao finalizar a rota, o aplicativo exibe um resumo com pontos tratados, realizados, não realizados e indicação de execução offline.
- No checklist, rotas que possuem execução finalizada hoje aparecem identificadas como **finalizada hoje** no próprio dropdown.
- O botão de consulta não funciona para execução offline ainda não sincronizada, porque o servidor ainda não possui o histórico completo da execução.
