function AbrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/EventoAnual/Modal/',
        'GET',
        { eventoId: id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineSalvarCadastroModal();
                assineMascarasDoComponente($('#_Modal'));
                $('#_Modal').modal('show');
            }
        }
    );
}

function assineSalvarCadastroModal() {
    $('#btn-salvar-modal').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-evento');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/EventoAnual/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/EventoAnual/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function removerRegistro() {
    RequisicaoAjaxComCarregamento(
        '/EventoAnual/Remova/',
        'POST',
        { eventoId: $('#_Modal').find('#Id').val() },
        function (data) {
            if (data.sucesso) {
                $('#_Modal').modal('hide');
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/EventoAnual/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        }
    );
}