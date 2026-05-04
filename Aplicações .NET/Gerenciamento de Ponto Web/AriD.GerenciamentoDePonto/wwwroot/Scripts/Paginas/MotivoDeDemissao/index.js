function AbrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/MotivoDeDemissao/Modal/',
        'GET',
        { motivoId: id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineSalvarCadastroModal();
                $('#_Modal').modal('show');
            }
        }
    );
}

function assineSalvarCadastroModal() {
    $('#btn-salvar-modal').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-motivo');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/MotivoDeDemissao/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/MotivoDeDemissao/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function removerRegistro() {
    RequisicaoAjaxComCarregamento(
        '/MotivoDeDemissao/Remova/',
        'POST',
        { motivoId: $('#_Modal').find('#Id').val() },
        function (data) {
            if (data.sucesso) {
                $('#_Modal').modal('hide');
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/MotivoDeDemissao/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        }
    );
}