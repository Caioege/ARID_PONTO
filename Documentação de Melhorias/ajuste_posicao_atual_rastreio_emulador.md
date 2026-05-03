# Ajuste de posicao atual no rastreio

## O que e

Melhoria no aplicativo `Aplicativos/ARID_RASTREIO` para evitar que o rastreio do motorista reutilize indefinidamente a ultima posicao conhecida do dispositivo durante testes em emulador.

Antes, em modo debug, uma coordenada antiga podia ser aceita como valida por tempo indeterminado. Com isso, ao iniciar ou acompanhar uma rota no emulador, o backend podia receber sempre a mesma latitude e longitude se o Android Emulator nao estivesse emitindo novas coordenadas.

## Como usar

Para testar uma rota no emulador, inicie o dispositivo normalmente:

```powershell
flutter emulators --launch Medium_Phone_API_36.1
```

Depois altere a localizacao do emulador pelo Android Studio em `Extended Controls > Location`, ou envie coordenadas pelo ADB:

```powershell
adb emu geo fix -49.236400 -16.706900
adb emu geo fix -49.238200 -16.708000
```

O comando `geo fix` usa a ordem `longitude latitude`.

## Por que

A rota cadastrada no sistema representa o trajeto planejado. Ela nao move automaticamente o GPS do dispositivo ou do emulador.

O aplicativo deve enviar a posicao real capturada pelo Android. Por isso, durante testes, o emulador precisa receber novas coordenadas. A melhoria limita o reaproveitamento de `getLastKnownPosition()` a leituras recentes e forca uma tentativa de `getCurrentPosition()` quando a ultima posicao esta velha.

## Guias de uso

Durante o teste em debug:

- Se a ultima posicao conhecida tiver ate 60 segundos, ela pode ser reaproveitada.
- Se a ultima posicao conhecida tiver mais de 60 segundos, o app tenta capturar uma posicao atual.
- Se a captura atual falhar em debug, a coordenada antiga nao e reenviada.
- Em producao, se a captura atual falhar e existir uma ultima posicao, o app ainda pode usa-la como fallback para reduzir perda de telemetria.

Para validar o ajuste, mude a coordenada do emulador durante uma rota em andamento e acompanhe os registros enviados para `/api/rastreio-app/rotas/salvar-ponto`.
