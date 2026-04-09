# Módulo de Bônus (VT/VA) - Regras e Funcionamento

Este documento detalha o funcionamento e a arquitetura do Módulo de Bônus (Vale Transporte e Vale Alimentação) no sistema AriD de Gerenciamento de Ponto.

## 1. O que é o Módulo de Bônus?
O Módulo de Bônus permite o cadastro de benefícios diários aos quais os servidores têm direito, parametrizando valores em Reais (R$) e regras específicas de sua concessão.

## 2. Tipos de Bônus e Vínculo com a Folha de Ponto
**Sim, os bônus possuem vínculo direto com a folha de ponto apurada do servidor.** O cálculo mensal percorre cada dia do mês de apuração (`PontoDoDia`) e avalia de acordo com o **Tipo de Bônus**:

*   **Bônus Diário (Ex: Vale Transporte / Alimentação padrão):**
    - Se o servidor **trabalhou no dia**, ele ganha o **Valor Diário** configurado.
    - **Apenas dias com Carga Horária:** Se ativado, o sistema ignora dias onde não há previsão de jornada de trabalho (folgas, feriados sem escala, etc), mesmo que haja batida de ponto residual.
    - **Desconto por Falta/Atraso (Minutos):** É possível definir uma tolerância em minutos (configuração `MinutosFaltaDesconto`). Se o total de horas negativas no dia atingir esse limite, o bônus do dia é cancelado.
*   **Bônus Mensal (Ex: Prêmio Assiduidade):** Atribuído de forma integral ao final do mês, porém condicionado às presenças.
    - **Tolerância Mensal (Minutos):** Define um limite global de minutos de falta no mês (`MinutosFaltaDescontoMensal`). Se a soma das faltas atingir este valor, o bônus é zerado.
    - **Perde TUDO com 1 Falta?** (`PerdeIntegralmenteComFalta`). Se ativada, qualquer falta injustificada no mês anula (zera) 100% o valor do bônus daquele mês.

Em ambos os casos, a distribuição pode ser universal ou restringida a **Funções/Cargos Específicos** selecionados no cadastro do bônus.

## 3. Regra de "Turno Intercalado" (Bônus Dobrado)
Algumas prefeituras ou organizações oferecem o benefício em dobro caso o servidor realize expedientes muito espaçados no mesmo dia (ex: trabalha de manhã, vai embora e volta de noite).
No cadastro de bônus, é possível habilitar a flag **"Turno Intercalado (Dobro)?"**.
*   **Tolerância de Minutos:** A área de RH define "Minutos de Intervalo para Dobro".
*   Se a diferença de tempo entre a primeira saída (`Saida1`) e a segunda entrada (`Entrada2`) for superior ou igual à tolerância, o sistema pagará **2 diárias** (o dobro) em vez de 1 naquele dia específico.

## 4. Regra de Vigência e Carga Horária
A verificação de **Carga Horária** utiliza a jornada prevista para o servidor no dia (`ponto.CargaHoraria`). O sistema agora garante que dias com carga horária prevista sejam analisados mesmo que não haja registro de ponto no banco de dados para aquele dia (evitando que faltas justificadas ou escalas vazias sejam ignoradas pelo filtro de carga horária).

## 5. Exportação para Folha de Pagamento
O Módulo de Bônus está completamente integrado à rotina de Exportação da Folha de Pagamento do sistema (`Exportação -> Gerar TXT/CSV`). 

Como a Folha de Pagamento tradicionalmente exporta minutos (ex: Horas Extras, Atrasos), foi criada uma regra especial para o bônus:
*   O exportador aglutina os bônus mensais de cada servidor (`BonusCalculado`).
*   Ele constrói a linha no layout do cliente injetando o tipo `Bonus = 6`.
*   Em vez de minutos ou formato `HH:MM`, a exportação para a tag `Bonus` assume obrigatoriamente um formato Decimal Monetário (`F2` Ex: `150,00`), alocando corretamente na coluna de `QTD/VALOR` do layout do Tribunal/e-Social/Sistema da Prefeitura.

## 6. Cálculo por Dias Calendários

A partir da versão atual, o sistema itera por **todos os dias do calendário** do mês de referência (ex: de 1 a 31 de janeiro), independentemente de haver ou não registro de ponto ou escala no banco de dados para o dia específico.

1.  **Bônus Diário**:
    - Se "Apenas dias com Carga Horária" estiver **desmarcado**: O dia é contabilizado para o bônus, a menos que haja uma falta injustificada ou atraso acima da tolerância. Se o servidor trabalhar em um dia extra (sem carga horária), o bônus também é concedido se houver registro de ponto.
    - Se estiver **marcado**: O dia conta se houver carga horária prevista OU se houver trabalho efetivo (registros de ponto em qualquer um dos 5 períodos possíveis). Isso garante que o trabalho em dias extras não seja penalizado pelo filtro.
2.  **Faltas e Justificativas**: Dias com carga horária mas sem batida de ponto são analisados quanto a justificativas. Se houver qualquer justificativa (Afastamento, Abono ou Justificativa Manual em qualquer período), o bônus é mantido. Caso contrário, é considerada "Falta Injustificada" e o bônus do dia é removido.

## 7. Travas de Segurança e Integridade

Para garantir a consistência dos dados históricos, foram implementadas as seguintes travas:

1.  **Bloqueio de Tipo de Bônus**: Uma vez que uma configuração de bônus possua cálculos realizados, o campo **Tipo de Bônus** (Diário ou Mensal) torna-se **bloqueado** para edição. Para alterar o tipo, o gestor deve inativar a configuração atual e criar uma nova.
2.  **Folhas de Ponto Fechadas**: O bônus de um servidor **não é recalculado** se o mês de referência na Folha de Ponto já estiver marcado como **Fechado**. O cálculo automático só ocorre se o gestor reabrir a folha ou no momento do fechamento formal.
