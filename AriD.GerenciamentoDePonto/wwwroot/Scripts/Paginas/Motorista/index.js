function abrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/Motorista/Modal/',
        'GET',
        { motoristaId: id },
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
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-motorista');
        
        let servidorId = $('#ServidorId').val();
        let numeroCNH = $('#NumeroCNH').val();
         
        if (!servidorId || !numeroCNH) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Motorista/Salvar/',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Motorista/Index');
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
        text: "Deseja realmente remover o motorista?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sim, remover!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/Motorista/Remova/',
                'POST',
                { motoristaId: $('#_Modal').find('#Id').val() },
                function (data) {
                    if (data.sucesso) {
                        $('#_Modal').modal('hide');
                        MensagemRodape('success', data.mensagem);
                        CarregarPagina('/Motorista/Index');
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                }
            );
        }
    });
}

function carregarHistoricoSituacao() {
    let id = $('#_Modal').find('#Id').val();
    $.ajax({
        url: '/Motorista/ObtenhaHistoricoSituacao',
        type: 'GET',
        data: { motoristaId: id },
        success: function(data) {
            if (data.sucesso && data.historico) {
                let tbody = $('#tabela-historico-situacao tbody');
                tbody.empty();
                if (data.historico.length === 0) {
                    tbody.append('<tr><td colspan="4" class="text-center">Nenhum histórico encontrado.</td></tr>');
                    return;
                }
                data.historico.forEach(function(h) {
                    tbody.append(`<tr><td>${h.dataAlteracao}</td><td>${h.situacaoAnterior}</td><td>${h.situacaoNova}</td><td>${h.usuario}</td></tr>`);
                });
            }
        }
    });
}

// Filtros
$(function () {
    $('#FiltroSituacao, #FiltroCategoriaCNH').on('change', function () {
        aplicarFiltrosMotorista();
    });

    function aplicarFiltrosMotorista() {
        var params = {
            Situacao: $('#FiltroSituacao').val() !== "" ? parseInt($('#FiltroSituacao').val()) : null,
            CategoriaCNH: $('#FiltroCategoriaCNH').val() !== "" ? parseInt($('#FiltroCategoriaCNH').val()) : null
        };
        $('#Adicional').val(JSON.stringify(params));
        carregarTabelaPaginadaComPesquisa('/Motorista/TabelaPaginada');
    }
});
