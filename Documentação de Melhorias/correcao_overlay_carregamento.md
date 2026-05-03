# Correção do overlay de carregamento

## o que é

Ajuste no overlay exibido com a mensagem "Carregando..." para que ele fique acima de todos os elementos visuais da tela, incluindo o painel direito do login e componentes Select2 no sistema web.

## como usar

O comportamento é automático. Ao chamar o carregamento pelo plugin `jquery.loading` ou pela tela de login, o overlay passa a ser exibido com prioridade visual suficiente para cobrir os campos e painéis da interface.

## por que

O overlay do `jquery.loading` calculava o `z-index` a partir do elemento alvo. No login, o `body` gerava um overlay com `z-index` baixo, enquanto o painel do formulário tinha `z-index` maior. Em telas internas, componentes Select2 também podiam permanecer acima do carregamento.

## guias de uso

- Use `AbrirCaixaDeCarregamento` para carregamentos gerais baseados em SweetAlert.
- Use `jquery.loading` apenas quando for necessário bloquear uma região específica da tela.
- Caso uma tela use `jquery.loading` diretamente, não reduza o `z-index` do overlay abaixo de componentes como Select2, modais ou menus flutuantes.
