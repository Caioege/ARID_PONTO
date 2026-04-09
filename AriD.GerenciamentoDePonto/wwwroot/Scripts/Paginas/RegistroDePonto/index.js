$(document).ready(function () {
    assineMascarasDoComponente($('.card-body'));
});

function pesquisarRegistros() {
    const adicional = {
        DataInicio: $('#FiltroDataInicio').val(),
        DataFim: $('#FiltroDataFim').val(),
        FiltroUnidadeId: $('#FiltroUnidadeId').val()
    };

    $('#Adicional').val(JSON.stringify(adicional));

    carregarTabelaPaginadaComPesquisa('/RegistroDePonto/TabelaPaginada');
}
