# Reestruturacao da Execucao de Rotas do Motorista

## o que e
Esta melhoria redefine o modelo de execucao de rotas do aplicativo do motorista para separar claramente o cadastro base da rota do estado operacional da viagem. O objetivo e que cada execucao tenha sua propria sessao, sua propria telemetria e sua propria trilha de auditoria.

O novo desenho elimina a dependencia de colunas espalhadas em `rotaexecucao`, remove historicos em JSON como fonte principal e passa a registrar eventos operacionais de forma estruturada.

## por que
O desenho anterior acumulou responsabilidades em lugares diferentes:

- localizacao sendo gravada por rota, e nao por execucao;
- origem, destino e paradas com comportamentos diferentes entre si;
- pausas salvas em JSON;
- status operacionais misturados ao cadastro base da parada;
- scripts legados espalhados, com sobreposicao de responsabilidades.

Isso dificulta manutencao, auditoria e evolucao do aplicativo. O novo desenho deixa o fluxo do motorista mais consistente com a regra de negocio e mais escalavel para novas validacoes futuras.

## como usar
1. Use os scripts base de rota apenas para cadastro estrutural: `rota`, `paradarota`, configuracoes de pausa da rota, unidades e geofencing.
2. Use o script `ScriptsSQL/alteracoes_20260428_execucao_rotas_motorista_unificada.sql` para criar toda a camada operacional de execucao.
3. Ajuste o backend para gravar `rotaexecucao` para abrir e fechar a sessao, `rotaexecucaolocalizacao` para telemetria continua, `rotaexecucaoevento` para origem, paradas, destino e outros marcos, `rotaexecucaopausa` para pausas, e `rotaexecucaodesvio` para desvios.
4. Ajuste o app para sempre enviar `RotaExecucaoId` nas chamadas operacionais.

## guias de uso

### guia 1: abertura da rota
Quando o motorista iniciar a rota, o backend deve criar uma linha em `rotaexecucao` com o motorista, o veiculo, o checklist executado e a data/hora de inicio. Em seguida, deve criar um evento `InicioRota` em `rotaexecucaoevento`.

### guia 2: telemetria continua
Cada leitura valida de GPS deve gravar em `rotaexecucaolocalizacao`, sempre vinculada a `RotaExecucaoId`. A linha de execucao deve receber tambem a ultima latitude, ultima longitude e ultima atualizacao.

### guia 3: confirmacao de origem, parada e destino
Sempre que o motorista confirmar origem, uma parada intermediaria ou destino, o sistema deve gravar uma linha em `rotaexecucaoevento` com:

- tipo do evento;
- parada ou unidade relacionada;
- entregue;
- observacao;
- latitude;
- longitude;
- data/hora do evento.

### guia 4: pausas
Quando o motorista iniciar uma pausa, o sistema grava uma linha em `rotaexecucaopausa` com motivo, data/hora e localizacao inicial. Ao finalizar, completa essa mesma linha com data/hora e localizacao final.

### guia 5: encerramento
Ao encerrar a rota, o backend deve validar pendencias, registrar evento `FimRota`, atualizar `rotaexecucao` para status finalizada e preencher `DataHoraFim`.

## escopo da limpeza atual
Nesta etapa foram reorganizados os scripts da pasta `ScriptsSQL` para:

- remover referencias legadas de execucao dos scripts base;
- manter nos scripts base apenas o cadastro estrutural da rota;
- centralizar o novo desenho operacional em um unico arquivo.

## o que mudou no cadastro da rota
O cadastro web da rota voltou a tratar `paradarota` somente como estrutura base da rota. Isso significa que a parada cadastrada na tela de rota guarda apenas o que e fixo do planejamento:

- endereco;
- latitude;
- longitude;
- link;
- unidade vinculada;
- ordem.

Campos operacionais como `Entregue`, observacao de execucao e data/hora de conclusao deixaram de fazer parte da parada base. Esses dados agora pertencem exclusivamente a `rotaexecucaoevento`.

## como usar essa separacao
1. Use `/Rota/Cadastro` apenas para desenhar a rota planejada.
2. Cadastre origem, destino e paradas com seus dados geograficos e ordem.
3. Nao use a tela de cadastro para inferir status de entrega ou historico operacional.
4. Consulte a execucao da rota e seus eventos para saber o que foi confirmado pelo motorista em campo.

## por que essa separacao melhora o sistema
Separar cadastro base de evento operacional evita que a mesma parada carregue estado historico de execucoes anteriores. Isso reduz ambiguidade, facilita auditoria e elimina erros de tela causados por propriedades removidas do modelo antigo.

## guias de uso adicionais

### guia 6: leitura do monitoramento
No monitoramento e no historico da execucao, as paradas devem ser montadas combinando a parada base da rota com o ultimo evento da execucao para aquela parada. Assim o mapa mostra a estrutura planejada e, ao mesmo tempo, o status confirmado pelo motorista naquela sessao.

### guia 7: consultas tipadas
Controladores nao devem executar consultas SQL diretamente nem depender de `dynamic` para montar dados de tela. O fluxo deve usar servicos e repositorios com DTOs ou classes especificas para manter o contrato do backend previsivel e seguro para evolucao.

## arquivos relacionados
- `ScriptsSQL/alteracoes_20260322.sql`
- `ScriptsSQL/alteracoes.sql`
- `ScriptsSQL/alteracoes_20260420_rotas_origem_destino_status.sql`
- `ScriptsSQL/alteracoes_20260420_rotas_pausas.sql`
- `ScriptsSQL/alteracoes_20260426_rastreio_localizacao_parada.sql`
- `ScriptsSQL/alteracoes_20260428_execucao_rotas_motorista_unificada.sql`
