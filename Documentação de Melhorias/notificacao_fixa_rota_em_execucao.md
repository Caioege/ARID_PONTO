# Notificação fixa de rota em execução

## O que é

Melhoria no aplicativo de rastreio que exibe uma notificação fixa do Android enquanto uma rota está em execução em segundo plano.

A notificação mostra que a rota está ativa, informa a descrição da rota e indica se o aplicativo está enviando localização para o servidor ou salvando os dados localmente em uma execução offline.

## Como usar

O motorista não precisa habilitar nenhuma configuração extra.

Ao iniciar uma rota, o aplicativo inicia automaticamente o serviço de rastreamento em segundo plano e exibe a notificação fixa. Ao tocar na notificação ou no botão `Abrir rota`, o aplicativo é aberto novamente na tela principal.

Quando a rota é encerrada, ou quando o serviço de rastreamento é parado pelo fluxo normal do aplicativo, a notificação é removida automaticamente.

Em versões recentes do Android, o sistema pode permitir que o usuário dispense manualmente uma notificação de serviço em primeiro plano. Quando isso acontecer e a rota ainda estiver ativa, o aplicativo recria a notificação automaticamente.

## Por que

A notificação fixa deixa claro para o motorista que a rota continua sendo acompanhada mesmo com o aplicativo em segundo plano.

Também reduz o risco de o usuário encerrar o aplicativo achando que o rastreamento já parou, além de tornar o comportamento mais próximo de aplicativos de logística e entrega que mantêm uma indicação persistente durante operações ativas.

## Guias de uso

1. Inicie uma rota pelo fluxo normal do motorista.
2. Verifique a barra de notificações do Android.
3. Confirme que existe uma notificação chamada `Rota em execução`.
4. Toque em `Abrir rota` para voltar ao aplicativo.
5. Encerre a rota pelo aplicativo.
6. Confirme que a notificação desaparece automaticamente.

Para validar a persistência, tente remover a notificação enquanto a rota estiver ativa. Se o Android permitir a remoção manual, a notificação deve voltar automaticamente porque o serviço de rastreamento continua em execução.

Durante uma execução offline, a notificação informa que os dados estão sendo salvos localmente. Durante uma execução online, informa que a localização está sendo enviada em segundo plano.
