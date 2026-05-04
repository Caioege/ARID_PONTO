function AbrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/JustificativaDeAusencia/Modal/',
        'GET',
        { justificativaId: id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_Modal'));
                assineSalvarCadastroModal();
                $('#_Modal').modal('show');
            }
        }
    );
}

function assineSalvarCadastroModal() {
    $('#btn-salvar-modal').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-justificativa');;
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/JustificativaDeAusencia/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/JustificativaDeAusencia/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function removerRegistro() {
    RequisicaoAjaxComCarregamento(
        '/JustificativaDeAusencia/Remova/',
        'POST',
        { justificativaId: $('#_Modal').find('#Id').val() },
        function (data) {
            if (data.sucesso) {
                $('#_Modal').modal('hide');
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/JustificativaDeAusencia/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        }
    );
}

function onChangeTipoDeLimite() {
    let tipoDeLimite = $('#TipoDeLimite').val();
    if (tipoDeLimite == '0' || tipoDeLimite == 'NaoUtiliza') {
        $('#LabelTotalDeUsos').removeClass('obrigatorio');
        $('#LabelTotalDeUsos').parent().hide();
    } else {
        $('#LabelTotalDeUsos').addClass('obrigatorio');
        $('#LabelTotalDeUsos').parent().show();
    }
}