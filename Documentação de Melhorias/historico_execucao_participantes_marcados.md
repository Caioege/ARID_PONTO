# Participantes marcados no histórico da execução da rota

## O que é

Melhoria no painel web de monitoramento para que o histórico da execução da rota liste apenas os participantes registrados no início daquela execução.

Os pacientes, acompanhantes e profissionais exibidos passam a vir da tabela `rotaexecucaopresenca`, e não mais apenas do cadastro base da rota.

## Como usar

No painel de monitoramento, selecione uma execução de rota.

Nos detalhes e no resumo da execução, o sistema exibirá:

- pacientes marcados pelo motorista;
- acompanhantes marcados pelo motorista;
- profissionais marcados pelo motorista;
- situação registrada como **Presente** ou **Ausente**.

## Por que

O cadastro da rota representa a previsão. O histórico da execução precisa refletir o que realmente foi conferido pelo motorista no início daquela viagem.

Isso evita exibir pacientes, acompanhantes ou profissionais que estavam cadastrados na rota, mas não foram registrados naquela execução.

## Guias de uso

- Se a execução não tiver registro de presença, os campos de pacientes, acompanhantes e profissionais ficarão vazios no histórico.
- A lista exibida é separada por execução, usando `RotaExecucaoId`.
- Acompanhantes são exibidos em campo próprio, separados dos pacientes.
- O status de presença é mostrado ao lado do nome para facilitar auditoria posterior.
