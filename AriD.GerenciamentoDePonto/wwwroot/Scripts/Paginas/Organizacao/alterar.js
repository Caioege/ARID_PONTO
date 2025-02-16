$(document).ready(() => {
    assineEventoBotaoSalvar();

    if ($('#Id').val() != '0') {
        CarregueTabelaDeUnidadesOrganizacionais();
        assineClickImagem();
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

var assineClickImagem = function () {
    $('.img-foto').on('click', function () {
        $('#fotoInput').trigger('click');
    });

    $('#fotoInput').on('change', function () {
        let formData = new FormData();
        let fileInput = document.querySelector('#fotoInput');

        if (fileInput.files.length > 0) {
            formData.append('file', fileInput.files[0]);
            formData.append('id', $('#Id').val());

            $.ajax({
                url: '/Organizacao/PostBrasao',
                type: 'POST',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
            }).done(function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Organizacao/Alterar/' + $('#Id').val());
                } else {
                    $('#fotoInput').val('').trigger('change');
                    MensagemRodape('warning', data.mensagem);
                }
            });
        }
    });
}