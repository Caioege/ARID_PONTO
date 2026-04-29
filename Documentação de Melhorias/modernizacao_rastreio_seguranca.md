# Modernização e Segurança no Rastreio de Rotas (21/04/2026)

Esta documentação descreve as melhorias implementadas no sistema de rastreio de frotas para garantir a persistência de dados, isolamento de execuções recorrentes e segurança contra simulação de localização (Fake GPS).

## O que é

Estas melhorias visam:
1. **Persistência de Sessão**: Garantir que se o usuário fechar o aplicativo ou o sistema reiniciar, a rota em execução seja restaurada exatamente de onde parou (incluindo veículo e checklist selecionados).
2. **Isolamento de Execução**: Permitir que rotas recorrentes possam ser executadas múltiplas vezes sem que os dados de uma execução interfiram na outra (ex: paradas marcadas no dia anterior não devem aparecer marcadas hoje).
3. **Segurança (Anti-Fake GPS)**: Detectar e registrar o uso de ferramentas de simulação de localização para prevenir fraudes no rastreio.

## Como Usar

### No Aplicativo Mobile
- Ao abrir o app, se houver uma rota em andamento, o sistema exibirá uma tela de carregamento ("Restaurando sua sessão...") e levará o usuário diretamente para o painel da rota.
- O sistema bloqueia o início de rotas se detectar que o dispositivo está simulando a localização.
- Durante a execução, todas as coordenadas enviadas carregam um marcador de segurança se a localização for suspeita.

### No Backend / Banco de Dados
- Foi introduzida a tabela `paradaexecucao` para armazenar o status das paradas por cada execução individual.
- A tabela `localizacaorota` agora possui a coluna `GpsSimulado (BIT)` para auditoria.

## Por que foi implementado?

- **Confiabilidade**: Motoristas reportavam perda de dados ao fechar o app acidentalmente.
- **Integridade**: Rotas recorrentes acumulavam o status das paradas, exigindo limpeza manual ou lógica complexa de reset. O novo modelo de isolamento resolve isso na estrutura de dados.
- **Conformidade**: A detecção de Fake GPS é essencial para garantir que as rotas planejadas foram de fato percorridas pelo veículo físico.

## Guias de Uso Técnico

### Estrutura de Tabelas
- `rotaexecucao`: Agora armazena `VeiculoId` e `ChecklistExecucaoId` para restauração completa.
- `paradaexecucao`: Centraliza o status de entrega/observação vinculado a um `id` de execução específico.

### APIs Atualizadas
- `POST /api/rastreio-app/rotas/iniciar`: Agora aceita `checklistExecucaoId`.
- `POST /api/rastreio-app/rotas/salvar-ponto`: Agora aceita flag `gpsSimulado`.
- `POST /api/rastreio-app/checklist`: Agora retorna o ID da execução do checklist para vinculação.
