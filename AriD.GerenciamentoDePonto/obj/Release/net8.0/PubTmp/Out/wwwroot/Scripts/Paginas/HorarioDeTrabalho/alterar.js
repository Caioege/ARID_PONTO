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

    assineEventoBotaoSalvar();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let formulario = ObtenhaFormularioSerializado('formulario-horario-trabalho');
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
                MensagemRodape('success', data.mensagem);
                $('#Id').val(data.id);
            });
    });
}

function obtenhaHorarioDia(linha) {
    return {
        Entrada1: linha.find('.entrada1').val(),
        Saida1: linha.find('.saida1').val(),
        Entrada2: linha.find('.entrada2').val(),
        Saida2: linha.find('.saida2').val(),
        Entrada3: linha.find('.entrada3').val(),
        Saida3: linha.find('.saida3').val(),
        Entrada4: linha.find('.entrada4').val(),
        Saida4: linha.find('.saida4').val(),
        Entrada5: linha.find('.entrada5').val(),
        Saida5: linha.find('.saida5').val(),
    };
}