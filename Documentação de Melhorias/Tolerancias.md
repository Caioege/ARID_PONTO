# Cadastro de Tolerância (Por Jornada)

## Descrição da Necessidade
O sistema AriD Ponto possuía configurações de tolerância gerais por dia (Tolerância Diária) ou por DSR.
A nova necessidade requer um controle mais granular, permitindo definir tolerâncias específicas em relação a cada marcação de ponto nos diferentes períodos da jornada (até 5 períodos).

## Regras de Negócio Implementadas

Foram criados 4 novos campos de configuração aplicáveis à vigência de um Horário de Trabalho (`HorarioDeTrabalhoVigencia`):

1. **Tolerância Antes da Entrada:** Define o limite (em minutos) em que o servidor pode bater o ponto antes do horário previsto de entrada de um período sem que isso gere banco de horas ou horas extras. (Ex: se a entrada é 08:00 e a tolerância é 10m, uma batida às 07:55 é arredondada/considerada como 08:00 para fins de saldo).
2. **Tolerância Após a Entrada:** Define o atraso aceitável (em minutos) após o horário previsto de entrada. 
3. **Tolerância Antes da Saída:** Define a saída antecipada aceitável (em minutos) antes do término previsto de um período.
4. **Tolerância Após a Saída:** Define o limite (em minutos) de horas extras ignoradas caso o servidor saia após o horário previsto do período.

## Como Usar no Sistema
- Acesse a interface web, navegue até os Cadastros de **Horário de Trabalho**.
- Ao **Adicionar** ou **Alterar** uma vigência de um horário, localize a seção de "Tolerâncias".
- Preencha os campos `Antes da Entrada`, `Após a Entrada`, `Antes da Saída` e `Após a Saída` com o valor desejado em minutos. Por padrão, o valor é `0` (desativado).
- Salve o horário.
- A partir da data de vigência configurada, a rotina de Geração da Folha de Ponto (Ponto do Dia) aplicará essas regras ao calcular o saldo daquele dia para os servidores atrelados a este horário.
