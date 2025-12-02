function abrirModalAtestado() {
    $('#_ModalAtestado').modal('show');
}

function salvarAtestado() {
    let dadosFormulario = ObtenhaFormularioSerializado('formulario-registro-manual');
    if (!dadosFormulario.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
        return;
    }

    var formData = new FormData();
    formData.append('VinculoDeTrabalhoId', $('#_ModalAtestado #VinculoDeTrabalhoId').val());
    formData.append('Observacao', $('#_ModalAtestado #Observacao').val());
    formData.append('JustificativaDeAusenciaId', $('#_ModalAtestado #JustificativaId').val());
    formData.append('DataInicialAtestado', $('#_ModalAtestado #DataInicial').val());
    formData.append('DataFinalAtestado', $('#_ModalAtestado #DataFinal').val());

    var fileInput = $('input[name="Anexo"]')[0];
    if (fileInput.files.length > 0) {
        formData.append('Imagem', fileInput.files[0]);
    }

    AbrirCaixaDeCarregamento('Carregando...');

    setTimeout(function () {
        $.ajax({
            url: '/PortalServidor/SalvarPontoManual',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            error: function (jqXHR, textStatus, errorThrown) {
                FecharCaixaDeCarregamento();
                MensagemRodape('warning', 'Ocorreu um erro inesperado ao fazer a requisição. Tente novamente mais tarde.');
            }
        }).done(function (data) {
            FecharCaixaDeCarregamento();

            if (data.sucesso) {
                $('#_ModalAtestado').modal('hide');
                CarregarPagina('/PortalServidor/Atestados');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
    }, 750);
}