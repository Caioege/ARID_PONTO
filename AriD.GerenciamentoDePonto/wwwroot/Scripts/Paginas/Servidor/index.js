function abrirModalAcaoEmLote(nomenclatura) {
    Swal.fire({
        title: 'Ações em Lote',
        text: `Selecione a ação que deseja aplicar a todos os ${nomenclatura}:`,
        icon: 'question',
        input: 'select',
        inputOptions: {
            '1': 'Habilitar acesso ao aplicativo/portal',
            '2': 'Desabilitar acesso ao aplicativo/portal',
            '3': 'Habilitar registro de ponto no aplicativo',
            '4': 'Desabilitar registro de ponto no aplicativo',
            '5': 'Habilitar registro manual no aplicativo/portal',
            '6': 'Desabilitar registro manual no aplicativo/portal',
            '7': 'Habilitar registro de atestado no aplicativo/portal',
            '8': 'Desabilitar registro de atestado no aplicativo/portal',
            '9': 'Alterar comprovação de PONTO no App para NENHUMA',
            '10': 'Alterar comprovação de PONTO no App para TIRAR SELFIE',
            '11': 'Alterar comprovação de PONTO no App para LIVENESS FACIAL'
        },
        inputPlaceholder: 'Selecione uma opção',
        showCancelButton: true,
        confirmButtonText: 'Executar',
        cancelButtonText: 'Cancelar',
        inputValidator: (value) => {
            if (!value) {
                return 'Você precisa selecionar uma ação!';
            }
        }
    }).then((result) => {
        if (result.isConfirmed) {
            if (result.value == '9' || result.value == '10' || result.value == '11') {
                Swal.fire({
                    title: 'Motivo da Alteração',
                    text: "Por ser uma configuração de segurança, você precisa informar o motivo desta alteração em lote.",
                    input: 'textarea',
                    icon: 'warning',
                    inputPlaceholder: 'Justifique a alteração da regra de Liveness...',
                    showCancelButton: true,
                    confirmButtonText: 'Confirmar e Executar',
                    cancelButtonText: 'Cancelar',
                    inputValidator: (value) => {
                        if (!value || value.trim().length === 0) {
                            return 'O motivo é obrigatório!';
                        }
                    }
                }).then((motivoResult) => {
                    if (motivoResult.isConfirmed) {
                        executarAcaoEmLote(result.value, motivoResult.value);
                    }
                });
            } else {
                executarAcaoEmLote(result.value, null);
            }
        }
    });
}

function executarAcaoEmLote(opcaoSelecionada, motivo) {
    RequisicaoAjaxComCarregamento(
        '/Servidor/ExecutarAcaoEmLote',
        'POST',
        { acao: opcaoSelecionada, motivo: motivo },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/Servidor/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}