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
            '8': 'Desabilitar registro de atestado no aplicativo/portal'
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
            executarAcaoEmLote(result.value);
        }
    });
}

function executarAcaoEmLote(opcaoSelecionada) {
    RequisicaoAjaxComCarregamento(
        '/Servidor/ExecutarAcaoEmLote',
        'POST',
        { acao: opcaoSelecionada },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/Servidor/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}