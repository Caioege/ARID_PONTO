# Relatório de Auditoria de Ausências (Item 9 do TR)

## Objetivo
Atender ao item do Termo de Referência que exige um relatório de auditoria detalhando todas as inserções, exclusões e alterações realizadas em ocorrências (afastamentos/ausências), identificando o operador responsável, data e hora da ação.

## Implementação

- **Menu**: Adicionado um novo item no menu lateral esquerdo, dentro do grupo de `Relatórios`, chamado `Auditoria de Ausências`.
- **Permissão**: Criada a permissão `eItemDePermissao_RelatorioAuditoriaDeAusencias` e vinculada à tela de Configurações de Grupos de Permissão. O administrador precisa acessar `Administração -> Grupos de Permissão` e marcar a caixa `Auditoria de Ausências` no bloco de Relatórios.
- **Visualização e Filtros**: A tela permite filtrar as ações de auditoria por um período (Início do Afastamento / Fim do Afastamento) e, para perfis administrativos, por Unidade de Lotação.
- **Relatório PDF**: O relatório exportado lista os dados recuperados da tabela de log `LogAuditoriaPonto`. Essa tabela já armazena histórico de alterações feitas nas folhas, ponto do dia, etc. O relatório filtra eventos dessa tabela cujos campos `Acao` ou `Descricao` correspondam a inserções/modificações de afastamentos.

## Como Usar
1. Conceda a permissão no grupo de usuários (ex: Administrador).
2. Acesse `Relatórios -> Auditoria de Ausências`.
3. Preencha os filtros desejados.
4. Clique em `Processar` para baixar o arquivo PDF contendo o detalhamento de quem fez o quê, quando e as justificativas.
