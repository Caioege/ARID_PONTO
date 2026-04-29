# Correção do cadastro de horário de trabalho com períodos

## o que é
Correção no cadastro de horário de trabalho do sistema web para garantir que os horários de entrada e saída dos períodos sejam enviados corretamente do navegador para o ASP.NET MVC Core durante o salvamento.

## como usar
Ao cadastrar ou alterar um horário de trabalho, informe normalmente os períodos na grade semanal e clique em salvar.

O sistema agora:
- envia `Dias` e `RegrasHoraExtra` no formato esperado pelo model binder do MVC Core;
- preserva os dados de vigência também na tela de inclusão;
- exibe a mensagem interna do banco quando ocorrer falha de persistência.

## por que
O JavaScript estava montando listas complexas em um formato de serialização que podia não ser reconstruído corretamente pelo backend. Como consequência, o cadastro do horário podia salvar parcialmente, criando o horário, mas descartando os períodos informados.

## guias de uso
Para validar a correção:

1. Acesse o cadastro de horário de trabalho.
2. Crie um novo horário com pelo menos um dia preenchido com `Entrada 1` e `Saída 1`.
3. Salve o cadastro.
4. Reabra o horário salvo e confirme se os períodos permanecem preenchidos.
5. Se houver erro de banco, verifique a mensagem exibida no rodapé, pois ela agora mostra o detalhe interno da exceção.
