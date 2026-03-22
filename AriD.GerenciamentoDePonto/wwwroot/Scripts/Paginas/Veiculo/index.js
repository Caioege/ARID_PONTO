function abrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/Veiculo/Modal/',
        'GET',
        { veiculoId: id },
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
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-veiculo');
        
        let placa = $('#Placa').val();
         
        if (!placa) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Veiculo/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Veiculo/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function removerRegistro() {
    Swal.fire({
        title: 'Você tem certeza?',
        text: "Deseja realmente remover o veículo?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sim, remover!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/Veiculo/Remova/',
                'POST',
                { veiculoId: $('#_Modal').find('#Id').val() },
                function (data) {
                    if (data.sucesso) {
                        $('#_Modal').modal('hide');
                        MensagemRodape('success', data.mensagem);
                        CarregarPagina('/Veiculo/Index');
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                }
            );
        }
    });
}
