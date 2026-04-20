# Melhorias no Cadastro de Rotas - Gestão de Frota

## O que é?
Esta melhoria consiste em uma série de aprimoramentos no módulo de cadastro e gerenciamento de rotas do sistema de frotas, focando em robustez, usabilidade e integridade de dados.

## Por que?
Para transformar o gerenciamento de frotas em uma ferramenta mais profissional, era necessário permitir a designação de múltiplos motoristas, melhorar a experiência de cadastro de paradas com dados inteligentes e garantir que não houvesse duplicação de passageiros/pacientes.

## O que foi alterado?
1.  **Suporte a Motorista Secundário**: Campo opcional adicionado para rotas que exigem revezamento.
2.  **Modal Inteligente de Paradas**: Novo componente para adicionar/editar pontos de parada com busca por:
    *   Unidade da Rota (carregamento automático da base).
    *   Unidade Organizacional (lista de unidades do sistema).
    *   CEP (integração ViaCEP).
    *   Manual/Mapa (seleção visual no mapa).
3.  **Reordenação de Paradas**: Recurso de arrastar e soltar (drag-and-drop) para definir a ordem das paradas.
4.  **Validação de CPF**: Prevenção de duplicidade de pacientes/acompanhantes no cadastro.
5.  **Restrição de Fluxo**: Gerenciamento de paradas habilitado apenas após o salvamento inicial da rota.

## Como usar?
1.  Acesse **Frota > Rotas**.
2.  Ao criar uma nova rota, preencha os dados básicos e os motoristas.
3.  Após salvar, acesse a edição da rota.
4.  Na seção **Pontos de Parada Espaciais**, clique em **Adicionar Ponto**.
5.  Escolha o tipo de busca desejado. Se selecionar "Unid. Rota", o sistema buscará os dados da unidade de destino automaticamente.
6.  Confirme o ponto e, se necessário, use o ícone de menu à esquerda na tabela para reordenar os pontos arrastando-os.

## Arquivos Relacionados
- **Model**: `AriD.BibliotecaDeClasses/Entidades/Rota.cs`
- **Controller**: `AriD.GerenciamentoDePonto/Controllers/RotaController.cs`
- **View**: `AriD.GerenciamentoDePonto/Views/Rota/Cadastro.cshtml`
- **JS**: `AriD.GerenciamentoDePonto/wwwroot/Scripts/Paginas/Rota/index.js`
- **SQL**: `ScriptsSQL/alteracoes_20260420_melhorias_rotas_frota.sql`
