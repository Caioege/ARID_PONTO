# Parâmetros de início da rota por presença

## O que é

Melhoria no cadastro da rota para definir se a execução pode ser iniciada sem paciente/acompanhante presente e sem profissional presente.

Foram adicionados dois parâmetros:

- Permitir iniciar sem paciente/acompanhante.
- Permitir iniciar sem profissional.

## Como usar

No cadastro da rota, acesse a aba **Acompanhantes/Pacientes** para configurar a regra de paciente/acompanhante.

Na aba **Equipe/Profissionais**, configure a regra de profissional.

Quando a permissão estiver marcada, o motorista pode iniciar a rota mesmo que ninguém daquele grupo seja marcado como presente. Quando estiver desmarcada, o aplicativo bloqueia o início até que exista ao menos um participante presente no grupo correspondente.

## Por que

Algumas rotas podem ser operacionais e não exigir paciente, acompanhante ou profissional no início. Outras rotas precisam obrigatoriamente confirmar presença antes de sair. A configuração por rota evita uma regra única para cenários diferentes.

## Guias de uso

### Exigir paciente ou acompanhante

1. Abra o cadastro da rota.
2. Entre na aba **Acompanhantes/Pacientes**.
3. Desmarque **Permitir iniciar sem paciente/acompanhante**.
4. Salve a rota.
5. No aplicativo, o motorista precisará marcar ao menos um paciente ou acompanhante como presente.

### Exigir profissional

1. Abra o cadastro da rota.
2. Entre na aba **Equipe/Profissionais**.
3. Desmarque **Permitir iniciar sem profissional**.
4. Salve a rota.
5. No aplicativo, o motorista precisará marcar ao menos um profissional como presente.

### Comportamento offline

O pacote offline do aplicativo também recebe os parâmetros da rota. Assim, a validação local continua funcionando mesmo quando o motorista inicia a rota sem conexão.

As rotas já existentes recebem o padrão permissivo no banco de dados para preservar o comportamento atual até que a configuração seja ajustada manualmente.
