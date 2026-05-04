# Funcionamento offline no rastreio

Data: 03/05/2026

## o que é

Melhoria do aplicativo `ARID_RASTREIO` para permitir que o motorista execute rotas sem conexão com a internet, desde que tenha habilitado o funcionamento offline e baixado previamente um pacote válido de rotas.

O fluxo online continua sendo o fluxo principal. O modo offline é um recurso operacional para locais sem conectividade, mantendo a execução da rota e preservando auditoria administrativa.

O pacote offline contém rotas, veículos, checklist, origem, destino, paradas e regras necessárias para a tela funcionar sem consultar o servidor. A validade operacional inicial é de 3 dias. Registros feitos sem conexão ficam no aparelho até que a sincronização envie os dados ao backend.

Toda execução, evento, pausa e localização sincronizada a partir do modo offline fica marcada com auditoria offline no banco, incluindo `LocalExecucaoId`, `ClientEventId`, `IdentificadorDispositivo`, `DataHoraRegistroLocal` e `DataHoraSincronizacao`.

## como usar

No app do motorista, acesse o menu `Offline`.

1. Habilite `Funcionamento offline`.
2. O app baixa o pacote offline enquanto houver conexão.
3. Com o pacote válido, o motorista pode iniciar uma rota mesmo sem internet.
4. Durante a rota offline, o app salva localmente checklist, início, eventos, pausas, localizações e encerramento.
5. Quando a conexão voltar, o app tenta sincronizar automaticamente.
6. O usuário também pode usar `Sincronizar agora` na tela `Offline`.

Sempre que o servidor de rastreio estiver inacessível, o app exibe a indicação `OFFLINE` no topo das telas do motorista. Se houver uma rota ativa, o detalhe informa que os registros serão salvos localmente. Quando a conexão voltar, o app tenta enviar as pendências e mantém a indicação de pendência se a sincronização falhar.

Na indicação `OFFLINE`, o usuário pode tocar no ícone de ajuda para consultar um resumo do funcionamento offline, incluindo o que fica salvo localmente e o que depende de sincronização.

A tela `Offline` também exibe o estado `Aplicativo online` ou `Aplicativo offline`. Esse estado não depende apenas do Wi-Fi ou da rede móvel do Android: o app tenta comunicar com o backend e considera offline quando o servidor de rastreio não responde.

## por que

Rotas podem passar por locais sem cobertura ou com conexão instável. Antes dessa melhoria, a operação dependia de comunicação online contínua, o que podia impedir o início de uma rota, interromper registros operacionais ou perder contexto de rastreio.

O modo offline preserva a operação do motorista e, ao mesmo tempo, mantém rastreabilidade administrativa. Os administradores conseguem auditar quais registros foram feitos offline, conferir se a rota foi parcial ou completamente offline e acompanhar no monitoramento rotas que possivelmente pararam de comunicar.

## guias de uso

### baixar pacote offline

- O download só deve ser feito com conexão ativa.
- O endpoint usado pelo app é `GET /api/rastreio-app/offline/pacote`.
- Antes de baixar o pacote, o app valida comunicação real com o backend em `GET /api/rastreio-app/conectividade`.
- O pacote expira em 3 dias, considerando o campo `ValidoAte`.
- Se o pacote estiver vencido, vazio ou ausente, o app não permite iniciar rota offline.
- Ao atualizar o pacote, o app substitui o cache de rotas, veículos, checklist e paradas.

### executar rota offline

- O app cria uma execução local com `localExecucaoId`.
- O `RotaExecucaoId` real só é conhecido depois da sincronização com o backend.
- Cada registro operacional recebe um `clientEventId` para evitar duplicidade.
- Eventos de origem, parada, destino, pausa e encerramento sao gravados no SQLite local.
- A localização em foreground é gravada em `offline_localizacao` quando a execução é offline.
- O app continua priorizando o fluxo online sempre que houver conexão.

### sincronizar pendências

- Endpoint backend: `POST /api/rastreio-app/offline/sincronizar`.
- Antes de sincronizar, o app valida comunicação real com o backend em `GET /api/rastreio-app/conectividade`.
- O app monta um lote por `localExecucaoId`.
- O backend cria ou reutiliza a execução real pelo `LocalExecucaoId`.
- Eventos, pausas e localizações são gravados com `RegistradoOffline = 1`.
- A tabela `rotaexecucaosincronizacaooffline` registra a idempotência por `LocalExecucaoId`, `ClientEventId` e `TipoRegistro`.
- Reenvio do mesmo pacote não deve duplicar execução, evento, pausa ou ponto de localização.
- Se existir outra rota ativa para o mesmo motorista no servidor, a sincronização automática é bloqueada para evitar mistura de execuções.

