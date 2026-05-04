# Plano do dashboard de monitoramento de rotas

## o que é
Este plano define a evolução da tela web existente de `Rota > Monitoramento` para um dashboard operacional de gestão mobile com mapa em tempo real, indicadores, alertas e barra lateral fixa com informações da rota selecionada.

A tela atual já existe em:

- `AriD.GerenciamentoDePonto/Views/Rota/Monitoramento.cshtml`
- `AriD.GerenciamentoDePonto/wwwroot/Scripts/Paginas/Rota/monitoramento.js`
- `AriD.GerenciamentoDePonto/Controllers/RotaController.cs`
- `AriD.Servicos/Servicos/ServicoMonitoramentoRotas.cs`
- `AriD.BibliotecaDeClasses/DTO/MonitoramentoRotasDTO.cs`

A nova versão não deve criar uma tela paralela. Ela deve reaproveitar a rota `Rota/Monitoramento`, o mapa Leaflet, o endpoint `Rota/ObterDadosMonitoramento` e a estrutura atual de execução de rotas, alterando principalmente o layout, a composição visual e os dados retornados para a barra lateral.

## por que
O termo de referência pede que o sistema permita acompanhar veículos em rota em tempo real, acompanhar todos os veículos simultaneamente, exibir informações da viagem, motorista, paciente, profissional de saúde, observações, ocorrências, manutenções e notificações.

Hoje a tela de monitoramento já resolve parte técnica importante:

- carrega mapa Leaflet com OpenStreetMap;
- atualiza automaticamente a cada 15 segundos;
- desenha trajetos por execução;
- exibe marcador do veículo;
- exibe paradas;
- identifica pausas;
- indica rota possivelmente offline;
- indica desvio detectado;
- usa `rotaexecucao`, `rotaexecucaolocalizacao`, `rotaexecucaoevento`, `rotaexecucaopausa` e `rotaexecucaodesvio`.

A melhoria deve transformar essa base em um dashboard operacional, seguindo estritamente o layout conceitual definido: cards no topo, mapa central dominante, barra lateral direita com informações da rota e alertas, tabela/lista inferior e modo de foco no mapa.

## como usar
Fluxo esperado depois da implementação:

1. O operador acessa `Gestão Mobile > Monitoramento`.
2. O dashboard abre com cards de resumo no topo e o mapa ocupando a área principal.
3. Todos os veículos em rota aparecem no mapa com cores por status.
4. Ao clicar em um veículo, a barra lateral direita passa a exibir os dados daquela rota.
5. O operador pode alternar filtros, selecionar uma rota pela lista inferior ou clicar diretamente no mapa.
6. O operador pode acionar o modo foco do mapa para ocultar cards, barra lateral, alertas e lista, mantendo apenas o mapa ativo.
7. O botão de chat aparece dentro da barra lateral da rota, mas nesta fase será apenas visual, sem envio, recebimento ou abertura de conversa.

## guias de uso

### layout obrigatório
O layout deve seguir a composição visual já definida na imagem conceitual:

- topo com título `Gestão Mobile - Rotas em Tempo Real` e controles compactos;
- linha superior de cards KPI;
- mapa central como área principal da página;
- barra lateral direita fixa para alertas e detalhes da rota selecionada;
- lista/tabela inferior de rotas em acompanhamento;
- painel de rota selecionada com status, motorista, veículo, paciente, profissional, observação e ações;
- botão de foco do mapa sempre visível sobre o mapa;
- botão de chat visível nas informações da rota, sem funcionalidade nesta etapa.

Não usar hero, página comercial, cards decorativos grandes ou layout diferente do mockup. A tela deve parecer ferramenta operacional densa, feita para uso recorrente.

### modo foco do mapa
O dashboard deve ter um estado `mapa-em-foco`.

Quando ativado:

- ocultar cards KPI;
- ocultar barra lateral direita;
- ocultar lista inferior;
- manter filtros essenciais minimizados ou recolhidos;
- expandir o wrapper do mapa para ocupar o máximo possível do conteúdo;
- manter indicador de sincronização;
- manter botão para sair do foco;
- chamar `map.invalidateSize()` após a transição para o Leaflet recalcular o tamanho.

Quando desativado:

- restaurar o layout completo;
- manter a rota selecionada;
- manter filtros aplicados;
- manter o estado atual do mapa sempre que possível.

### barra lateral da rota
A barra lateral direita deve substituir o uso principal dos popups longos no marcador. O popup pode continuar existindo de forma resumida, mas o painel lateral passa a ser a fonte principal de detalhe.

Dados mínimos da rota selecionada:

- descrição da rota;
- status operacional;
- motorista;
- veículo;
- paciente;
- profissional de saúde;
- observação da rota;
- data agendada;
- hora de início;
- hora de fim ou em execução;
- última atualização;
- velocidade média;
- última comunicação do app;
- alerta de offline quando aplicável;
- alerta de desvio quando aplicável;
- classificação offline quando aplicável;
- lista curta de paradas;
- lista curta de pausas;
- eventos/ocorrências recentes.

Ações visuais:

- `Chat` com ícone, apenas visual e sem funcionalidade nesta etapa;
- `Enviar push`, planejado para fase posterior;
- `Ver detalhes`, opcional se houver página/modal de auditoria;
- `Centralizar no mapa`, funcionalidade local permitida nesta etapa.

### cards KPI
Os cards superiores devem ser calculados a partir do retorno de `ObterDadosMonitoramento` ou de um resumo adicional no mesmo endpoint.

Cards previstos:

