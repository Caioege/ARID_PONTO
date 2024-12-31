function AbrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/TipoDoVinculoDeTrabalho/Modal/',
        'GET',
        { tipoDoVinculoDeTrabalhoId: id },
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
        RequisicaoAjaxComCarregamento(
            '/TipoDoVinculoDeTrabalho/Salvar/',
            'POST',
            ObtenhaFormularioSerializado('formulario-tipo'),
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/TipoDoVinculoDeTrabalho/Index');
                } else {
                    MensagemRodape('Warning', data.mensagem);
                }
            }
        );
    });
}