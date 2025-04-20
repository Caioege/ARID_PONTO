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

        if (perfil == '2' || perfil == 'Escola') {
            $('#_Modal #label-unidade').addClass('obrigatorio');
            $('#_Modal #div-unidade').show();
        } else {
            $('#_Modal #div-unidade').hide();
            $('#_Modal #label-unidade').removeClass('obrigatorio');
            $('#_Modal #UnidadeOrganizacionalId').val('').trigger('change');
        }

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

function removerUsuario(id) {
    Swal.fire({
        text: "Tem certeza que deseja remover esse usuário?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "SIM",
        cancelButtonText: 'NÃO'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/Usuario/Remover',
                'POST',
                { id },
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