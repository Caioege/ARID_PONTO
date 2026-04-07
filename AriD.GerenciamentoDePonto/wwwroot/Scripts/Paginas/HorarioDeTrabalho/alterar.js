$(document).ready(() => {
    if ($('#VigenciaSelect').length) {
        $('#VigenciaSelect').on('change', function () {
            const horarioId = $('#Id').val();
            const vigId = $(this).val();
            CarregarPagina('/HorarioDeTrabalho/Alterar/' + horarioId + '?vigenciaId=' + vigId);
        });
    }

    if ($('#btn-nova-vigencia').length) {
        $('#btn-nova-vigencia').on('click', function () {
            const horarioId = $('#Id').val();
            const vigenciaBaseId = $('#VigenciaSelect').val();

            Swal.fire({
                title: 'Nova vigência',
                input: 'text',
                inputLabel: 'Data de início (dd/mm/aaaa)',
                inputPlaceholder: 'ex: 01/03/2026',
                showCancelButton: true,
                confirmButtonText: 'Criar',
                cancelButtonText: 'Cancelar'
            }).then((r) => {
                if (!r.isConfirmed) return;

                RequisicaoAjaxComCarregamento(
                    '/HorarioDeTrabalho/ClonarVigencia',
                    'POST',
                    { horarioId, vigenciaBaseId, vigenciaInicio: r.value },
                    function (data) {
                        if (data.sucesso) {
                            MensagemRodape('success', data.mensagem);
                            CarregarPagina('/HorarioDeTrabalho/Alterar/' + horarioId + '?vigenciaId=' + data.vigenciaId);
                        } else {
                            MensagemRodape('warning', data.mensagem);
                        }
                    }
                );
            });
        });
    }

    $('#UtilizaCincoPeriodos').on('change', function () {
        if ($(this).val() == 'true') {
            $('.ocultar-cinco-periodo').show();
        } else {
            $('.ocultar-cinco-periodo').hide();
            $('.ocultar-cinco-periodo').find('.hora').val('');
            $('.linha').trigger('change');
        }
    });

    $('#UtilizaBancoDeHoras').on('change', function () {
        toggleCamposBancoHoras();
    });

    $('.linha').find('.hora').on('change', function () {
        if ($(this).val().length != 5) {
            $(this).val('');
            return;
        }

        let linha = $(this).parents('tr');

        $.ajax({
            url: '/HorarioDeTrabalho/CalculaCargaHorariaDoDia',
            type: 'GET',
            data: obtenhaHorarioDia(linha)
        }).done(function (data) {
            if (data.sucesso) {
                linha.find('.carga-horaria').html(data.cargaHoraria);
            }
        });
    });

    $('#TipoCargaHoraria').on('change', function () {
        if ($(this).val() == '1') {
            $('#UtilizaCincoPeriodos').val('false').trigger('change');
            $('.colunas-horario-nao-fixo').hide();
            $('#div-utiliza-5').hide();
            $('.colunas-horario-fixo').show();
        } else {
            $('.colunas-horario-fixo').hide();
            $('.colunas-horario-nao-fixo').show();
            $('#div-utiliza-5').show();
        }

        if ($(this).val() == '2') {
            $('#label-chmensal-fixa').addClass('obrigatorio');
            $('#CargaHorariaMensalFixa').prop('disabled', false).prop('readonly', false).trigger('change');
            $('#tabela-horario .hora').prop('disabled', true).prop('readonly', true).trigger('change');
        } else {
            $('#label-chmensal-fixa').removeClass('obrigatorio');
            $('#CargaHorariaMensalFixa').val('');
            $('#CargaHorariaMensalFixa').prop('disabled', true).prop('readonly', true).trigger('change');
            $('#tabela-horario .hora').prop('disabled', false).prop('readonly', false).trigger('change');
        }
    });
    $('#TipoCargaHoraria').trigger('change');

    assineEventoBotaoSalvar();

    $(document).on('click', '.btn-add-faixa', function () {
        const tabela = $(this).closest('.accordion-body').find('.tabela-faixas tbody');
        const ordem = tabela.find('tr.linha-faixa').length + 1;

        tabela.append(`
            <tr class="linha-faixa">
                <td class="text-center">
                    <input type="text" class="form-control ordem" value="${ordem}" readonly />
                </td>
                <td class="text-center">
                    <input type="number"
                           class="form-control minutosAte"
                           step="1" min="0" max="999"
                           placeholder="vazio = restante" />
                </td>
                <td class="text-center">
                    <input type="number"
                           class="form-control percentual"
                           step="1" min="0" max="300"
                           placeholder="50, 70, 100..." />
                </td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-outline-danger btn-remove-faixa">X</button>
                </td>
            </tr>
        `);
    });

    $(document).on('click', '.btn-remove-faixa', function () {
        const tbody = $(this).closest('tbody');
        $(this).closest('tr').remove();

        // renumera ordens
        tbody.find('tr.linha-faixa').each(function (i) {
            $(this).find('.ordem').val(i + 1);
        });
    });

    initSelect2Vigencia();
    toggleCamposBancoHoras();
    initSelect2PrioridadeBH();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        atualizarValorEnumColunas();

        let dadosFormulario = ObtenhaFormularioSerializado('formulario-horario-trabalho');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        let formulario = dadosFormulario.dados;
        formulario.Dias = [];

        $.each($('.linha'), function (i, linha) {
            let dia = obtenhaHorarioDia($(linha));
            dia.DiaDaSemana = $(linha).data('diadasemana');
            dia.Id = $(linha).data('id');
            formulario.Dias.push(dia);
        });

        formulario.RegrasHoraExtra = obterRegrasHoraExtraDaTela();

        formulario.BancoDeHorasPrioridadePercentuais = formulario.BancoDeHorasPrioridadePercentuais || "100,70,50";

        RequisicaoAjaxComCarregamento(
            '/HorarioDeTrabalho/Salvar',
            'POST',
            formulario,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/HorarioDeTrabalho/Alterar/' + data.id + '?vigenciaId=' + data.vigenciaId);
                    return;
                }
                MensagemRodape('warning', data.mensagem);
            });
    });
}