### auditoria

- `rotaexecucao` guarda se houve registro offline e se a execução foi completamente offline.
- `rotaexecucaolocalizacao`, `rotaexecucaoevento`, `rotaexecucaopausa` e `rotaexecucaodesvio` possuem campos de auditoria offline.
- `DataHoraUltimaComunicacaoApp` é atualizada nos recebimentos relevantes do app, como início, localização, evento, pausa, encerramento e sincronização offline.
- `DataHoraUltimaComunicacaoApp` é usada no monitoramento para identificar rotas possivelmente offline.
- A classificação histórica considera registros operacionais sincronizados, não apenas pontos GPS.

### conferência no histórico e monitoramento

- No histórico de execuções da rota, a coluna `Auditoria` indica `Online`, `Rota executada parcialmente offline` ou `Rota executada completamente offline`.
- Ao abrir uma execução no mapa do histórico, o operador pode alternar entre `Mostrar tudo`, `Mostrar somente registros offline` e `Mostrar somente registros online`.
- Pontos e trechos offline são destacados em laranja e linha tracejada.
- O detalhe da execução mostra `LocalExecucaoId`, identificador do dispositivo, quantidade de pontos GPS offline e eventos/pausas com origem offline ou online.
- No monitoramento de rotas, execuções que já possuem registros sincronizados offline exibem a classificação no popup e o trecho offline destacado.

### alerta de possivelmente offline

- No monitoramento em tempo real, execuções ativas sem comunicação do app por 5 minutos ou mais recebem o alerta `Possivelmente offline`.
- A comunicação considera localização, eventos operacionais, pausa, encerramento e lote de sincronização.
- Quando não existe `DataHoraUltimaComunicacaoApp`, o sistema usa a última localização recebida ou o início da execução como referência.
- A tela de monitoramento exibe contador de rotas possivelmente offline.
- Rotas possivelmente offline recebem destaque no mapa e popup com tempo sem comunicação e data/hora da última comunicação.
- Esse alerta é operacional e temporário. A classificação histórica `parcialmente offline` ou `completamente offline` só é confirmada depois da sincronização dos registros locais.

## limitações

- A remoção do app ou limpeza dos dados locais antes da sincronização pode apagar registros pendentes.
- O pacote offline é um snapshot. Se a rota for alterada depois do download, o app continua usando os dados baixados enquanto o pacote estiver válido.
- Rota removida ou desativada depois do download pode exigir conferência administrativa na sincronização.
- O volume de pontos GPS pode crescer em rotas longas sem conexão; por isso a tela `Offline` deve ser monitorada quando houver muitas pendências.
- A sincronização automática depende de o app ser aberto novamente ou recuperar conexão durante a execução.
- O chat da rota depende de internet e comunicação com o servidor. O botão permanece visível na rota, mas exibe aviso de indisponibilidade quando o app está offline.
- `Possivelmente offline` não prova que a rota foi executada offline. É apenas um alerta de falta de comunicação recente.

## fluxo de suporte

1. Verificar no app do motorista se há pendências na tela `Offline`.
2. Pedir ao motorista para abrir o app com conexão ativa e acionar `Sincronizar agora`.
3. Se a sincronização falhar, conferir a mensagem exibida no app.
4. No sistema web, conferir o histórico da rota e a coluna `Auditoria`.
5. Se houver conflito por outra rota ativa do mesmo motorista, encerrar ou auditar a execução conflitante antes de tentar nova sincronização.
6. Para auditoria técnica, consultar `LocalExecucaoId`, `ClientEventId`, `IdentificadorDispositivo`, `DataHoraRegistroLocal` e `DataHoraSincronizacao`.

## arquivos principais

- `AriD.GerenciamentoDePonto/Controllers/RastreioAppController.cs`
- `AriD.Servicos/Servicos/ServicoDeAplicativoDeRastreio.cs`
- `AriD.Servicos/Servicos/ServicoMonitoramentoRotas.cs`
- `AriD.GerenciamentoDePonto/Views/Rota/Cadastro.cshtml`
- `AriD.GerenciamentoDePonto/Views/Rota/Monitoramento.cshtml`
- `AriD.GerenciamentoDePonto/wwwroot/Scripts/Paginas/Rota/cadastro.js`
- `AriD.GerenciamentoDePonto/wwwroot/Scripts/Paginas/Rota/monitoramento.js`
- `Aplicativos/ARID_RASTREIO/lib/core/storage/offline_database.dart`
- `Aplicativos/ARID_RASTREIO/lib/modules/motorista/offline/`
- `ScriptsSQL/alteracoes_20260503_rastreio_offline_auditoria.sql`
