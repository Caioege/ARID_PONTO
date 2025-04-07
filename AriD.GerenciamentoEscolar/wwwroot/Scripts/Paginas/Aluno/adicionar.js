$(document).ready(() => {
    assineEventoBotaoSalvar();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-aluno');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Aluno/Salvar',
            'POST',
            dadosFormulario.dados,
            function (data) {
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/Aluno/Alterar/' + data.id);
            });
    });
}