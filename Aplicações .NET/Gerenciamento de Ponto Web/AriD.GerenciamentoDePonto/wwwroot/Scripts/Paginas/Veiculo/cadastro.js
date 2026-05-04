$(function () {
    assineSalvarCadastro();
    assineMascarasDoComponente($('body'));
    
    const veiculoId = $('#Id').val();
    if (veiculoId > 0) {
        carregarItensChecklist(veiculoId);
    }
});

function assineSalvarCadastro() {
    $('#btn-salvar').on('click', function () {
        let formulario = ObtenhaFormularioSerializado('formulario-veiculo');
        
        if (!formulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Veiculo/Salvar/',
            'POST',
            formulario.dados,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    // Se for novo, vai para tela de edição para liberar checklist/manutenção
                    if ($('#Id').val() == 0) {
                        CarregarPagina('/Veiculo/Alterar/' + data.id);
                    } else {
                        CarregarPagina('/Veiculo/Index');
                    }
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });
}

function carregarItensChecklist(veiculoId) {
    const $lista = $('#lista-checklist');
    
    $.get('/Veiculo/ObtenhaChecklist', { veiculoId: veiculoId }, function (data) {
        if (data.sucesso) {
            $lista.empty();
            if (data.itens.length === 0) {
                $lista.append('<div class="p-4 text-center text-muted">Ainda não há itens de checklist cadastrados.</div>');
            } else {
                data.itens.forEach(function (item) {
                    $lista.append(`
                        <div class="list-group-item d-flex justify-content-between align-items-center">
                            <span><i class='bx bx-check text-success me-2'></i>${item.descricao}</span>
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
    const veiculoId = $('#Id').val();
    const $input = $('#novo-item-checklist');
    const descricao = $input.val();

    if (!descricao) return;

    $.post('/Veiculo/SalvarItemChecklist', { veiculoId: veiculoId, descricao: descricao }, function (data) {
        if (data.sucesso) {
            $input.val('');
            carregarItensChecklist(veiculoId);
            MensagemRodape('success', 'Item adicionado ao checklist.');
        } else {
            MensagemRodape('warning', data.mensagem);
        }
    });
}

function removerItemChecklist(id) {
    const veiculoId = $('#Id').val();
    $.post('/Veiculo/RemoverItemChecklist', { id: id }, function (data) {
        if (data.sucesso) {
            carregarItensChecklist(veiculoId);
            MensagemRodape('success', 'Item removido.');
        }
    });
}

// Lógica de Manutenção (Sub-modal)
function abrirModalManutencao(id) {
    var veiculoId = $('#Id').val();
    $.get('/Manutencao/Modal', { maintenanceId: id, veiculoId: veiculoId }, function (res) {
        if (res.sucesso) {
            $('#div-modal-suplementar').html(res.html);
            var modal = new bootstrap.Modal(document.getElementById('modal-manutencao'));
            modal.show();
        }
    });
}

function salvarManutencao() {
    let formulario = ObtenhaFormularioSerializado('form-manutencao');

    if (!formulario.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
        return;
    }

    $.post('/Manutencao/Salvar', formulario.dados, function (res) {
        if (res.sucesso) {
            MensagemRodape('success', res.mensagem);
            bootstrap.Modal.getInstance(document.getElementById('modal-manutencao')).hide();
            
            // Recarregar a página para atualizar a tabela e a KM do veículo
            setTimeout(() => {
                CarregarPagina('/Veiculo/Alterar/' + $('#Id').val());
            }, 500);
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
                    CarregarPagina('/Veiculo/Alterar/' + $('#Id').val());
                } else {
                    MensagemRodape('warning', res.mensagem);
                }
            });
        }
    });
}
