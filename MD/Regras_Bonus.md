# Módulo de Bônus (VT/VA) - Regras e Funcionamento

Este documento detalha o funcionamento e a arquitetura do Módulo de Bônus (Vale Transporte e Vale Alimentação) no sistema AriD de Gerenciamento de Ponto.

## 1. O que é o Módulo de Bônus?
O Módulo de Bônus permite o cadastro de benefícios diários aos quais os servidores têm direito, parametrizando valores em Reais (R$) e regras específicas de sua concessão.

## 2. Tipos de Bônus e Vínculo com a Folha de Ponto
**Sim, os bônus possuem vínculo direto com a folha de ponto apurada do servidor.** O cálculo mensal percorre cada dia do mês de apuração (`PontoDoDia`) e avalia de acordo com o **Tipo de Bônus**:

*   **Bônus Diário (Ex: Vale Transporte / Alimentação padrão):** Se o servidor **trabalhou no dia**, ele ganha o **Valor Diário** configurado. A soma dos dias dá o total. O usuário **não perde todo o bônus mensal** se faltar alguns dias, perde apenas do dia faltado.
*   **Bônus Mensal (Ex: Prêmio Assiduidade):** Atribuído de forma integral ao final do mês, porém condicionado às presenças. Existe a regra opcional **"Perde TUDO com 1 Falta?"** (`PerdeIntegralmenteComFalta`). Se ativada, qualquer falta injustificada no mês anula (zera) 100% o valor do bônus daquele mês.

Em ambos os casos, a distribuição pode ser universal ou restringida a **Funções/Cargos Específicos** selecionados no cadastro do bônus.

## 3. Regra de "Turno Intercalado" (Bônus Dobrado)
Algumas prefeituras ou organizações oferecem o benefício em dobro caso o servidor realize expedientes muito espaçados no mesmo dia (ex: trabalha de manhã, vai embora e volta de noite).
No cadastro de bônus, é possível habilitar a flag **"Turno Intercalado (Dobro)?"**.
*   **Tolerância de Minutos:** A área de RH define "Minutos de Intervalo para Dobro".
*   Se a diferença de tempo entre a primeira saída (`Saida1`) e a segunda entrada (`Entrada2`) for superior ou igual à tolerância, o sistema pagará **2 diárias** (o dobro) em vez de 1 naquele dia específico.

## 4. Pagamento Especial em Finais de Semana e Feriados
A bandeira **"Paga em FDS e Feriados?"** nas configurações estende a diária integralmente quando o motor de cálculo identifica que o registro de ponto adimplente do dia ocorreu em um repouso regular do calendário, em função de escalas extraordinárias.

## 5. Exportação para Folha de Pagamento
O Módulo de Bônus está completamente integrado à rotina de Exportação da Folha de Pagamento do sistema (`Exportação -> Gerar TXT/CSV`). 

Como a Folha de Pagamento tradicionalmente exporta minutos (ex: Horas Extras, Atrasos), foi criada uma regra especial para o bônus:
*   O exportador aglutina os bônus mensais de cada servidor (`BonusCalculado`).
*   Ele constrói a linha no layout do cliente injetando o tipo `Bonus = 6`.
*   Em vez de minutos ou formato `HH:MM`, a exportação para a tag `Bonus` assume obrigatoriamente um formato Decimal Monetário (`F2` Ex: `150,00`), alocando corretamente na coluna de `QTD/VALOR` do layout do Tribunal/e-Social/Sistema da Prefeitura.
