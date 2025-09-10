$(document).ready(() => {
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
        if ($(this).val() == 'true') {
            $('#div-data-bh').fadeIn('fast');
        } else {
            $('#div-data-bh').fadeOut('fast');
        }
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
            $('#CargaHorariaMensalFixa').prop('disabled', true).prop('readonly', true).trigger('change');
            $('#tabela-horario .hora').prop('disabled', false).prop('readonly', false).trigger('change');
        }
    });
    $('#TipoCargaHoraria').trigger('change');

    assineEventoBotaoSalvar();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
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

        RequisicaoAjaxComCarregamento(
            '/HorarioDeTrabalho/Salvar',
            'POST',
            formulario,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/HorarioDeTrabalho/Alterar/' + data.id);
                }
                else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function obtenhaHorarioDia(linha) {
    return {
        Entrada1: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.entrada1').val(),
        Saida1: $('#TipoCargaHoraria').val() != '0' ? null : linha.find('.entrada2').val(),
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