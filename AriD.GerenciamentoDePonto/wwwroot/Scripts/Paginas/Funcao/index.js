function AbrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/Funcao/Modal/',
        'GET',
        { funcaoId: id },
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
            '/Funcao/Salvar/',
            'POST',
            ObtenhaFormularioSerializado('formulario-funcao'),
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Funcao/Index');
                } else {
                    MensagemRodape('Warning', data.mensagem);
                }
            }
        );
    });
}