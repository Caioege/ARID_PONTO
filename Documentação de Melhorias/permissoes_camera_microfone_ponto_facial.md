# Permissões de câmera e microfone no Ponto Facial

## O que é

Melhoria no fluxo de registro de ponto facial do aplicativo `ARID_PONTO_FACIAL` para garantir que as permissões de câmera e microfone sejam solicitadas antes da abertura da comprovação por selfie ou liveness no iOS.

## Como usar

No registro de ponto, o usuário mantém o botão pressionado para iniciar o envio da comprovação. Se a comprovação exigir câmera, o aplicativo verifica a permissão da câmera e solicita o acesso quando ainda não foi concedido.

No iOS, além da câmera, o aplicativo também verifica e solicita o microfone antes de abrir o fluxo de câmera, mantendo compatibilidade com a configuração nativa do `permission_handler`.

## Por que

Em publicações iOS, o usuário podia receber a mensagem "Você precisa habilitar a permissão da câmera para enviar sua comprovação" sem que o sistema exibisse previamente o pedido nativo de câmera/microfone.

A mudança evita esse bloqueio prematuro: quando a permissão ainda pode ser solicitada, o app chama o prompt nativo. Se o usuário negar ou se a permissão já estiver bloqueada nas configurações do aparelho, a mensagem de bloqueio existente continua sendo exibida.

## Guias de uso

1. Gere os arquivos nativos com `flutter pub get` antes de instalar pods no iOS.
2. No iOS, execute `pod install` dentro de `Aplicativos/ARID_PONTO_FACIAL/ios` após alterações de dependências ou permissões.
3. Para validar em dispositivo real, remova o app instalado antes do teste quando precisar ver o prompt nativo novamente, pois o iOS não reapresenta a permissão depois que o usuário nega de forma definitiva.
4. Se o usuário negar a permissão, oriente-o a habilitar câmera e microfone em Ajustes > Privacidade e Segurança > Câmera/Microfone ou nos ajustes do aplicativo.
