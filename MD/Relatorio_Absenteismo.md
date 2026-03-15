# Relatório de Absenteísmo

## Descrição da Necessidade
Criar uma visão para acompanhamento consolidado dos índices de absenteísmo (ausências não programadas, atrasos sistêmicos e faltas parciais ou totais) dos servidores, num período de tempo, facilitando o trabalho da gestão em identificar padrões comportamentais.

## Regras de Negócio Implementadas

O relatório cruza os dados das entidades `PontoDoDia` com `VinculoDeTrabalho` e as tabelas relacionadas, filtrando apenas ocorrências em que:
- O servidor deveria ter trabalhado (não era DSR, Feriado ou Facultativo completo).
- Não há configuração de `Afastamento` justificado com abono (Atestados médicos lançados via sistema).
- O campo `HorasNegativas` no ponto do dia foi superior a `0` ou não houve nenhuma marcação (falta integral sem justificativa/abono para o período).

Isso resulta numa lista de servidores que apresentaram horas negativas ou faltas injustificadas.

## Como Usar no Sistema
1. Acesse o menu de **Relatórios**.
2. Clique no relatórios de **Absenteísmo**.
3. Preencha os filtros desejados (ex: Período Inicial e Final, Lotação / Unidade, Departamento ou um Servidor específico).
4. Clique em Gerar.
5. A tela listará organizadamente: Nome do Servidor, Matrícula, as Datas em que houve absenteísmo, se a ocorrência foi Falta Integral ou Atraso e o total de horas perdidas no dia.
