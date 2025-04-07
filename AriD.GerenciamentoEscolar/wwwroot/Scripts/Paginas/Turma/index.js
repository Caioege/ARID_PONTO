function carregarListaDeTurmas(anoLetivo) {
    carregarTabelaPaginadaComPesquisa('/Turma/TabelaPaginada', 'grid', `anoLetivo=${anoLetivo}`);
}

function onChangeAnoLetivo() {
    carregarListaDeTurmas($('#AnoLetivo').val());
}