# Plano de Melhoria - Funcionamento Offline do Aplicativo de Rastreio

Data: 03/05/2026

## o que e

Este plano descreve a evolucao do aplicativo `ARID_RASTREIO` para permitir que o motorista execute rotas mesmo sem conexao com a internet, desde que tenha habilitado previamente o funcionamento offline e baixado os dados necessarios da rota.

O funcionamento ideal continua sendo online. O modo offline deve existir como recurso operacional para locais sem conectividade, mantendo rastreabilidade completa para auditoria administrativa.

## por que

Hoje a execucao da rota depende do servidor para criar a sessao real da viagem e retornar o `RotaExecucaoId`. Quando o motorista fica sem internet, o app pode perder a capacidade de iniciar, registrar eventos ou enviar pontos de localizacao em tempo real.

A melhoria deve resolver esse problema sem esconder do sistema que parte da operacao aconteceu offline. Toda informacao registrada sem conexao precisa chegar ao backend marcada como offline, permitindo auditoria posterior no historico, dashboard e monitoramento.

## principios do desenho

- O fluxo online continua sendo o fluxo principal.
- O modo offline deve ser habilitado explicitamente pelo usuario em uma nova configuracao no menu.
- O app so deve baixar dados offline enquanto estiver online.
- Dados baixados para operacao offline expiram em 3 dias.
- Quando estiver offline, o app grava uma execucao local temporaria.
- A execucao local usa um identificador proprio, como `localExecucaoId`, ate ser sincronizada com o servidor.
- O `RotaExecucaoId` real so deve existir depois que a sincronizacao criar ou reconciliar a execucao no backend.
- Toda acao registrada offline deve carregar metadata de auditoria.
- O operador deve conseguir diferenciar rotas online, parcialmente offline e completamente offline.
- O monitoramento deve alertar rotas em andamento que possivelmente estao offline por falta de comunicacao recente.

## regras de funcionamento

### habilitacao do modo offline

1. Criar uma nova opcao no menu/configuracoes do app: `Funcionamento offline`.
2. A opcao so pode ser habilitada se o app estiver online.
3. Ao habilitar, o app deve baixar o pacote offline do motorista.
4. O pacote deve conter toda a estrutura necessaria para operar:
   - rotas do motorista;
   - veiculos por rota;
   - itens de checklist;
   - origem, destino e paradas;
   - coordenadas, links e ordem das paradas;
   - regras de pausa da rota;
   - dados auxiliares necessarios para a tela funcionar sem consulta ao servidor.
5. O app deve gravar `dataHoraDownload`.
6. O pacote offline e valido por 3 dias.
7. Se o pacote estiver vencido, o app nao deve permitir iniciar rota offline.

### inicio de rota offline

1. Se estiver online, manter o fluxo atual.
2. Se estiver offline, permitir iniciar rota somente quando:
   - o modo offline estiver habilitado;
   - existir pacote offline baixado;
   - o pacote nao tiver mais de 3 dias;
   - a rota, o veiculo e o checklist existirem no cache local.
3. Ao iniciar offline, criar uma execucao local pendente com `localExecucaoId`.
4. O app deve registrar localmente:
   - rota;
   - veiculo;
   - checklist;
   - data/hora local de inicio;
   - latitude/longitude inicial, quando disponivel;
   - status de execucao offline.
5. O app deve informar ao motorista que a rota foi iniciada offline e sera sincronizada posteriormente.

### registros durante a rota offline

Enquanto estiver offline, o app deve salvar localmente:

- pontos de localizacao;
- confirmacao de origem;
- confirmacao de paradas;
- confirmacao de destino;
- inicio e fim de pausa;
- encerramento da rota;
- observacoes;
- informacoes de GPS simulado;
- data/hora local de cada registro.

Cada item deve ter um identificador local unico, como `clientEventId`, para evitar duplicidade na sincronizacao.

### badge e mensagens no app

