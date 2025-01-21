$(document).ready(() => {
    assineEventoBotaoSalvar();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        RequisicaoAjaxComCarregamento(
            '/Servidor/Salvar',
            'POST',
            ObtenhaFormularioSerializado('formulario-servidor'),
            function (data) {
                MensagemRodape('success', data.mensagem);
                $('#Id').val(data.id);
            });
    });
}