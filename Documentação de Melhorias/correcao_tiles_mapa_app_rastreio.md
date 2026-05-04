# Correção dos tiles do mapa no aplicativo de rastreio

## O que é

Melhoria no carregamento dos mapas do aplicativo de rastreio para deixar de acessar diretamente os servidores públicos do OpenStreetMap.

O aplicativo passa a usar uma camada de mapa centralizada, evitando o bloqueio exibido quando o app tenta buscar tiles em `tile.openstreetmap.org`.

## Como usar

Nenhuma ação operacional é necessária para o motorista ou acompanhante.

Ao abrir uma tela com mapa no aplicativo, a camada de tiles será carregada pelo componente compartilhado `RastreioTileLayer`.

## Por que

O servidor público do OpenStreetMap pode bloquear aplicativos que consomem tiles diretamente, principalmente em uso embarcado/mobile. Quando isso ocorre, o mapa exibe a mensagem de acesso bloqueado no lugar dos tiles.

Centralizar a camada de mapa também facilita futuras trocas de provedor, caso seja necessário usar uma chave própria ou um serviço contratado.

## Guias de uso

Telas ajustadas:

- Mapa de execução da rota do motorista.
- Monitoramento da rota pelo acompanhante.
- Histórico/trajeto da rota pelo acompanhante.

Para novas telas com mapa, utilize `RastreioTileLayer` em vez de declarar diretamente um `TileLayer` com URL fixa.
