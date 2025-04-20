function abrirModalExportarCSV() {
    RequisicaoAjaxComCarregamento(
        '/RegistroDePonto/AbrirModalExportarCSV',
        'GET',
        { },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_ModalExportarCSV'));
                $('#_ModalExportarCSV').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function processarExportarCSV() {
    let dadosFormulario = ObtenhaFormularioSerializado('formulario-exportar-csv');
    if (!dadosFormulario.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
        return;
    }

    RequisicaoAjaxComCarregamento(
        '/Relatorio/ProcessarExportarCSVRegistrosDeFrequencia',
        'POST',
        {
            escolaId: $('#_ModalExportarCSV #EscolaId').val(),
            dataInicio: $('#_ModalExportarCSV #DataInicio').val(),
            dataFim: $('#_ModalExportarCSV #DataFim').val(),
            separador: $('#_ModalExportarCSV #Separador').val()
        },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', 'O arquivo será baixado...');
                downloadBase64File(data.base64, data.fileName, data.mimeType);
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}