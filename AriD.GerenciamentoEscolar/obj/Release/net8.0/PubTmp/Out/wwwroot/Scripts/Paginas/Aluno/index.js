function carregarListaDeAlunos(somenteMatriculados) {
    carregarTabelaPaginadaComPesquisa('/Aluno/TabelaPaginada', 'grid', `somenteMatriculados=${somenteMatriculados}`);
}

function onChangeSomenteMatriculados() {
    carregarListaDeAlunos($('#SomenteMatriculados').val() == 'sim');
}