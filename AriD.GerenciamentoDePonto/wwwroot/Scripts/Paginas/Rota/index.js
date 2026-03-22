function abrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/Rota/Modal/',
        'GET',
        { rotaId: id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineEventosModal();
                $('#_Modal').modal('show');
            }
        }
    );
}

function assineEventosModal() {
    $('#btn-adicionar-parada').on('click', function () {
        let endereco = $('#novaParadaEndereco').val();
        let link = $('#novaParadaLink').val();
        let obs = $('#novaParadaObs').val();

        if (!endereco) {
            MensagemRodape('warning', 'Informe o endereço da parada!');
            return;
        }

        let idTemporario = 0; // 0 significa novo para o backend

        let novaLinha = `
            <tr data-id="${idTemporario}" data-entregue="false" data-endereco="${endereco}" data-link="${link}" data-obs="${obs}">
                <td>${endereco}</td>
                <td>${link}</td>
                <td>${obs}</td>
                <td><span class="badge bg-secondary">Pendente</span></td>
                <td>
                    <button type="button" class="btn btn-sm btn-icon btn-danger btn-remover-parada" onclick="removerLinhaParada(this)"><i class='bx bx-trash'></i></button>
                </td>
            </tr>
        `;

        $('#tabela-paradas tbody').append(novaLinha);
        $('#novaParadaEndereco').val('');
        $('#novaParadaLink').val('');
        $('#novaParadaObs').val('');
    });

    $('#btn-salvar-modal').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-rota');
        
        let motoristaId = $('#MotoristaId').val();
        let descricao = $('#Descricao').val();
         
        if (!motoristaId || !descricao) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos (Motorista, Descrição)!');
            return;
        }

        // Colher as paradas
        let paradas = [];
        $('#tabela-paradas tbody tr').each(function() {
            let row = $(this);
            paradas.push({
                Id: parseInt(row.attr('data-id')),
                Endereco: row.attr('data-endereco'),
                Link: row.attr('data-link'),
                Observacao: row.attr('data-obs'),
                Entregue: row.attr('data-entregue') === "true"
            });
        });

        // Montar payload final
        let payload = {
            rota: dadosFormulario.dados,
            paradas: paradas
        };

        // Necessário JSON POST para mandar objeto complexo com lista
        $.ajax({
            url: '/Rota/Salvar/',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            beforeSend: function () { BloquearTela(); },
            complete: function () { DesbloquearTela(); },
            success: function (data) {
                if (data.sucesso) {
                    $('#_Modal').modal('hide');
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Rota/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            },
            error: function (xhr) {
                MensagemAviso('Erro ao salvar os dados.', 'Tente novamente.', 'error');
            }
        });
    });
}

function removerLinhaParada(button) {
    $(button).closest('tr').remove();
}

function removerRegistro() {
    Swal.fire({
        title: 'Você tem certeza?',
        text: "Deseja realmente remover esta rota e todas suas paradas?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sim, remover!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/Rota/Remova/',
                'POST',
                { rotaId: $('#_Modal').find('#Id').val() },
                function (data) {
                    if (data.sucesso) {
                        $('#_Modal').modal('hide');
                        MensagemRodape('success', data.mensagem);
                        CarregarPagina('/Rota/Index');
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                }
            );
        }
    });
}

function carregarHistoricoExecucoes() {
    let id = $('#_Modal').find('#Id').val();
    $.ajax({
        url: '/Rota/ObtenhaHistoricoExecucoes',
        type: 'GET',
        data: { rotaId: id },
        success: function(data) {
            if (data.sucesso && data.historico) {
                let tbody = $('#tabela-historico-execucoes tbody');
                tbody.empty();
                if (data.historico.length === 0) {
                    tbody.append('<tr><td colspan="4" class="text-center">Nenhuma execução registrada.</td></tr>');
                    return;
                }
                data.historico.forEach(function(h) {
                    tbody.append(`<tr><td>${h.dataHoraInicio}</td><td>${h.usuarioInicio}</td><td>${h.dataHoraFim}</td><td>${h.usuarioFim}</td></tr>`);
                });
            }
        }
    });
}

// Filtros
$(function () {
    $('#FiltroSituacao, #FiltroRecorrente').on('change', function () {
        aplicarFiltrosRota();
    });

    function aplicarFiltrosRota() {
        var params = {
            Situacao: $('#FiltroSituacao').val() !== "" ? parseInt($('#FiltroSituacao').val()) : null,
            Recorrente: $('#FiltroRecorrente').val() !== "" ? ($('#FiltroRecorrente').val() === "true") : null
        };
        $('#Adicional').val(JSON.stringify(params));
        carregarTabelaPaginadaComPesquisa('/Rota/TabelaPaginada');
    }
});
