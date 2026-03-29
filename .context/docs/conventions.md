# Convenções de Nomenclatura e Padrões de UI

Para manter a consistência com o restante do projeto, siga estas regras ao criar ou refatorar telas de cadastro:

## 1. Controller Actions (Nomes Padrão)

- **GET `Adicionar()`**: Carrega a view para criação de um novo registro.
- **GET `Alterar(int id)`**: Carrega a view para edição de um registro existente.
- **POST `Salvar(Entidade objeto)`**: Action única para persistir dados (Create e Update) via AJAX.
    - Deve retornar um `JsonResult`: `Json(new { sucesso = true, mensagem = "Sucesso!", id = 123 })`.
    - Deve incluir tratamento de erro com `try-catch` retornando `sucesso = false`.

## 2. Views (Nomes Padrão)

- **`Adicionar.cshtml`**: View de cadastro inicial.
- **`Alterar.cshtml`**: View de edição.
- **`_TabelaPaginada.cshtml`**: Partial view para listagem com WebGrid.

## 3. JavaScript e AJAX

- **Localização**: `wwwroot/Scripts/Paginas/[NomeDoModulo]/alterar.js`.
- **Compartilhamento**: Geralmente, `Adicionar.cshtml` e `Alterar.cshtml` referenciam o mesmo arquivo `alterar.js`.
- **Funções Globais**:
    - `ObtenhaFormularioSerializado(formId)`: Valida e serializa o formulário.
    - `RequisicaoAjaxComCarregamento(url, method, data, successCallback)`: Realiza a chamada AJAX com spinner de carregamento.
    - `MensagemRodape(type, message)`: Exibe feedback (success, warning, error).
    - `CarregarPagina(url)`: Realiza navegação interna entre páginas (SPA-like).

## 4. Estrutura do Formulário

- Use `<form id="formulario-[modulo]">`.
- O botão de salvar deve ter `id="btn-salvar"` e `type="button"` (não `submit`).
- O botão de voltar deve usar `CarregarPagina`.

## 5. Validação de Formulários

O sistema utiliza um padrão de validação automática baseado em classes CSS e atributos de label:
- **Labels Obrigatórias**: Devem ter a classe `obrigatorio` e o atributo `for` apontando para o `id` do campo correspondente.
    - Ex: `<label class="form-label obrigatorio" for="CampoId">Nome</label>`
- **Inputs**: O `id` do input gerado pelo Helper (ex: `@Html.TextBoxFor`) deve coincidir com o `for` da label.
- **Funcionamento**: A função `ObtenhaFormularioSerializado` verifica se todos os campos vinculados a labels `obrigatorio` possuem valor preenchido.

## 6. Documentação de Melhorias

- **Obrigatoriedade**: Toda e qualquer nova funcionalidade ou melhoria deve ser documentada em Markdown dentro da pasta `Documentação de Melhorias/`.
- **Conteúdo**: O documento deve conter as seções: "o que é", "como usar", "por que" e "guias de uso".
- **Consulta**: Antes de iniciar qualquer tarefa, consulte esta pasta para entender as regras de negócio vigentes.

> [!IMPORTANT]
> Nunca utilize o termo "Cadastrar" para nomes de Actions ou Views se o objetivo for Adicionar ou Alterar, procure sempre seguir o padrão acima. Além disso, **sempre** atualize a documentação de melhorias ao finalizar uma tarefa.
