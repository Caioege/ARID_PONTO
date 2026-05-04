# Padronizacao de icones do ARID RASTREIO

## O que e

Padronizacao da identidade visual do aplicativo ARID RASTREIO nos pontos em que havia divergencia de icones: tela de login, barra superior, menu lateral e icone nativo do aplicativo.

A melhoria adiciona um novo asset interno (`assets/images/app-icon-rastreio.png`) para uso na interface e atualiza os icones nativos de Android, iOS, macOS e web mantendo os nomes ja referenciados pelos manifestos.

## Como usar

Use `assets/images/app-icon-rastreio.png` sempre que a interface precisar exibir a marca do ARID RASTREIO.

Na tela de login, a logomarca da ARID TECNOLOGIA fica no final do card por meio do asset `assets/images/logo-arid-tecnologia.png`, copiado do APP PONTO FACIAL.

No menu lateral, o cabecalho deve exibir a foto do servidor autenticado quando o campo `usuario.foto` estiver preenchido em base64. Se a foto estiver ausente ou invalida, o app exibe o fallback padrao de pessoa.

## Por que

Antes da melhoria, o aplicativo exibia referencias diferentes para a mesma identidade visual:

- `route.png` na tela de login.
- `smart-car.png` na barra superior.
- icone de onibus no menu lateral.
- arquivos nativos de launcher separados das telas internas.

Isso gerava uma experiencia inconsistente e dificultava identificar qual marca deveria representar o ARID RASTREIO.

## Guias de uso

Ao criar novas telas, use o asset `app-icon-rastreio.png` para representar o aplicativo. Evite reutilizar `bus.png`, `smart-car.png` ou `route.png` como marca principal.

Se o launcher precisar ser atualizado novamente, mantenha os mesmos nomes de arquivo ja existentes em `android/app/src/main/res`, `ios/Runner/Assets.xcassets`, `macos/Runner/Assets.xcassets` e `web/icons`, para evitar mudancas desnecessarias nos manifestos.

Ao alterar o payload de login, preserve o campo `foto` do `UsuarioDTO` como base64 para que o menu lateral consiga renderizar a foto do servidor autenticado.
