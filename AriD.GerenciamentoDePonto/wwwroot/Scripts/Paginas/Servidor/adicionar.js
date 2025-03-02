$(document).ready(() => {
    assineEventoBotaoSalvar();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-servidor');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Servidor/Salvar',
            'POST',
            dadosFormulario.dados,
            function (data) {
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/Servidor/Alterar/' + data.id)
            });
    });
}