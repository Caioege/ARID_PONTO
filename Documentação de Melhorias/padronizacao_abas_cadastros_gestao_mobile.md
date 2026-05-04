# Padronização das abas nos cadastros da gestão mobile

## O que é

Melhoria visual aplicada às abas dos cadastros de veículos, motoristas e rotas para usar o mesmo padrão de navegação já utilizado nas telas de Organização e Unidade Organizacional.

As abas passam a usar botões em formato de `pills`, com ícones, texto em destaque, área ativa evidenciada e conteúdo dentro de um contêiner branco com borda e sombra leve.

## Como usar

Na prática, o uso não muda para o usuário:

- No cadastro de veículos, acessar as abas de dados gerais, checklist e manutenções.
- No cadastro de motoristas, acessar dados básicos, histórico e dados do servidor quando disponíveis.
- No cadastro de rotas, acessar geral, pacientes, profissionais e histórico de execuções quando a rota já estiver cadastrada.

O comportamento de troca de abas continua sendo o mesmo, apenas com o padrão visual atualizado.

## Por que

A padronização reduz diferenças visuais entre telas de cadastro e facilita a leitura dos módulos da gestão mobile. Como Organização e Unidade Organizacional já utilizavam um padrão mais claro e consistente, os cadastros relacionados à frota e rotas foram ajustados para seguir a mesma identidade visual.

## Guias de uso

Para novas telas com abas, usar a estrutura:

- `nav-align-top` como wrapper externo.
- `nav nav-pills custom-nav-pills mb-3` para a lista de abas.
- Ícones `bx` dentro dos botões.
- Conteúdo das abas dentro de um bloco `border rounded p-4 shadow-sm`.
- `tab-content` sem padding e sem sombra própria.

Ao alterar abas existentes, preservar os IDs dos painéis e os atributos `data-bs-target` sempre que houver JavaScript dependente desses seletores.
