function AbrirModal() {
    RequisicaoAjaxComCarregamento(
        '/GrupoDePermissao/Modal/',
        'GET',
        { },
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
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-grupodepermissao');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/GrupoDePermissao/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/GrupoDePermissao/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}