$(document).ready(function () {
    assineChangeEquipamento();
    assineProcessar();
});

function assineChangeEquipamento() {
    $('#EquipamentoId').on('change', function () {
        let equipamentoId = $(this).val();

        if (equipamentoId) {
            $.ajax({
                url: '/EquipamentoDePonto/ObtenhaNSR',
                type: 'GET',
                data: { equipamentoId }
            }).done(function (data) {
                if (data.sucesso) {
                    $('#UltimoNSRLido').val(data.nsr);
                } else {
                    $('#UltimoNSRLido').val('');
                }
            });
        }
        else {
            $('#UltimoNSRLido').val('');
        }
    });
}

function assineProcessar() {
    $('#btn-processar').on('click', function () {
        let equipamentoId = $('#EquipamentoId').val();
        if (!equipamentoId) {
            MensagemRodape('warning', 'Informe o equipamento.');
            return;
        }

        let arquivo = document.querySelector('#ArquivoAFD');
        if (arquivo.files.length == 0) {
            MensagemRodape('warning', 'Anexe o arquivo.');
            return;
        }

        let formData = new FormData();
        formData.append('arquivo', arquivo.files[0]);
        formData.append('equipamentoId', equipamentoId);
        formData.append('ultimoNSRInformado', $('#UltimoNSRLido').val() || 0);

        AbrirCaixaDeCarregamento('Importando registros...');
        try {
            $.ajax({
                url: '/EquipamentoDePonto/ImportarAFD',
                type: 'POST',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
            }).done(function (data) {
                FecharCaixaDeCarregamento();
                if (!data.sucesso) {
                    MensagemRodape('warning', data.mensagem);
                } else {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/EquipamentoDePonto/ImportarAFD');
                }
            });
        } catch {
            FecharCaixaDeCarregamento();
        }
    });
}