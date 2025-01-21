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
        RequisicaoAjaxComCarregamento(
            '/Departamento/Salvar/',
            'POST',
            ObtenhaFormularioSerializado('formulario-departamento'),
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Departamento/Index');
                } else {
                    MensagemRodape('Warning', data.mensagem);
                }
            }
        );
    });
}