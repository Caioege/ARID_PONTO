# Dashboard de monitoramento de rotas com foco, alertas e manutenções

## O que é
Esta melhoria ajusta o dashboard de monitoramento da Gestão Mobile para carregar todas as rotas do período consultado, incluindo rotas finalizadas, e deixar os filtros visuais no frontend.

O painel direito do mapa passa a ser exclusivo da rota selecionada. Quando uma rota é selecionada, o mapa entra em foco operacional naquela rota e oculta as demais até que o usuário remova o foco pelo botão `X`.

## Como usar
1. Acesse `Gestão Mobile > Monitoramento`.
2. Use **Data da rota** para mudar o período consultado.
3. Use **Exibir finalizadas** para controlar se rotas finalizadas aparecem no mapa.
4. Use os filtros rápidos **Todos**, **Em rota**, **Em ocorrência**, **Sem sinal**, **Com mensagem**, **Finalizadas** e **Atrasadas** para ajustar somente a visualização do mapa e da lista.
5. Clique em um veículo, alerta ou card de rota para focar uma rota.
6. Clique em uma manutenção para abrir o preview e ver as rotas vinculadas ao veículo.
7. Use o botão `X` no painel direito para remover o foco e voltar a centralizar o mapa com todas as rotas visíveis.
8. Use a ação **Prevista x executada** para sobrepor o traçado previsto pelas paradas cadastradas ao trajeto executado.
9. Na **Linha do tempo**, clique no botão de PIN de um evento para exibir no mapa o local onde ele ocorreu.

## Por que
Rotas finalizadas podem poluir o mapa quando o operador está acompanhando veículos em tempo real. Ainda assim, elas precisam compor indicadores, alertas e manutenções do dia.

A separação entre carregamento de dados e filtro visual evita que os KPIs fiquem incompletos quando o operador oculta rotas finalizadas. A separação entre painel de rota, alertas gerais e manutenções também reduz ruído operacional no acompanhamento.

O PIN de evento permite auditar rapidamente onde uma ocorrência, parada, chegada ou finalização foi registrada sem sair do mapa de monitoramento.

## Guias de uso
- O serviço de monitoramento sempre retorna todas as rotas do período.
- O frontend mantém uma base geral para visão operacional e uma base visual para mapa/lista.
- KPIs, alertas gerais, chat e busca de rota usam a base geral de rotas. As manutenções usam uma base própria de veículos.
- O mapa e a lista de acompanhamento usam a base visual, onde o filtro **Exibir finalizadas** é aplicado.
- O painel superior mantém somente **Data da rota**, **Exibir finalizadas** e **Atualizar**.
- Os filtros de motorista, veículo, tipo de veículo e rota foram removidos de dentro do mapa, porque a lista inferior já exibe as rotas carregadas em cards compactos.
- Os KPIs consideram todas as rotas carregadas, não apenas as rotas visíveis no mapa.
- Os KPIs incluem mensagens não lidas, manutenções de veículos e atraso operacional. O atraso operacional considera rotas ativas com perda de comunicação, alerta crítico/alto, desvio ou comunicação sem atualização recente.
- Os filtros rápidos alteram apenas o mapa e a lista inferior. Eles não reduzem a base de cálculo dos KPIs, alertas gerais, chat ou manutenções.
- Ao focar uma rota, o mapa exibe somente essa rota até que o foco seja removido.
- O painel direito exibe somente informações da rota selecionada, linha do tempo, paradas e ações operacionais.
- O painel direito inclui uma linha do tempo consolidada e ordenada por data/hora com início, eventos, pontos tratados e finalização da execução selecionada.
- Todos os itens da linha do tempo exibem data e hora no padrão `dd/mm/aaaa hh:mm:ss`, e a ordenação considera hora, minuto e segundo.
- Na linha do tempo, paradas realizadas exibem ícone de confirmação em verde e paradas não realizadas exibem ícone de negativa em vermelho.
- Eventos recentes não são exibidos em uma seção separada para evitar duplicidade; eles aparecem dentro da linha do tempo.
- Eventos de parada confirmada são consolidados no próprio item da parada, para que `P1`, `P2` e demais pontos apareçam uma única vez e no horário real da confirmação.
- Os eventos de origem e destino aparecem com o nome da unidade cadastrada e o endereço do cadastro na sequência, usando a mesma marca visual de confirmação das paradas.
- O item **Rota finalizada** aparece somente quando a execução realmente estiver finalizada.
- Eventos com latitude e longitude possuem botão de PIN na linha do tempo. Ao fechar o popup ou clicar em outro local do mapa, o PIN temporário é removido.
- A seção **Alertas por rota** fica abaixo do mapa e mostra somente o último alerta de cada rota.
- Os alertas gerais são ordenados por severidade antes da data, para priorizar ocorrências críticas/altas mesmo quando existirem eventos informativos mais recentes.
- Cada card de alerta usa o formato `Rota - Veículo - Motorista` e permite focar a rota relacionada.
- A seção **Manutenções próximas ou vencidas** fica no final da página e lista manutenções dos veículos, independentemente de estarem vinculadas a uma rota do dia.
- Quando a manutenção não possui **Próxima Data (Vencimento)** preenchida, o monitoramento usa a **Data da Manutenção** como referência para vencimento ou agendamento.
- Ao clicar em uma manutenção, o sistema abre um modal com o preview da manutenção, dados do veículo, situação, data de referência, quilometragem atual, próxima quilometragem e as rotas carregadas no período que usam aquele veículo.
- No modal da manutenção, quando houver rota vinculada no período selecionado, o operador pode focar a rota diretamente pelo botão **Focar**.
- A lista inferior de rotas permanece em cards compactos para evitar rolagem lateral quando houver muitos pacientes ou profissionais.
- Eventos da linha do tempo com latitude e longitude ficam clicáveis e exibem um PIN temporário no mapa.
- Ao selecionar outra rota ou remover o foco, o PIN temporário do evento é removido.
- Rotas finalizadas não exibem o desenho do veículo no mapa. Para histórico finalizado, o mapa usa somente PIN de início e PIN de fim, mantendo o veículo apenas para rotas ativas.
