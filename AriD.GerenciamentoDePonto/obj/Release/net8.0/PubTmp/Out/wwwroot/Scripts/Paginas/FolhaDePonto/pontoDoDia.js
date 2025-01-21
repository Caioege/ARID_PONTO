$(document).ready(() => {
    assineChangeUnidadeOrganizacional();
    $('#UnidadeOrganizacionalId').trigger('change');
});

function assineChangeUnidadeOrganizacional() {
    $('#UnidadeOrganizacionalId').on('change', function () {
        let campoFuncao = $('#FuncaoId');
        let campoDepartamento = $('#DepartamentoId');
        let campoHorarioDeTrabalho = $('#HorarioDeTrabalhoId');

        campoFuncao.empty().trigger("change");
        campoDepartamento.empty().trigger("change");
        campoHorarioDeTrabalho.empty().trigger("change");

        if ($(this).val()) {
            $.ajax({
                url: '/FolhaDePonto/FiltrosPontoDoDia',
                type: 'GET',
                data: { unidadeId: $(this).val() }
            }).done(function (data) {
                if (data.sucesso) {
                    if (data.funcoes.length > 0) {
                        adicioneItemNoCampoSelecionavel(campoFuncao, '', 'Todas');

                        $.each(data.funcoes, function (i, item) {
                            adicioneItemNoCampoSelecionavel(campoFuncao, item.codigo, item.descricao);
                        });

                        campoFuncao.val('').trigger('change');
                    }

                    if (data.departamentos.length > 0) {
                        adicioneItemNoCampoSelecionavel(campoDepartamento, '', 'Todos');

                        $.each(data.departamentos, function (i, item) {
                            adicioneItemNoCampoSelecionavel(campoDepartamento, item.codigo, item.descricao);
                        });

                        campoDepartamento.val('').trigger('change');
                    }

                    if (data.horarios.length > 0) {
                        adicioneItemNoCampoSelecionavel(campoHorarioDeTrabalho, '', '');

                        $.each(data.horarios, function (i, item) {
                            adicioneItemNoCampoSelecionavel(campoHorarioDeTrabalho, item.codigo, item.descricao);
                        });

                        campoHorarioDeTrabalho.val('').trigger('change');
                    }
                    else {
                        MensagemRodape('warning', 'Nenhum servidor lotado nessa unidade.');
                    }
                }
            });
        }
    });
}

function carregarPontoDoDia() {
    let unidadeId = $('#UnidadeOrganizacionalId').val() || '';
    let horarioId = $('#HorarioDeTrabalhoId').val() || '';
    let departamentoId = $('#DepartamentoId').val() || '';
    let funcaoId = $('#FuncaoId').val() || '';
    let data = $('#Data').val() || '';


    ajusteValidacaoDeCampo($('#UnidadeOrganizacionalId'), unidadeId != '');
    ajusteValidacaoDeCampo($('#HorarioDeTrabalhoId'), horarioId != '');
    ajusteValidacaoDeCampo($('#Data'), data != '');

    $('#div-ponto-dia').html('');

    if (unidadeId && horarioId && data) {
        RequisicaoAjaxComCarregamento(
            '/FolhaDePonto/CarregarPontoDoDia',
            'GET',
            {
                unidadeId,
                horarioId,
                departamentoId,
                funcaoId,
                data
            },
            function (data) {
                if (data.sucesso) {
                    $('#div-ponto-dia').html(data.html);
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    }
}

function modalPontoDia(vinculoDeTrabalhoId, data, acao) {
    RequisicaoAjaxComCarregamento(
        '/FolhaDePonto/ModalEdicaoPontoDia',
        'GET',
        {
            vinculoDeTrabalhoId,
            acao,
            data
        },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_Modal'));
                $('#_Modal').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function salvarPontoDia() {
    let form = $('#formulario-pontodia');
    let data = {
        vinculoDeTrabalhoId: form.find('#VinculoDeTrabalhoId').val(),
        data: $('#Data').val(),
        valorHora: $('[data-bs-target="#navs-pills-hora"]').hasClass('active') ? form.find('#ValorHora').val() : null,
        justificativaId: $('[data-bs-target="#navs-pills-justificativa"]').hasClass('active') ? form.find('#JustificativaId').val() : null,
        acao: form.find('#Acao').val()
    };

    RequisicaoAjaxComCarregamento(
        '/FolhaDePonto/AtualizePontoDia',
        'POST',
        data,
        function (data) {
            if (data.sucesso) {
                $(`#linha-${form.find('#VinculoDeTrabalhoId').val()}`).replaceWith(data.html);
                $('#_Modal').modal('hide');
                MensagemRodape('success', data.mensagem);
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function converterData(data) {
    if (!/^\d{2}\/\d{2}\/\d{4}$/.test(data)) {
        throw new Error("Formato de data inválido. Use DD/MM/YYYY.");
    }
    const [dia, mes, ano] = data.split("/");
    return `${ano}-${mes}-${dia}`;
}