function AbrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/EventoAnual/Modal/',
        'GET',
        { eventoId: id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineSalvarCadastroModal();
                assineMascarasDoComponente($('#_Modal'));
                $('#_Modal').modal('show');
            }
        }
    );
}

function assineSalvarCadastroModal() {
    $('#btn-salvar-modal').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-evento');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/EventoAnual/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/EventoAnual/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function removerRegistro() {
    RequisicaoAjaxComCarregamento(
        '/EventoAnual/Remova/',
        'POST',
        { eventoId: $('#_Modal').find('#Id').val() },
        function (data) {
            if (data.sucesso) {
                $('#_Modal').modal('hide');
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/EventoAnual/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        }
    );
}

function exportarListagem() {
    Swal.fire({
        title: 'Selecione o formato de exportação',
        input: 'radio',
        inputOptions: {
            '0': 'PDF',
            '1': 'Excel (.xlsx)',
            '2': 'Texto (.txt)'
        },
        inputValue: '0',
        showCancelButton: true,
        confirmButtonText: '<i class="bx bxs-download"></i> Exportar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        inputValidator: (value) => {
            if (!value) {
                return 'Você precisa escolher um formato.'
            }
        }
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/Relatorio/ExportarEventosAnuais',
                'POST',
                { tipoDeExportacao: result.value },
                function (data) {
                    if (data.sucesso) {
                        MensagemRodape('success', 'O arquivo será baixado...');
                        downloadBase64File(data.base64, data.fileName, data.mimeType);
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
        }
    });
}