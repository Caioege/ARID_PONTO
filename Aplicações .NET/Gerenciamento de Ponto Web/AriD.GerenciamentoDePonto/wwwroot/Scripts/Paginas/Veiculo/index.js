function abrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/Veiculo/Modal/',
        'GET',
        { veiculoId: id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineSalvarCadastroModal();
                assineMascarasDoComponente($('#_Modal'));
                $('#_Modal').modal('show');
                
                if (id > 0) {
                    carregarItensChecklist(id);
                }
            }
        }
    );
}

function carregarItensChecklist(veiculoId) {
    const $lista = $('#lista-checklist');
    
    $.get('/Veiculo/ObtenhaChecklist', { veiculoId: veiculoId }, function (data) {
        if (data.sucesso) {
            $lista.empty();
            if (data.itens.length === 0) {
                $lista.append('<div class="p-3 text-center text-muted">Ainda não há itens de checklist.</div>');
            } else {
                data.itens.forEach(function (item) {
                    $lista.append(`
                        <div class="list-group-item d-flex justify-content-between align-items-center">
                            <span>${item.descricao}</span>
                            <button type="button" class="btn btn-sm text-danger" onclick="removerItemChecklist(${item.id})">
                                <i class='bx bx-trash'></i>
                            </button>
                        </div>
                    `);
                });
            }
        }
    });
}

function adicionarItemChecklist() {
    const veiculoId = $('#_Modal').find('#Id').val();
    const $input = $('#novo-item-checklist');
    const descricao = $input.val();

    if (!descricao) return;

    $.post('/Veiculo/SalvarItemChecklist', { veiculoId: veiculoId, descricao: descricao }, function (data) {
        if (data.sucesso) {
            $input.val('');
            carregarItensChecklist(veiculoId);
            MensagemRodape('success', 'Item adicionado.');
        } else {
            MensagemRodape('warning', data.mensagem);
        }
    });
}

function removerItemChecklist(id) {
    const veiculoId = $('#_Modal').find('#Id').val();
    $.post('/Veiculo/RemoverItemChecklist', { id: id }, function (data) {
        if (data.sucesso) {
            carregarItensChecklist(veiculoId);
            MensagemRodape('success', 'Item removido.');
        }
    });
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

// Filtros
function aplicarFiltrosVeiculo() {
    var params = {
        Situacao: $('#FiltroSituacao').val() !== "" ? parseInt($('#FiltroSituacao').val()) : null,
        TipoCombustivel: $('#FiltroTipoCombustivel').val() !== "" ? parseInt($('#FiltroTipoCombustivel').val()) : null
    };
    $('#Adicional').val(JSON.stringify(params));
    carregarTabelaPaginadaComPesquisa('/Veiculo/TabelaPaginada');
}

$(function () {
    $('#FiltroSituacao, #FiltroTipoCombustivel').on('change', function () {
        aplicarFiltrosVeiculo();
    });
});

function carregarManutencoes() {
    var veiculoId = $('#_Modal').find('#Id').val();
    $('#lista-manutencoes').html('<div class="p-3 text-center text-muted">Carregando histórico...</div>');
    $.get('/Manutencao/Index', { veiculoId: veiculoId }, function (html) {
        $('#lista-manutencoes').html(html);
    });
}

function abrirModalManutencao(id) {
    var veiculoId = $('#_Modal').find('#Id').val();
    $.get('/Manutencao/Modal', { maintenanceId: id, veiculoId: veiculoId }, function (res) {
        if (res.sucesso) {
            $('body').append('<div id="area-modal-manutencao"></div>');
            $('#area-modal-manutencao').html(res.html);
            var modal = new bootstrap.Modal(document.getElementById('modal-manutencao'));
            modal.show();
            
            document.getElementById('modal-manutencao').addEventListener('hidden.bs.modal', function () {
                $('#area-modal-manutencao').remove();
            });
        }
    });
}

function salvarManutencao() {
    if (!$('#form-manutencao')[0].checkValidity()) {
        $('#form-manutencao')[0].reportValidity();
        return;
    }

    var formData = $('#form-manutencao').serialize();
    $.post('/Manutencao/Salvar', formData, function (res) {
        if (res.sucesso) {
            MensagemRodape('success', res.mensagem);
            bootstrap.Modal.getInstance(document.getElementById('modal-manutencao')).hide();
            carregarManutencoes();
            
            // Atualizar KM na tela principal do veículo se necessário
            var km = $('#KmNaManutencao').val();
            if (parseInt(km) > parseInt($('#QuilometragemAtual').val())) {
                $('#QuilometragemAtual').val(km);
            }
        } else {
            MensagemRodape('warning', res.mensagem);
        }
    });
}

function removerManutencao(id) {
    Swal.fire({
        title: 'Você tem certeza?',
        text: "Deseja remover este registro de manutenção?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sim, remover!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Manutencao/Remova', { maintenanceId: id }, function (res) {
                if (res.sucesso) {
                    MensagemRodape('success', res.mensagem);
                    carregarManutencoes();
                } else {
                    MensagemRodape('warning', res.mensagem);
                }
            });
        }
    });
}
