# Checklist do aplicativo com dropdowns e etapas recolhíveis

## O que é
Esta melhoria reorganiza a tela de checklist do aplicativo `ARID_RASTREIO`.

A seleção de rota e veículo volta a usar dropdowns, como no comportamento anterior. A tela passa a concentrar as etapas em um único fluxo com seções recolhíveis: **Rota**, **Veículo** e **Checklist**.

## Como usar
1. Abra a tela **Checklist**.
2. Escolha a rota no dropdown da seção **Rota**.
3. A seção **Rota** fecha e a seção **Veículo** abre automaticamente.
4. Escolha o veículo no dropdown da seção **Veículo**.
5. A seção **Veículo** fecha e a seção **Checklist** abre automaticamente.
6. Marque os itens do checklist e toque em **Salvar checklist**.
7. Ao salvar com sucesso, a seção **Checklist** é fechada.

## Por que
A seleção em lista ocupava muito espaço e exigia mais rolagem. O uso de dropdowns deixa a seleção mais compacta e recupera o padrão anterior esperado pelo usuário.

As seções recolhíveis reduzem a necessidade de transitar visualmente por várias áreas da tela e deixam claro qual etapa está ativa no momento.

## Guias de uso
- A rota é obrigatória para carregar os veículos.
- O veículo é obrigatório para carregar os itens do checklist.
- A etapa seguinte abre automaticamente quando a etapa atual é concluída.
- Quando uma rota já está iniciada, os campos continuam bloqueados para edição.
- O indicador superior de etapas continua mostrando o progresso entre rota, veículo e checklist.