Quando o app detectar perda de conexao durante uma execucao, deve exibir uma badge persistente no topo da tela com texto semelhante a:

`OFFLINE`

Na primeira deteccao de queda de conexao durante a execucao, exibir mensagem:

`Voce esta offline. As informacoes da rota serao salvas neste aparelho. Se a conexao voltar durante o processo, a sincronizacao sera feita automaticamente. Caso contrario, abra o aplicativo depois para enviar os dados pendentes.`

Quando a conexao voltar:

- exibir estado `Sincronizando...`;
- tentar sincronizacao automatica;
- ao concluir, informar que os dados foram sincronizados;
- se falhar, manter indicacao de pendencia e permitir nova tentativa manual.

### tela de pendencias

Criar uma area no menu/configuracoes:

- `Dados pendentes de sincronizacao`;
- quantidade de execucoes pendentes;
- data/hora da ultima tentativa;
- erro da ultima tentativa, quando houver;
- botao `Sincronizar agora`.

## auditoria offline no backend

Toda informacao gerada offline deve ser persistida no sistema com marcacao explicita.

Campos sugeridos para os registros sincronizados:

- `RegistradoOffline`;
- `DataHoraRegistroLocal`;
- `DataHoraSincronizacao`;
- `IdentificadorDispositivo`;
- `LocalExecucaoId`;
- `ClientEventId`.

Essa metadata deve existir, direta ou indiretamente, para:

- `rotaexecucao`;
- `rotaexecucaoevento`;
- `rotaexecucaolocalizacao`;
- `rotaexecucaopausa`;
- `rotaexecucaodesvio`, quando aplicavel.

A execucao tambem deve ter campos consolidados ou calculados:

- `PossuiRegistroOffline`;
- `ExecucaoOfflineCompleta`;
- `DataHoraPrimeiroRegistroOffline`;
- `DataHoraUltimoRegistroOffline`;
- `DataHoraUltimaComunicacaoApp`.

## classificacao historica da execucao

Na conferencia da rota, dashboard ou historico de execucao, o sistema deve indicar:

- `Rota executada completamente offline`;
- `Rota executada parcialmente offline`;
- sem selo especial quando nao houver registro offline.

Regra proposta:

- `Completamente offline`: inicio, eventos operacionais principais e encerramento foram registrados localmente antes da sincronizacao.
- `Parcialmente offline`: existe mistura de registros online e offline na mesma execucao.
- `Online`: nenhum registro operacional veio do modo offline.

A classificacao deve considerar localizacao, eventos, pausas e encerramento. Nao deve depender apenas dos pontos de GPS.

## visualizacao dos trechos offline

Nas telas de conferencia, historico e dashboard:

1. Exibir selo no resumo da execucao quando houver offline.
2. Diferenciar visualmente trechos offline no mapa.
3. Usar linha tracejada ou cor diferente para trechos offline.
4. Marcar eventos offline com icone ou tooltip.
5. Permitir filtros:
   - `Mostrar tudo`;
   - `Mostrar somente registros offline`;
   - `Mostrar somente registros online`.

Ao filtrar por offline, o mapa e a linha do tempo devem exibir somente:

- pontos de GPS offline;
- paradas confirmadas offline;
- pausas iniciadas/finalizadas offline;
- encerramento offline, se houver.

## monitoramento de rotas possivelmente offline

No monitoramento em tempo real, criar alerta para rotas em andamento que possivelmente ficaram offline.

Regra:

- rota esta em andamento;
- e o sistema nao recebeu localizacao, evento, pausa, encerramento, heartbeat ou pacote de sincronizacao nos ultimos 5 minutos;
- entao exibir `Possivelmente offline`.

Se a execucao nao tiver `DataHoraUltimaComunicacaoApp`, usar `DataHoraInicio` como referencia:

- `DataHoraInicio` com mais de 5 minutos;
- nenhuma comunicacao posterior;
- status em andamento;
- marcar como `Possivelmente offline`.