function obtenhaHorarioDia(linha) {
    return {
        Entrada1: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.entrada1').val(),
        Saida1: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.saida1').val(),
        Entrada2: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.entrada2').val(),
        Saida2: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.saida2').val(),
        Entrada3: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.entrada3').val(),
        Saida3: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.saida3').val(),
        Entrada4: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.entrada4').val(),
        Saida4: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.saida4').val(),
        Entrada5: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.entrada5').val(),
        Saida5: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.saida5').val(),
        CargaHorariaFixa: $('#TipoCargaHoraria').val() == '1' ? linha.find('.chfixa').val() : null
    };
}

function removerHorario(id) {
    Swal.fire({
        text: "Tem certeza que deseja remover esse horário de trabalho?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "SIM",
        cancelButtonText: 'NÃO'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/HorarioDeTrabalho/Remover',
                'POST',
                { id },
                function (data) {
                    if (data.sucesso) {
                        MensagemRodape('success', data.mensagem);
                        CarregarPagina('/HorarioDeTrabalho/Index');
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
        }
    });
}

function atualizarValorEnumColunas() {
    var somaTotal = 0;

    $('.check-coluna:checked').each(function () {
        somaTotal += parseInt($(this).val());
    });

    $('#ColunasVisiveis').val(somaTotal);
}

function obterRegrasHoraExtraDaTela() {
    let regras = [];

    $('.regra-he').each(function () {
        const tipoDia = parseInt($(this).data('tipodia'), 10);
        const gerarBase = $(this).find('.he-gerar-base').val() === 'true';
        const percBase = lerDecimalPtBr($(this).find('.he-percent-base').val());
        const aprovarAuto = $(this).find('select[name*="AprovarAutomaticamente"]').val() === 'true';

        let faixas = [];
        let ordem = 1;

        $(this).find('.tabela-faixas tbody tr.linha-faixa').each(function () {
            const minutosAteStr = $(this).find('.minutosAte').val();
            const percentualStr = $(this).find('.percentual').val();
            
            const percentual = lerDecimalPtBr(percentualStr);
            if (!percentual || percentual <= 0) return;

            const minutosAte = minutosAteStr ? parseInt(minutosAteStr, 10) : null;

            faixas.push({
                Ordem: ordem++,
                MinutosAte: minutosAte,
                Percentual: percentual,
            });
        });

        regras.push({
            TipoDia: tipoDia,
            GerarHoraExtraSobreBaseDaJornada: gerarBase,
            PercentualBase: gerarBase ? (percBase || 0) : 0,
            Faixas: faixas,
            AprovarAutomaticamente: aprovarAuto,

        });
    });

    return regras;
}

function lerDecimalPtBr(valor) {
    if (!valor) return 0;
    valor = (valor + '').trim().replace(',', '.');
    valor = valor.replace(/[^0-9.]/g, '');
    return parseFloat(valor || '0');
}

function bhEstaAtivo() {
    const el = $('#UtilizaBancoDeHoras');
    if (!el.length) return false;

    const v = (el.val() || '').toString().toLowerCase();
    return v === 'true' || v === '1' || v === 'sim';
}

function toggleCamposBancoHoras() {
    const ativo = bhEstaAtivo();

    $('#div-data-bh').toggle(ativo);
    $('#div-bh-extra').toggle(ativo);
}

function parsePercentuais(texto) {
    if (!texto) return [];
    return texto
        .split(',')
        .map(x => x.trim())
        .filter(x => /^\d+$/.test(x))
        .filter(x => x.length > 0);
}

function initSelect2PrioridadeBH() {
    const $sel = $('#sel-bh-prioridade');
    const $hidden = $('#BancoDeHorasPrioridadePercentuais');

    if (!$sel.length || !$hidden.length) return;

    const iniciais = parsePercentuais($hidden.val());

    $sel.empty();
    iniciais.forEach(p => {
        const opt = new Option(p, p, true, true);
        $sel.append(opt);
    });

    $sel.select2({
        tags: true,
        width: '100%',
        tokenSeparators: [',', ' '],
        placeholder: 'Ex: 100,70,50',
        createTag: function (params) {
            const term = (params.term || '').trim();

            if (!/^\d+$/.test(term)) return null;

            return {
                id: term,
                text: term,
                newTag: true
            };
        }
    });

    function syncHidden() {
        const vals = ($sel.val() || [])
            .map(x => (x || '').toString().trim())
            .filter(x => /^\d+$/.test(x));

        const seen = new Set();
        const unique = [];
        vals.forEach(v => {
            if (!seen.has(v)) { seen.add(v); unique.push(v); }
        });

        $hidden.val(unique.join(','));
    }

    $sel.on('change', syncHidden);
    syncHidden();
}

function initSelect2Vigencia() {
    const $vig = $('#VigenciaSelect');
    if (!$vig.length) return;

    $vig.select2({
        width: '100%',
        minimumResultsForSearch: 10
    });
}