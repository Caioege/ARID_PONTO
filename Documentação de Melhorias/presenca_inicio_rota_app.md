# Presença de pacientes, acompanhantes e profissionais no início da rota

## O que é
Esta melhoria adiciona uma etapa obrigatória antes de iniciar a rota no aplicativo do motorista.

Nessa etapa, o motorista confere pacientes, acompanhantes e profissionais vinculados à rota e marca quem está presente. O motorista também pode adicionar um paciente já cadastrado ou cadastrar um novo paciente diretamente no aplicativo antes de iniciar a execução.

## Como usar
1. No aplicativo, selecione a rota.
2. Selecione o veículo e salve o checklist.
3. Toque em **Iniciar rota agora**.
4. Na tela **Conferir presença**, marque pacientes, acompanhantes e profissionais presentes.
5. Use **Adicionar existente** para incluir um paciente já cadastrado.
6. Use **Novo paciente** para preencher os dados básicos de um paciente ainda não cadastrado.
7. Toque em **Iniciar rota** para gravar a presença e iniciar a execução.

## Por que
A confirmação de presença cria um manifesto operacional da execução. Isso registra quem realmente embarcou ou acompanhou a rota, sem depender apenas do cadastro planejado.

Quando o motorista inclui um paciente pelo aplicativo, o sistema também cria ou vincula esse paciente à rota, mantendo o cadastro base coerente com o que foi executado.

## Guias de uso
- A presença é registrada na tabela `rotaexecucaopresenca`.
- O registro é vinculado à execução da rota, não substitui o histórico cadastral.
- Pacientes e acompanhantes são tratados separadamente no manifesto de presença.
- Profissionais vinculados à rota também aparecem para confirmação.
- Pacientes adicionados pelo aplicativo são marcados como criados pelo app no registro de presença.
- Em modo offline, a presença é salva no pacote local e enviada na sincronização da execução.
- O motorista só inicia a rota após confirmar ou cancelar a etapa de presença.
