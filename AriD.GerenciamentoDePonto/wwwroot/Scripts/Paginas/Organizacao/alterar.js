$(document).ready(() => {
    assineEventoBotaoSalvar();

    if ($('#Id').val() != '0') {
        CarregueTabelaDeUnidadesOrganizacionais();
    }
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        RequisicaoAjaxComCarregamento(
            '/Organizacao/Salvar',
            'POST',
            ObtenhaFormularioSerializado('formulario-organizacao'),
            function (data) {
                MensagemRodape('success', data.mensagem);
                $('#Id').val(data.id);
            });
    });
}

function CarregueTabelaDeUnidadesOrganizacionais() {
    $('#Adicional').val(JSON.stringify({ OrganizacaoId: $('#Id').val() }));
    $.ajax({
        url: '/UnidadeOrganizacional/TabelaPaginada',
        type: 'GET',
        data: { Adicional: $('#Adicional').val() }
    }).done(function (data) {
        $('#grid').html(data);
    });
}