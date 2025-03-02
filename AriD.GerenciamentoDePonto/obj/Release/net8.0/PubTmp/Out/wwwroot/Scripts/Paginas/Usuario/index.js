function AbrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/Usuario/Modal/',
        'GET',
        { usuarioId: id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineSalvarCadastroModal();
                assineChangeGrupoDePermissao();
                assineMascarasDoComponente($('#_Modal'));
                $('#_Modal').modal('show');
            }
        }
    );
}

function assineSalvarCadastroModal() {
    $('#btn-salvar-modal').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-usuario');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Usuario/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Usuario/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function assineChangeGrupoDePermissao() {
    $('#PerfilDeAcesso').on('change', function () {
        let perfil = $(this).val();
        $('#GrupoDePermissaoId').html('');

        if (perfil) {
            $.ajax({
                url: '/Usuario/CarregueListaDeGruposDePermissao',
                type: 'GET',
                data: { perfil }
            }).done(function (data) {
                if (data.sucesso) {
                    $("#GrupoDePermissaoId").append("<option value=''></option>");
                    $.each(data.gruposDePermissao, function (i, item) {
                        $("#GrupoDePermissaoId").append("<option value='" + item.codigo + "'>" + item.descricao + "</option>");
                    });

                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
        }
    });

    if ($('#_Modal').find('#Id').val() == '0') {
        $('#PerfilDeAcesso').trigger('change');
    }
}