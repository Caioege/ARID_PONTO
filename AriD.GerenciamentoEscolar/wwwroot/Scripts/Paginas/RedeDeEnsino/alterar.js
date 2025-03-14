$(document).ready(() => {
    assineEventoBotaoSalvar();

    if ($('#Id').val() != '0') {
        CarregueTabelaDeescolasOrganizacionais();
        assineClickImagem();
    }
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-redeDeEnsino');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/RedeDeEnsino/Salvar',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/RedeDeEnsino/Alterar/' + data.id)
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function CarregueTabelaDeescolasOrganizacionais() {
    $('#Adicional').val(JSON.stringify({ RedeDeEnsinoId: $('#Id').val() }));
    $.ajax({
        url: '/Escola/TabelaPaginada',
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
                url: '/RedeDeEnsino/PostBrasao',
                type: 'POST',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
            }).done(function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/RedeDeEnsino/Alterar/' + $('#Id').val());
                } else {
                    $('#fotoInput').val('').trigger('change');
                    MensagemRodape('warning', data.mensagem);
                }
            });
        }
    });
}