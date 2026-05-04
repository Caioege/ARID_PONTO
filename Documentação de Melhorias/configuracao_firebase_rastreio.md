# Configuracao Firebase no aplicativo de rastreio

## O que e

Configuracao do Firebase Cloud Messaging no aplicativo `ARID_RASTREIO` para permitir que o app obtenha um token FCM e envie esse token ao backend pelo endpoint `/api/rastreio-app/registrar-token`.

## Como usar

No Android, o arquivo `google-services.json` deve ficar em `Aplicativos/ARID_RASTREIO/android/app/google-services.json` e o `applicationId` deve permanecer alinhado ao pacote cadastrado no Firebase: `br.com.arid.arid_rastreio`.

No iOS, o arquivo `GoogleService-Info.plist` deve ficar em `Aplicativos/ARID_RASTREIO/ios/Runner/GoogleService-Info.plist` e o bundle identifier deve permanecer alinhado ao app cadastrado no Firebase: `br.com.arid.arid-rastreio`.

Apos login bem-sucedido, o aplicativo solicita permissao de notificacao, obtem o token FCM e registra esse token no servidor. Quando o Firebase atualizar o token do dispositivo, o aplicativo envia o novo token novamente ao backend.

## Por que

Sem os arquivos nativos do Firebase e sem o plugin `google-services` no Android, o SDK nao consegue inicializar o projeto correto e o app nao recebe um token valido para notificacoes push.

No iOS, o arquivo `GoogleService-Info.plist` precisa entrar como recurso do target `Runner`, e o modo de background `remote-notification` precisa estar declarado para suportar notificacoes remotas.

## Guias de uso

1. Compile e instale o app de rastreio em um aparelho real ou emulador com Google Play Services.
2. Realize login com um usuario valido.
3. Confirme no banco se a tabela `servidor` recebeu `PushToken`, `PlataformaDispositivo` e `UltimoAcessoApp`.
4. No iOS, habilite Push Notifications no Apple Developer/Xcode e configure APNs no projeto Firebase antes de validar recebimento em aparelho real.
5. Para disparo pelo backend, use o token salvo em `servidor.PushToken`.

O backend ainda precisa estar configurado com credenciais validas de envio do Firebase para disparar mensagens pelo servidor.
