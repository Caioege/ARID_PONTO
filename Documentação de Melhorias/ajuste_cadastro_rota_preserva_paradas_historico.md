# Ajuste no Cadastro de Rota com Historico de Paradas

## o que e
Esta melhoria ajusta o salvamento do cadastro web de rotas para preservar os registros existentes de `paradarota` quando a rota ja possui historico de execucao.

Antes, ao alterar a rota, o sistema removia todas as paradas e criava novos registros. Como `rotaexecucaoevento` referencia `paradarota.Id`, essa recriacao podia gerar erro de chave estrangeira ao salvar uma alteracao simples, como adicionar um novo veiculo na rota.

## como usar
1. Acesse o cadastro da rota normalmente.
2. Adicione ou remova veiculos associados sem precisar recriar os pontos da rota.
3. Edite os dados de uma parada existente quando precisar corrigir endereco, coordenadas, unidade ou ordem.
4. Para remover uma parada, confirme antes se ela nao possui historico de execucao vinculado.

## por que
As paradas da rota sao cadastro base, mas tambem podem ser referenciadas por eventos historicos da execucao do motorista. Remover e recriar essas paradas quebra a integridade referencial do banco quando ja existe historico apontando para elas.

O novo comportamento atualiza a parada existente pelo mesmo `Id`, adiciona apenas paradas novas e remove apenas paradas que realmente sairam da tela e nao possuem historico.

## guias de uso

### guia 1: adicionar veiculo em rota existente
Ao adicionar um novo veiculo e manter os pontos da rota na tela, o sistema preserva os `Id`s das paradas atuais e altera somente os vinculos de veiculo necessarios.

### guia 2: editar parada com historico
Se uma parada ja foi usada em `rotaexecucaoevento`, ela pode ter seus dados cadastrais atualizados sem trocar o `Id`. Isso mantem o historico apontando para o mesmo ponto planejado.

### guia 3: remover parada com historico
Se o operador tentar remover uma parada que ja possui evento historico, o sistema bloqueia a remocao com uma mensagem clara. Nessa situacao, crie uma nova rota ou ajuste os dados da parada existente.

### guia 4: vinculos de veiculo
Os veiculos da rota agora tambem sao sincronizados por diferenca: o sistema remove apenas veiculos desmarcados e adiciona apenas veiculos novos.
