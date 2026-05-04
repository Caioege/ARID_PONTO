# Chat textual de rotas entre sistema e aplicativo

## O que é
Esta melhoria cria um chat simples, somente texto, vinculado à execução da rota.

O operador do sistema web acessa o chat pelo dashboard de monitoramento ao focar uma rota. O motorista ou responsável no aplicativo acessa o chat pela tela da rota em andamento.

## Como usar
1. No sistema web, acesse `Gestão Mobile > Monitoramento`.
2. Clique em uma rota para colocá-la em foco.
3. Clique em **Chat** no painel direito do mapa.
4. Envie mensagens de texto para o motorista pelo modal.
5. No aplicativo, abra a rota em andamento e toque em **Abrir chat da rota** para ler e responder.

## Por que
O termo de referência solicita comunicação entre o operador que acompanha a rota e o motorista ou responsável em campo.

O chat textual atende à comunicação operacional básica sem depender de integração externa de mensageria. Quando o sistema envia uma mensagem, o backend tenta disparar uma notificação push para o aplicativo usando o token já registrado no cadastro do servidor.

Como o chat ainda não usa socket em tempo real, o sistema passa a usar atualização periódica para reduzir a espera por novas mensagens.

## Guias de uso
- O histórico é salvo na tabela `rotaexecucaochat`.
- A mensagem é vinculada à `RotaExecucaoId`, não apenas à rota base.
- Rotas finalizadas permitem somente consulta do histórico.
- O sistema web bloqueia envio quando a execução está finalizada.
- O aplicativo também bloqueia envio quando o backend informa que a execução está finalizada.
- O push é tentado apenas quando o motorista possui `PushToken` cadastrado em `servidor`.
- Se o Firebase não estiver configurado ou o token não existir, a mensagem continua sendo salva normalmente.
- Após o login, o aplicativo tenta registrar o token Firebase pelo endpoint `api/rastreio-app/registrar-token`.
- O botão **Chat** do sistema exibe um badge com a quantidade de mensagens recebidas e ainda não lidas da rota focada.
- Quando o operador está na tela de monitoramento e chega mensagem de uma rota que não está em foco, o sistema exibe um alerta no canto da tela com o primeiro nome do motorista, veículo/placa e rota.
- O alerta de mensagem recebida usa layout próprio em bloco único, com ícone, texto e instrução de clique alinhados para evitar quebra visual em nomes, placas ou descrições maiores.
- Ao clicar nesse alerta, o painel coloca a rota da mensagem em foco e centraliza o mapa nela.
- O botão **Abrir chat da rota** do aplicativo exibe um badge com a quantidade de mensagens recebidas e ainda não lidas pelo aplicativo.
- No sistema web, o badge é consultado a cada 45 segundos para rotas ativas.
- A primeira consulta de mensagens não lidas no monitoramento funciona como referência inicial, para evitar popup de mensagens antigas ao abrir a tela.
- No aplicativo, o badge também é consultado a cada 45 segundos quando existe rota ativa.
- Com o chat aberto, sistema web e aplicativo recarregam as mensagens a cada 5 segundos.
- Ao abrir o chat, as mensagens recebidas daquela rota são marcadas como lidas para o lado que abriu a conversa.
- No aplicativo, o botão **Abrir chat da rota** continua visível durante o funcionamento offline, mas exibe um aviso informando que o chat depende de conexão com a internet.
- O aviso offline informa que mensagens já sincronizadas permanecem no histórico do servidor, mas envio, recebimento e atualização dependem de conexão.
- A melhoria continua sem anexos, áudio, imagens ou leitura por socket.
