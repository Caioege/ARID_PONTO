# Monitoramento de Rotas - Gestão Mobile

## O que é
A funcionalidade de "Monitoramento" é uma tela interativa que centraliza a visualização em tempo real de todas as rotas ativas dentro do módulo Gestão Mobile. Ela desenha no mapa o traçado das rotas executadas e a última posição reportada por cada motorista em serviço, utilizando a biblioteca Leaflet.js sobre OpenStreetMap.

## Por que foi implementado
A necessidade de gerenciar frota exige tomada de decisão ágil. Ter um mapa vivo que expõe a localização de todos os rotas ativas permite identificar desvios, estimar chegadas e organizar a logística diária sem a necessidade de contatar cada motorista individualmente, elevando o projeto "Gestão Mobile" à um nível premium de operação tecnológica. Utilizou-se Leaflet por ser rápido, gratuito (sem depender de assinaturas ou chaves corporativas onerosas do Google Maps) e extremamente flexível.

## Como usar
1. Acesse o menu lateral e clique em **Gestão Mobile** > **Monitoramento**.
2. A tela recarregará exibindo um mapa em tela-cheia ocupando o container principal.
3. Se existirem rotas em andamento (Rotas onde o motorista inciou a execução através do App), elas desenharão o percurso histórico no mapa, marcando o ponto final com a cor da rota e os dados do último local recebido.
4. **Atualizações Autônomas:** O mapa irá se atualizar independentemente a cada 10 segundos para buscar posições frescas do servidor, emitindo um pulso visual de "Sincronizado" no canto inferior esquerdo.

## Guias de uso (Filtros e Interações)
- **Filtro Retrátil:** No canto direito superior, há uma janela sobreposta ("overlay") que, por padrão, já é renderizada em estilo *glassmorphism* trazendo uma estética moderna para a página.
- Nesses filtros você pode reduzir a visibilidade do mapa pesquisando especificamente por um Motorista, Veículo ou Rota. Ao selecionar e aplicar o filtro, o mapa esconderá as linhas que não dão `match`.
- **Popups de Informação:** Clicando no "marcador" de um veículo presente no mapa, um balão informativo será aberto com o descritivo da Rota, nome do Motorista, identificação (Placa/Modelo) e o horário exato em que a última coordenada de GPS foi recebida.
- **Teste Antecipado (Mocks):** Exclusivamente para a organização ID: `7`, existe uma geração autônoma de dados falsos em tempo-real (para fins de simulação/homologação de entrega). Veículos simulados irão "andar" lentamente pelo mapa a cada segundo, permitindo que a administração teste todo o comportamento da página imediatamente.
