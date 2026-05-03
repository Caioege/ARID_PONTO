# Ajustes no Monitoramento de Rotas no Mapa

## o que e
Esta melhoria ajusta a tela web de monitoramento de rotas para tornar o mapa mais estavel ao navegar pela aplicacao e enriquecer os dados exibidos no acompanhamento da execucao.

Foram adicionados:

- reinicializacao segura do mapa Leaflet ao sair e voltar para a tela;
- velocidade media do veiculo no tooltip da rota;
- hora de inicio junto da informacao de atualizacao/finalizacao;
- pin visual no ponto inicial executado pelo motorista;
- cor de rota finalizada com o mesmo tom da rota ativa, mas dessaturada;
- envio de localizacao mais frequente no app de rastreio.

## como usar
1. Acesse `Rota > Monitoramento`.
2. Navegue para outra tela e volte para o monitoramento sem precisar recarregar o navegador.
3. Clique no marcador do veiculo para ver motorista, veiculo, velocidade media, inicio e atualizacao/finalizacao.
4. Use o pin de inicio para identificar onde a execucao realmente comecou.
5. Marque `Exibir finalizadas` para visualizar rotas concluidas em uma cor mais cinza, preservando o tom original.

## por que
O sistema carrega telas por navegacao dinamica. Quando o JavaScript do monitoramento era executado novamente, variaveis declaradas com `let` e o mapa Leaflet anterior permaneciam em memoria, causando erro de redeclaracao e impedindo o carregamento do mapa.

Tambem havia perda visual de contexto em rotas finalizadas, porque elas eram convertidas para cinza puro. A nova regra mantem a identidade de cor da rota e apenas reduz a saturacao.

## guias de uso

### guia 1: estabilidade do mapa
Ao abrir a tela, o script limpa o intervalo antigo de atualizacao, remove a instancia Leaflet anterior e cria uma nova instancia para o container atual.

### guia 2: velocidade media
A velocidade media e calculada a partir dos pontos de `rotaexecucaolocalizacao` que possuem `VelocidadeMetrosPorSegundo`, convertida para km/h e exibida no tooltip do veiculo.

### guia 3: inicio da execucao
O pin de inicio usa a primeira coordenada registrada no historico da execucao. Ele representa onde o motorista comecou a enviar telemetria daquela rota.

### guia 4: frequencia de localizacao
O app de rastreio passou a executar o evento periodico de foreground com intervalo menor e a stream de GPS usa um filtro de distancia menor. Isso aumenta a densidade dos pontos salvos e reduz trechos quebrados no desenho da rota.
