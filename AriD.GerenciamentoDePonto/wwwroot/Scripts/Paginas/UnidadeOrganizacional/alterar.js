$(document).ready(() => {
    assineEventoBotaoSalvar();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        RequisicaoAjaxComCarregamento(
            '/UnidadeOrganizacional/Salvar',
            'POST',
            ObtenhaFormularioSerializado('formulario-unidade'),
            function (data) {
                MensagemRodape('success', data.mensagem);
                $('#Id').val(data.id);
            });
    });
}