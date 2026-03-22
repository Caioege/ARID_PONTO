# Busca de Horários Similares (Modal de Inteligência)

## Descrição da Necessidade
Em diversas situações, o setor de RH ou o gestor precisa alterar o `Horário de Trabalho` vigente de um servidor. Como o número de horários cadastrados na instituição pode ser muito grande, tornou-se necessária uma ferramenta que sugira horários "parecidos" com o atual.

## Regras de Negócio Implementadas

A busca por horários similares foi implementada como um recuso interativo na tela, que:
1. Recebe o ID do Horário atual do servidor.
2. Compara a Carga Horária e as marcações de Entrada/Saída dos `HorarioDeTrabalhoDia` ativos (na vigência atual).
3. Busca na base de dados (tabela de Horários) outros horários que possuam uma carga horária e uma distribuição de períodos (dias da semana) muito similares ao pesquisado.

Foi incluído um componente de **Interface de Usuário (Modal)** com uma animação de "Pensando..." que transmite a sensação de que o sistema está fazendo uma análise inteligente e comparativa nos turnos.

## Como Usar no Sistema
1. Acesse o detalhe do Vínculo de Trabalho de um Servidor (*ou o próprio cadastro de Horário de Trabalho*).
2. Clique no botão **"Buscar Horários Parecidos"** ou no ícone da varinha mágica (indicando inteligência).
3. Um modal surgirá na tela, exibindo frases dinâmicas como *"Analisando a carga horária atual"*, *"Comparando turnos"*, com ícones de carregamento progressivo.
4. Após alguns segundos de processamento, o modal exibirá um resumo dos três a cinco horários mais aderentes.
5. O usuário poderá clicar em "Visualizar Horário" ou "Atribuir Horário" (caso disponível na tela de Vínculo) a partir dessa lista sugerida.
