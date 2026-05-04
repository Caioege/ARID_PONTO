function AbrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/Departamento/Modal/',
        'GET',
        { departamentoId: id },
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
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-departamento');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Departamento/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Departamento/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function removerRegistro() {
    RequisicaoAjaxComCarregamento(
        '/Departamento/Remova/',
        'POST',
        { departamentoId: $('#_Modal').find('#Id').val() },
        function (data) {
            if (data.sucesso) {
                $('#_Modal').modal('hide');
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/Departamento/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        }
    );
}