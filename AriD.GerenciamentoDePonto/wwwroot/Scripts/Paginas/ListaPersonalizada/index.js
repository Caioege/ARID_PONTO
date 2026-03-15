var _paginaAtualListas = 1;
var _limiteListas = 10;

$(document).ready(function () {
    carregarItensTabelaPaginada(1);
});

function carregarTabelaPaginadaComPesquisa() {
    carregarItensTabelaPaginada(1);
}

function carregarItensTabelaPaginada(pagina, limite) {

    if (!pagina) pagina = _paginaAtualListas;
    if (limite) _limiteListas = limite;

    RequisicaoAjaxComCarregamentoHtml('/ListaPersonalizada/TabelaPaginada', 'POST', {
        Pagina: pagina,
        Limit: _limiteListas,
        Pesquisa: $("#filtroTexto").val()
    }, function (htmlRetornado) {
        $("#grid").html(htmlRetornado);
    });
}

function AbrirModalLista(id) {
    RequisicaoAjaxComCarregamento('/ListaPersonalizada/Modal', 'GET', { listaId: id }, function (data) {
        if (data.sucesso) {
            $("#div-modal").html(data.html);
            $('#Permissoes').select2({ placeholder: "Selecione os operadores", dropdownParent: $('#_ModalListaPersonalizada') });
            $('#Servidores').select2({ placeholder: "Selecione os servidores", dropdownParent: $('#_ModalListaPersonalizada') });
            $('#_ModalListaPersonalizada').modal('show');
        } else {
            MensagemRodape('warning', data.mensagem || 'Não foi possível abrir a lista.');
        }
    });
}

function RemoverLista(id) {
    Swal.fire({
        text: "Tem certeza que deseja excluir permanentemente esta lista? Ao excluir, ela deixará de aparecer para você e para as pessoas que você compartilhou.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "SIM!",
        cancelButtonText: 'CANCELAR'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento('/ListaPersonalizada/Remova', 'POST', { listaId: id }, function (data) {
                if (data.sucesso) {
                    carregarItensTabelaPaginada(1);
                    MensagemRodape('success', data.mensagem);
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
        }
    });
}

function SalvarLista(id) {
    if (!$("#Nome").val()) {
        MensagemRodape('warning', "O nome da lista é obrigatório.");
        return;
    }
    
    var servidoresIds = $("#Servidores").val() || [];
    var permissoesIds = $("#Permissoes").val() || [];

    if (servidoresIds.length === 0) {
        MensagemRodape('warning', "Você precisa selecionar pelo menos 1 Servidor para a lista.");
        return;
    }

    var model = {
        Id: id,
        Nome: $("#Nome").val(),
        Itens: servidoresIds.map(function(sId) { return { ServidorId: sId }; }),
        Permissoes: permissoesIds.map(function(pId) { return { UsuarioId: pId }; })
    };

    RequisicaoAjaxComCarregamento('/ListaPersonalizada/Salvar', 'POST', model, function (data) {
        if (data.sucesso) {
            $('#_ModalListaPersonalizada').modal('hide');
            carregarItensTabelaPaginada(1);
            MensagemRodape('success', data.mensagem);
        } else {
            MensagemRodape('warning', data.mensagem);
        }
    });
}
