var nextTimeoutId;
$(document).ready(function () {
    assineMascarasDoComponente($('#dashboard-body'))
    carregarDashboard();
});

function carregarDashboard() {
    if (nextTimeoutId) {
        clearTimeout(nextTimeoutId);
    }

    $.ajax({
        url: '/Dashboard/CarregarDados',
        type: 'GET',
        data: { unidadeId: $('#UnidadeId').val() }
    }).done(function (data) {
        if ($('#TelaDashboard').val() == 'True') {
            if (data.sucesso) {
                $('#tabela-registros').html('');
                atualizarDashboard(data.dados);
            }

            nextTimeoutId = setTimeout(carregarDashboard, 10000);
        }
    });
}

function atualizarDashboard(dados) {
    $('#valor-contratos-ativos').html(dados.totalDeContratosAtivos);
    $('#valor-registros').html(dados.totalDeRegistrosHoje);
    $('#valor-equipamentos').html(dados.totalDeEquipamentosAtivos);

    assineGraficos(dados);

    let htmlTabela = '';
    $.each(dados.ultimosRegistrosRecebidos, function (i, registro) {
        htmlTabela += '<tr>';

        htmlTabela += `<td>${registro.dataHoraString || ''}</td>`;
        htmlTabela += `<td>${registro.escolaNome || ''}</td>`;
        htmlTabela += `<td>${registro.pessoaNome || ''}</td>`;
        htmlTabela += `<td>${registro.idEquipamento || ''}</td>`;
        htmlTabela += `<td>${registro.equipamento || ''}</td>`;

        htmlTabela += '</tr>';
    });
    $('#tabela-registros').html(htmlTabela);
}

function assineGraficos(dados) {
    if (Chart.getChart('graficoHoras')) {
        Chart.getChart('graficoHoras').destroy();
    }
    new Chart(document.getElementById("graficoHoras"), {
        type: 'bar',
        options: {
            responsive: true,
            maintainAspectRatio: false
        },
        data: {
            labels: dados.registrosPorHorario.item1,
            datasets: [{
                label: '',
                data: dados.registrosPorHorario.item2,
                backgroundColor: '#03C3EC'
            }]
        }
    });

    if (dados.registrosPorEquipamento && $('#UnidadeId').val()) {
        if (Chart.getChart('graficoEquipamento')) {
            Chart.getChart('graficoEquipamento').destroy();
        }
        new Chart(document.getElementById("graficoEquipamento"), {
            type: 'bar',
            options: {
                responsive: true,
                maintainAspectRatio: false
            },
            data: {
                labels: dados.registrosPorEquipamento.item1,
                datasets: [{
                    label: '',
                    data: dados.registrosPorEquipamento.item2,
                    backgroundColor: '#03C3EC'
                }]
            }
        });

        $('#div-equipamentos').fadeIn('fast');
    } else {
        $('#div-equipamentos').fadeOut('fast');
    }
}