Visual sugerido:

- badge `Possivelmente offline`;
- destaque no mapa/dashboard;
- tooltip com tempo sem comunicacao;
- contador de rotas possivelmente offline.

Importante: `Possivelmente offline` e um alerta operacional em tempo real, nao uma classificacao historica definitiva. A classificacao `parcialmente offline` ou `completamente offline` so fica confirmada depois que os dados offline forem sincronizados.

## endpoint de sincronizacao

Criar endpoint dedicado para sincronizacao offline em lote:

`POST /api/rastreio-app/offline/sincronizar`

O endpoint deve receber:

- identificacao da execucao local;
- dados de rota;
- veiculo;
- checklist;
- data/hora de inicio;
- eventos;
- pausas;
- pontos de localizacao;
- encerramento, se houver;
- metadata offline de cada registro.

O endpoint deve ser idempotente. Se receber o mesmo `clientEventId` ou pacote mais de uma vez, nao deve duplicar execucao, eventos, pausas ou pontos.

## conflitos e validacoes

### rota alterada depois do download

Se a rota foi alterada no sistema depois do download, mas o pacote local ainda esta dentro da validade de 3 dias, a sincronizacao deve aceitar o snapshot local como base historica da execucao.

### rota removida ou desativada

Se uma rota baixada foi removida ou desativada antes da sincronizacao, o backend deve preservar o historico recebido e marcar a execucao com divergencia para auditoria.

### execucao online ja existente

Se ja existir execucao em andamento para o mesmo motorista no servidor, a sincronizacao deve bloquear ou exigir reconciliacao. Regra inicial sugerida:

- bloquear sincronizacao automatica;
- registrar erro claro;
- orientar conferencia administrativa.

## etapas de desenvolvimento

### etapa 1 - modelagem e contratos

Status: executada em 03/05/2026.

Objetivo: definir a base tecnica sem alterar o comportamento do app.

Entregas:

- definido DTO de pacote offline em `OfflineRastreioDTO.cs`;
- definido DTO de sincronizacao offline em `OfflineRastreioDTO.cs`;
- definidos campos de auditoria offline nas entidades operacionais;
- definida base de `DataHoraUltimaComunicacaoApp` em `rotaexecucao`;
- definida base de idempotencia por `LocalExecucaoId` e `ClientEventId`;
- criado script SQL `ScriptsSQL/alteracoes_20260503_rastreio_offline_auditoria.sql`.

Arquivos provaveis:

- `AriD.BibliotecaDeClasses/DTO/Aplicativo/RotaApp/`;
- `AriD.BibliotecaDeClasses/Entidades/`;
- `AriD.Servicos/Servicos/ServicoDeAplicativoDeRastreio.cs`;
- `ScriptsSQL/`.

### etapa 2 - pacote offline no backend

Status: executada em 03/05/2026.

Objetivo: permitir que o app baixe os dados necessarios para operar offline.

Entregas:

- criado endpoint para baixar pacote offline;
- retorno inclui rotas, veiculos, checklist, paradas e regras de pausa;
- pacote inclui `DataHoraGeracao`, `ValidoAte` e `ValidadeEmDias`;
- pacote e gerado para o motorista autenticado;
- rotas do pacote consideram janela de validade de 3 dias.

Endpoint sugerido:

`GET /api/rastreio-app/offline/pacote`

### etapa 3 - armazenamento local no app

Status: executada em 03/05/2026.

Objetivo: criar a base offline no Flutter.

Entregas:

- adicionado banco local SQLite com `sqflite`;
- criadas tabelas locais de cache e fila;
- criado repositorio local offline;
- criado servico para baixar e salvar pacote offline;
- implementada validacao de expiracao pelo `ValidoAte` do pacote;
- criada configuracao persistida `modo_offline_habilitado`.

Arquivos provaveis:

