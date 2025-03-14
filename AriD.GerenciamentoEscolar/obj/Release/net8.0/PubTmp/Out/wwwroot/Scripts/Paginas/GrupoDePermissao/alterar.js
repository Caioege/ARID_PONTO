$(document).ready(function () {
    assineSalvar();
    removerRegistro();
});

function assineSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-grupodepermissao');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        let grupoDePermissao = {
            Id: $('#Id').val(),
            Sigla: $('#Sigla').val(),
            Descricao: $('#Descricao').val(),
            PerfilDeAcesso: $('#PerfilDeAcesso').val(),
            Ativo: $('#Ativo').val(),
            ListaDePermissao: []
        };

        $.each($('.table-permissao td [type="checkbox"]'), function (i, checkbox) {
            let _this = $(checkbox);
            grupoDePermissao.ListaDePermissao.push({
                Id: _this.data('id'),
                GrupoDePermissaoId: $('#Id').val(),
                EnumeradorNome: _this.data('enumeradornome'),
                ValorDoEnumerador: _this.data('valorenumerador'),
                PermissaoAtiva: _this.is(':checked')
            });
        });

        RequisicaoAjaxComCarregamento(
            '/GrupoDePermissao/Salvar/',
            'POST',
            { grupoDePermissao },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/GrupoDePermissao/Alterar?grupoDePermissaoId=' + $('#Id').val());
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function removerRegistro() {
    $('#btn-remover').on('click', function () {
        RequisicaoAjaxComCarregamento(
            '/GrupoDePermissao/Remova/',
            'DELETE',
            { grupoDePermissaoId: $('#Id').val() },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/GrupoDePermissao/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}