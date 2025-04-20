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

function removerGrupoDePermissao(id) {
    Swal.fire({
        text: "Tem certeza que deseja remover esse grupo de permissão?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "SIM",
        cancelButtonText: 'NÃO'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/GrupoDePermissao/Remova',
                'POST',
                { grupoDePermissaoId: id },
                function (data) {
                    if (data.sucesso) {
                        MensagemRodape('success', data.mensagem);
                        $('#_Modal').modal('hide');
                        $('#btn-pesquisar').trigger('click');
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
        }
    });
}