- `Aplicativos/ARID_RASTREIO/lib/core/storage/`;
- `Aplicativos/ARID_RASTREIO/lib/modules/motorista/`;
- `Aplicativos/ARID_RASTREIO/pubspec.yaml`.

### etapa 4 - UI de configuracao e pendencias

Status: executada em 03/05/2026.

Objetivo: permitir controle operacional pelo usuario.

Entregas:

- criada opcao `Offline` no drawer do motorista;
- criada tela para habilitar/desabilitar funcionamento offline;
- criada acao para baixar/atualizar pacote offline;
- exibida data do ultimo download;
- exibida validade do pacote;
- exibida quantidade de rotas baixadas;
- exibida quantidade de pendencias locais;
- criado botao `Sincronizar agora` preparado para a etapa de sincronizacao em lote.

### etapa 5 - execucao offline no app

Status: executada em 03/05/2026.

Objetivo: permitir que o motorista execute a rota sem internet.

Entregas:

- inicio de rota offline criado com `localExecucaoId`;
- checklist local salvo na fila offline quando nao ha conexao;
- rotas e veiculos passam a ser carregados do cache local quando o app esta offline;
- eventos de origem/parada/destino sao gravados localmente;
- inicio/fim de pausa sao gravados localmente;
- encerramento offline da execucao e gravado localmente;
- controller decide entre fluxo online e offline com base na conectividade;
- fluxo online foi mantido como prioridade;
- captura continua de GPS em foreground para offline fica para a etapa 6.

Arquivos provaveis:

- `Aplicativos/ARID_RASTREIO/lib/modules/motorista/checklist/`;
- `Aplicativos/ARID_RASTREIO/lib/modules/motorista/rotas/`;
- `Aplicativos/ARID_RASTREIO/lib/core/network/connectivity_service.dart`.

### etapa 6 - tracking offline em foreground

Status: executada em 03/05/2026.

Objetivo: nao perder pontos de localizacao quando o envio online falhar.

Entregas:

- `RotaBackgroundService` grava pontos localmente quando a execucao e offline;
- `rota_location_task.dart` usa `localExecucaoId` e `execucaoOffline` salvos no foreground task;
- pontos de GPS offline sao persistidos em `offline_localizacao`;
- pontos offline entram na `offline_sync_queue`;
- falhas do background deixam de ser engolidas silenciosamente e passam a gerar `debugPrint`;
- fluxo online continua exigindo `RotaExecucaoId` real.

Arquivos provaveis:

- `Aplicativos/ARID_RASTREIO/lib/core/service/rota_background_service.dart`;
- `Aplicativos/ARID_RASTREIO/lib/core/service/rota_location_task.dart`;
- `Aplicativos/ARID_RASTREIO/lib/core/service/rota_tracking_service.dart`.

### etapa 7 - badge e mensagens offline

Status: executada em 03/05/2026.

Objetivo: deixar claro para o motorista quando o app esta offline.

Entregas:

- criada deteccao de mudanca de conectividade durante rota ativa;
- exibida badge `OFFLINE`;
- exibida mensagem na primeira perda de conexao durante a rota;
- exibido estado visual de sincronizacao pendente quando a conexao volta e ha registros locais;
- mantida indicacao de pendencias ate a etapa de sincronizacao real;
- componente centralizado em `OfflineStatusBanner`.

### etapa 8 - sincronizacao em lote

Objetivo: enviar a execucao offline para o servidor de forma confiavel.

Entregas:

- [executado] criado endpoint `POST /api/rastreio-app/offline/sincronizar`;
- [executado] criado servico backend de sincronizacao;
- [executado] criada execucao real no servidor a partir da execucao local;
- [executado] mapeado `localExecucaoId` para `RotaExecucaoId`;
- [executado] envio de eventos, pausas e localizacoes em lote;
- [executado] registros sincronizados marcados como offline;
- [executado] preenchimento de `DataHoraSincronizacao`;
- [executado] idempotencia por `LocalExecucaoId`, `ClientEventId` e `TipoRegistro`;
- [executado] bloqueio inicial quando existe outra rota ativa para o motorista no servidor.

