$(document).ready(function() {
    toggleTipos(false);
    $('.select2').select2({
        width: '100%'
    });
    assineEventoBotaoSalvar();
});

function toggleTolerancia(animar = true) {
    const selector = '#divTolerancia';
    if ($('#TurnoIntercaladoPagaDobrado').is(':checked') && $('#TipoBonus').val() == '1') {
        animar ? $(selector).fadeIn() : $(selector).show();
    } else {
        animar ? $(selector).fadeOut() : $(selector).hide();
    }
}

function toggleTipos(animar = true) {
    if ($('#TipoBonus').val() == '1') { // Diario
        animar ? $('.divDiario').fadeIn() : $('.divDiario').show();
        $('.divMensal').hide();
        toggleTolerancia(animar);
    } else {
        $('.divDiario').hide();
        $('#divTolerancia').hide();
        animar ? $('.divMensal').fadeIn() : $('.divMensal').show();
    }
}

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-bonus');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Bonus/Salvar',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Bonus/Alterar/' + data.id);
                } else {
                    MensagemRodape('error', data.mensagem);
                }
            });
    });
}
