# Ocorrências no Espelho de Ponto (Rodapé do PDF)

Atendendo ao Item 7 do Termo de Referência, foi implementada a funcionalidade para incluir textos e observações adicionais no rodapé do espelho de ponto gerado no sistema.

## Como Usar
1. Acesse a tela de **Folha de Ponto**.
2. Selecione a Unidade e em seguida o Servidor desejado.
3. Clique em Pesquisar Folha.
4. Caso o usuário tenha as mesmas permissões para verificar os registros do aplicativo daquela folha de ponto, um novo botão amarela chamado **Ocorrências Espelho** aparecerá.
5. Ao clicar, um modal será aberto permitindo adicionar ocorrências/observações em formato de texto.
6. Digite a observação e salve. Você também pode apagar eventuais registros inseridos incorretamente na lista inferior.
7. Após lançar as ocorrências, feche a folha (se necessário e pertinente ao cargo) e clique no botão **Imprimir**.
8. O PDF gerado pelo iText7 conterá, ao final de todas as demais seções e antes das assinaturas, uma nova relação detalhada chamada "Ocorrências / Observações Adicionais".

## Componentes Técnicos
- **Entidade**: `OcorrenciaDoEspelhoPonto.cs`
- **Banco de Dados**: Tabela `OcorrenciaDoEspelhoPonto` incluída diretamente no banco através do script `ocorrencia_espelho.sql`.
- **Rotas**: 
  - `GET /FolhaDePonto/CarregarOcorrenciasRodape`
  - `POST /FolhaDePonto/SalvarOcorrenciaRodape`
  - `POST /FolhaDePonto/ExcluirOcorrenciaRodape`
- **Frontend**: Rotinas no `folhaDePonto.js` controlando a chamada e injeção do Modal construído dinamicamente na página via Javascript (na Div de Modais).
- **Backend (Gerador PDF)**: Método `RelatorioFolhaDePonto` na `FolhaDePontoController` alterado para receber as ocorrências e em sua montagem estrutural, no momento anterior ao descarte de stream do Documento, iterar e criar uma *Table* com *Cells* descrevendo cada ocorrência inserida.