### etapa 9 - auditoria no historico e dashboard

Objetivo: permitir conferencia administrativa do que ocorreu offline.

Entregas:

- [executado] exibido selo `Rota executada completamente offline`;
- [executado] exibido selo `Rota executada parcialmente offline`;
- [executado] filtro no mapa do historico para `Mostrar tudo`, `Mostrar somente registros offline` e `Mostrar somente registros online`;
- [executado] linha de eventos/pausas no detalhe do historico respeitando o filtro offline/online;
- [executado] metadata de auditoria exibida nos detalhes;
- [executado] trechos offline destacados no mapa com linha tracejada;
- [executado] monitoramento passa a receber e exibir classificacao offline quando houver registro sincronizado.

Arquivos provaveis:

- `AriD.Servicos/Servicos/ServicoMonitoramentoRotas.cs`;
- `AriD.GerenciamentoDePonto/Controllers/RotaController.cs`;
- `AriD.GerenciamentoDePonto/Views/Rota/Monitoramento.cshtml`;
- `AriD.GerenciamentoDePonto/wwwroot/Scripts/Paginas/Rota/monitoramento.js`.

### etapa 10 - alerta de possivelmente offline no monitoramento

Status: executada em 03/05/2026.

Objetivo: avisar operadores quando uma rota em andamento parou de comunicar.

Entregas:

- [executado] atualizar `DataHoraUltimaComunicacaoApp` em cada recebimento relevante;
- [executado] calcular alerta de 5 minutos sem comunicacao;
- [executado] exibir badge `Possivelmente offline`;
- [executado] destacar rota no mapa/dashboard;
- [executado] exibir tooltip com ultima comunicacao;
- [executado] adicionar contador de rotas possivelmente offline.

### etapa 11 - documentacao final

Status: executada em 03/05/2026.

Objetivo: documentar o comportamento implementado conforme regra do projeto.

Entregas:

- [executado] criar Markdown em `Documentação de Melhorias/`;
- [executado] incluir secoes `o que e`, `como usar`, `por que` e `guias de uso`;
- [executado] documentar regras dos 3 dias;
- [executado] documentar auditoria offline;
- [executado] documentar sincronizacao;
- [executado] documentar alerta de possivelmente offline;
- [executado] documentar limitacoes e fluxo de suporte.

## validacoes esperadas

- Iniciar rota online continua funcionando como hoje.
- Habilitar offline baixa pacote somente com conexao.
- Pacote vencido com mais de 3 dias bloqueia inicio offline.
- Rota iniciada offline salva eventos e localizacoes localmente.
- Ao voltar conexao, app sincroniza automaticamente.
- Sincronizacao repetida nao duplica dados.
- Historico mostra se a rota foi parcial ou completamente offline.
- Operador consegue filtrar somente registros offline.
- Monitoramento marca rota em andamento sem comunicacao ha mais de 5 minutos como `Possivelmente offline`.
- Dados sincronizados offline ficam auditaveis no banco.

## riscos tecnicos

- Volume alto de pontos de GPS no armazenamento local.
- Duplicidade se a sincronizacao for interrompida no meio.
- Conflitos quando ja existir execucao em andamento para o motorista.
- Divergencia entre snapshot baixado e rota alterada no servidor.
- Perda de dados se o app for removido antes da sincronizacao.
- Necessidade de cuidado com background service e isolates do Flutter.

## decisao de implementacao recomendada

Comecar pela etapa 1 e etapa 2, porque elas definem o contrato correto entre app e backend. Em seguida implementar armazenamento local e configuracao offline no app. A execucao offline deve ser feita somente depois que o contrato de sincronizacao e auditoria estiver fechado.
