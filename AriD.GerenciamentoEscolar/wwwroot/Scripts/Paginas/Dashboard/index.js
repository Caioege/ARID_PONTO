$(document).ready(function () {
    assineMascarasDoComponente($('#dashboard-body'))
    carregarDashboard(); // primeira execução imediata
});

function carregarDashboard() {
    // Exibir loaders
    $("[id^='loader-']").removeClass("d-none");

    // Simular chamada com timeout
    setTimeout(function () {
        // Aqui você faria requisição AJAX real para atualizar gráficos e dados
        // $.get('/Dashboard/Dados', function(data) { atualizarDashboard(data); });

        // Ocultar loaders
        $("[id^='loader-']").addClass("d-none");
        assineGraficos();
        // Atualiza a cada 10s
        setInterval(carregarDashboard, 10000);
    }, 1000);
}

function assineGraficos() {
    // Gráfico Frequência
    if (Chart.getChart('graficoFrequencia')) {
        Chart.getChart('graficoFrequencia').destroy();
    }
    new Chart(document.getElementById("graficoFrequencia"), {
        type: 'line',
        data: {
            labels: ["Seg", "Ter", "Qua", "Qui", "Sex"],
            datasets: [{
                label: "Frequência (%)",
                data: [87, 88, 85, 90, 89],
                borderColor: '#696CFF',
                fill: false
            }]
        }
    });

    // Gráfico Equipamentos
    if (Chart.getChart('graficoEquipamento')) {
        Chart.getChart('graficoEquipamento').destroy();
    }
    new Chart(document.getElementById("graficoEquipamento"), {
        type: 'bar',
        data: {
            labels: ["07:00", "12:00", "17:00"],
            datasets: [{
                label: "Registros",
                data: [324, 278, 192],
                backgroundColor: '#03C3EC'
            }]
        }
    });

    // Gráfico Frequência por Escola
    if (Chart.getChart('graficoPorEscola')) {
        Chart.getChart('graficoPorEscola').destroy();
    }
    new Chart(document.getElementById("graficoPorEscola"), {
        type: 'bar',
        data: {
            labels: ["Escola A", "Escola B", "Escola C"],
            datasets: [{
                label: "Frequência Média (%)",
                data: [91, 77, 88],
                backgroundColor: ['#71DD37', '#FF3E1D', '#FFAB00']
            }]
        }
    });
}