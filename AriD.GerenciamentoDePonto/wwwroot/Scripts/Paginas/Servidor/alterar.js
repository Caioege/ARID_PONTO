$(document).ready(() => {
    assineEventoBotaoSalvar();
    assinechangeArquivoImagem();
    CarregueTabelaDeVinculosDeContrato();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        RequisicaoAjaxComCarregamento(
            '/Servidor/Salvar',
            'POST',
            ObtenhaFormularioSerializado('formulario-servidor'),
            function (data) {
                MensagemRodape('success', data.mensagem);
            });
    });
}

function CarregueTabelaDeVinculosDeContrato() {
    
}

function abrirSelecionarArquivoImagem() {
    $('#input-file').trigger('click');
}

function assinechangeArquivoImagem() {
    $('#input-file').on('change', function () {
        let formData = new FormData();
        let fileInput = document.querySelector('#input-file');

        if (fileInput.files.length > 0) {
            formData.append('file', fileInput.files[0]);
            formData.append('id', $('#Id').val());

            $.ajax({
                url: '/Foto/SalvarFotoServidor',
                type: 'POST',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
            }).done(function (data) {
                $('#fechar-offcanvas').trigger('click');
                if (!data.sucesso) {
                    $('#fotoInput').val('').trigger('change');
                    MensagemRodape('warning', 'Ocorreu um erro ao tentar salvar a foto.');
                } else {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Servidor/Alterar/' + $('#Id').val());
                }
            });
        }
    });
}

function removerFotoServidor() {
    $.ajax({
        url: '/Foto/RemoverFotoServidor',
        type: 'DELETE',
        data: { id: $('#Id').val() }
    }).done(function (data) {
        $('#fechar-offcanvas').trigger('click');
        if (!data.sucesso) {
            $('#fotoInput').val('').trigger('change');
            MensagemRodape('warning', 'Ocorreu um erro ao tentar remover a foto.');
        } else {
            MensagemRodape('success', data.mensagem);
            CarregarPagina('/Servidor/Alterar/' + $('#Id').val());
        }
    });
}