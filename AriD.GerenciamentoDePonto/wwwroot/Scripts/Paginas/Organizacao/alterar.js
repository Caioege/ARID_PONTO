$(document).ready(() => {
    assineEventoBotaoSalvar();

    if ($('#Id').val() != '0') {
        CarregueTabelaDeUnidadesOrganizacionais();
    }
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-organizacao');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Organizacao/Salvar',
            'POST',
            dadosFormulario.dados,
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