- veículos em rota;
- ocorrências/desvios;
- manutenções próximas ou vencidas;
- rotas finalizadas no dia;
- rotas possivelmente offline ou sem sinal GPS.

Na primeira entrega visual, os cards podem ser calculados no JavaScript usando `allRoutesData`, exceto manutenção, que depende de dados de `ManutencaoVeiculo`.

### alertas
O painel de alertas deve priorizar:

- ocorrência ou desvio no percurso;
- rota possivelmente offline;
- manutenção vencida ou próxima;
- rota iniciada;
- chegada ao destino;
- rota finalizada;
- pausa/almoço em andamento;
- execução parcial ou totalmente offline.

Nesta fase, os alertas podem ser derivados dos dados já existentes:

- `SujeitoADesvio`;
- `PossivelmenteOffline`;
- `PossuiRegistroOffline`;
- `ExecucaoOfflineCompleta`;
- `Pausas` sem `DataHoraFim`;
- `Finalizada`.

Para manutenção, será necessário ampliar o backend para consultar `ManutencaoVeiculo` e `Veiculo.QuilometragemAtual`.

### dados já existentes que devem ser reaproveitados
O plano deve reaproveitar:

- `MonitoramentoRotaDTO.ExecucaoId`;
- `RotaId`;
- `Descricao`;
- `DataParaExecucao`;
- `NomePaciente`;
- `MedicoResponsavel`;
- `HoraInicio`;
- `HoraFim`;
- `MotoristaNome`;
- `PlacaModelo`;
- `TipoVeiculo`;
- `VelocidadeMediaKmH`;
- `UltimaLocalizacao`;
- `HistoricoLocalizacoes`;
- `HistoricoLocalizacoesDetalhado`;
- `UltimaAtualizacao`;
- `Paradas`;
- `Pausas`;
- `Finalizada`;
- `SujeitoADesvio`;
- `PossuiRegistroOffline`;
- `ExecucaoOfflineCompleta`;
- `ClassificacaoOffline`;
- `PossivelmenteOffline`;
- `MinutosSemComunicacao`;
- `UltimaComunicacaoApp`.

### lacunas de dados
Para atender completamente o termo de referência, o backend deve ser ampliado com:

- profissional de saúde estruturado, usando `RotaProfissional`, além do campo atual `MedicoResponsavel`;
- pacientes estruturados, usando `RotaPaciente`, além do campo atual `NomePaciente`;
- observação da rota, usando `Rota.Observacao`;
- status textual operacional da execução, a partir de `rotaexecucao.Status` e pausas abertas;
- manutenção próxima ou vencida por veículo, usando `ManutencaoVeiculo`;
- eventos recentes da execução, usando `rotaexecucaoevento`;
- ocorrências/desvios recentes, usando `rotaexecucaodesvio`.

### fases de implementação

#### fase 1: remodelar layout sem mudar contrato crítico
Arquivos principais:

- `Views/Rota/Monitoramento.cshtml`
- `wwwroot/Scripts/Paginas/Rota/monitoramento.js`

Entregas:

- substituir o card único do mapa por estrutura de dashboard;
- criar row de KPIs;
- criar área central do mapa;
- criar barra lateral direita;
- criar lista inferior;
- migrar os dados longos do popup para a barra lateral;
- manter o endpoint atual;
- manter polling de 15 segundos;
- adicionar seleção de rota ao clicar no marcador;
- adicionar botão visual de chat na barra lateral, sem handler funcional;
- adicionar modo foco do mapa.

#### fase 2: enriquecer DTO e backend
Arquivos principais:

- `DTO/MonitoramentoRotasDTO.cs`
- `ServicoMonitoramentoRotas.cs`
- `RotaController.cs`

Entregas:

- incluir `StatusExecucao`;
- incluir `StatusDescricao`;
- incluir `ObservacaoRota`;
- incluir lista resumida de profissionais;
- incluir lista resumida de pacientes;
- incluir eventos recentes;
- incluir dados de manutenção do veículo;
- incluir severidade dos alertas.

#### fase 3: alertas e push
Arquivos principais a definir conforme a arquitetura de notificação:

- `FirebaseServico.cs`;
- `IServicoNotificacao.cs`;
- servico novo ou existente para regras de alerta.

Entregas:

- push ao iniciar rota;
- push quando houver ocorrência;
- push quando chegar ao destino;
- push quando finalizar rota;
- alerta de manutenção próxima/vencida;
- persistência ou log de alertas enviados para evitar repetição indevida.

#### fase 4: chat de rota
Esta fase fica fora da primeira implementação.

Entregas futuras:

- tabela de mensagens por `RotaExecucaoId`;
- endpoint web para enviar mensagem;
- endpoint app para enviar/listar mensagens;
- notificação push de nova mensagem;
- histórico/auditoria da conversa.

Na fase atual, apenas o botão `Chat` deve aparecer na barra lateral da rota.

## critérios de aceite
1. A tela continua acessível em `Rota/Monitoramento`.
2. O mapa continua carregando rotas reais com Leaflet.
3. O layout segue estritamente o mockup conceitual: KPIs no topo, mapa central, barra lateral direita, lista inferior.
4. Ao clicar em um veículo, a barra lateral mostra a rota selecionada.
5. O botão `Chat` aparece na barra lateral, mas não executa nenhuma funcionalidade.
6. O modo foco oculta as demais informações e deixa somente o mapa ativo.
7. Ao sair do modo foco, o dashboard volta ao estado anterior.
8. O polling continua atualizando os dados sem recriar o mapa indevidamente.
9. O contador de rotas possivelmente offline continua visível ou representado no novo painel.
10. A documentação deve ser atualizada se houver mudança posterior de comportamento.
