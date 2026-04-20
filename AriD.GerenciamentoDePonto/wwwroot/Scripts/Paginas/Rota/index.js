function abrirModal(id) {
    if (id > 0) {
        CarregarPagina('/Rota/Cadastro/' + id);
    } else {
        CarregarPagina('/Rota/Cadastro/0');
    }
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

// Filtros
function aplicarFiltrosRota() {
    var params = {
        Situacao: $('#FiltroSituacao').val() !== "" ? parseInt($('#FiltroSituacao').val()) : null,
        Recorrente: $('#FiltroRecorrente').val() !== "" ? ($('#FiltroRecorrente').val() === "true") : null
    };
    $('#Adicional').val(JSON.stringify(params));
    carregarTabelaPaginadaComPesquisa('/Rota/TabelaPaginada');
}

$(function () {
    $('#FiltroSituacao, #FiltroRecorrente').on('change', function () {
        aplicarFiltrosRota();
    });
});
