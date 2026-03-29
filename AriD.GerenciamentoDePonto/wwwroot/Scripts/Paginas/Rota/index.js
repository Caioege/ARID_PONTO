function abrirModal(id) {
    RequisicaoAjaxComCarregamento(
        '/Rota/Modal/',
        'GET',
        { rotaId: id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineEventosModal();
                assineMascarasDoComponente($('#_Modal'));
                $('#_Modal').modal('show');
            }
        }
    );
}

function assineEventosModal() {
    $('#btn-adicionar-parada').on('click', function () {
        let endereco = $('#novaParadaEndereco').val();
        let link = $('#novaParadaLink').val();
        let obs = $('#novaParadaObs').val();

        if (!endereco) {
            MensagemRodape('warning', 'Informe o endereço da parada!');
            return;
        }

        let idTemporario = 0; // 0 significa novo para o backend

        let novaLinha = `
            <tr data-id="${idTemporario}" data-entregue="false" data-endereco="${endereco}" data-link="${link}" data-obs="${obs}">
                <td>${endereco}</td>
                <td>${link}</td>
                <td>${obs}</td>
                <td><span class="badge bg-secondary">Pendente</span></td>
                <td>
                    <button type="button" class="btn btn-sm btn-icon btn-danger btn-remover-parada" onclick="removerLinhaParada(this)"><i class='bx bx-trash'></i></button>
                </td>
            </tr>
        `;

        $('#tabela-paradas tbody').append(novaLinha);
        $('#novaParadaEndereco').val('');
        $('#novaParadaLink').val('');
        $('#novaParadaObs').val('');
    });

    $('#btn-salvar-modal').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-rota');
        
        let motoristaId = $('#MotoristaId').val();
        let descricao = $('#Descricao').val();
         
        if (!motoristaId || !descricao) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos (Motorista, Descrição)!');
            return;
        }

        let payload = dadosFormulario.dados;

        $('#tabela-paradas tbody tr').each(function(index) {
            let row = $(this);
            payload[`paradas[${index}].Id`] = row.attr('data-id');
            payload[`paradas[${index}].Endereco`] = row.attr('data-endereco');
            payload[`paradas[${index}].Link`] = row.attr('data-link');
            payload[`paradas[${index}].Observacao`] = row.attr('data-obs');
            payload[`paradas[${index}].Entregue`] = row.attr('data-entregue');
        });

        RequisicaoAjaxComCarregamento(
            '/Rota/Salvar/',
            'POST',
            payload,
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
    });
}

function removerLinhaParada(button) {
    $(button).closest('tr').remove();
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

function carregarHistoricoExecucoes() {
    let id = $('#_Modal').find('#Id').val();
    $.ajax({
        url: '/Rota/ObtenhaHistoricoExecucoes',
        type: 'GET',
        data: { rotaId: id },
        success: function(data) {
             if (data.sucesso && data.historico) {
                let tbody = $('#tabela-historico-execucoes tbody');
                tbody.empty();
                if (data.historico.length === 0) {
                    tbody.append('<tr><td colspan="5" class="text-center">Nenhuma execução registrada.</td></tr>');
                    return;
                }
                data.historico.forEach(function(h) {
                    let botaoMapa = h.dataHoraFim ? `<button type="button" class="btn btn-sm btn-icon btn-primary" title="Ver Rota" onclick="visualizarRotaNoMapa(${h.id})"><i class='bx bx-map-alt'></i></button>` : '';
                    tbody.append(`<tr><td>${h.dataHoraInicio}</td><td>${h.usuarioInicio}</td><td>${h.dataHoraFim || '-'}</td><td>${h.usuarioFim || '-'}</td><td>${botaoMapa}</td></tr>`);
                });
            }
        }
    });
}

let historicoMap = null;
let historicoLayers = [];

function visualizarRotaNoMapa(execucaoId) {
    $('#tabela-execucoes-container').fadeOut('fast', function () {
        $('#map-historico-container').fadeIn('fast', function () {
            if (!historicoMap) {
                historicoMap = L.map('map-historico').setView([-15.7942, -47.8821], 12);
                L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '&copy; OpenStreetMap'
                }).addTo(historicoMap);
            }
            
            // Clean old layers
            historicoLayers.forEach(l => historicoMap.removeLayer(l));
            historicoLayers = [];

            // Load Route path
            RequisicaoAjaxComCarregamento(
                '/Rota/ObterDadosExecucaoUnica',
                'GET',
                { execucaoId: execucaoId },
                function (res) {
                    if (res.sucesso && res.dados.length > 0) {
                        let rota = res.dados[0];
                        let hexBase = "#34495e";
                        let svgUrl = "data:image/svg+xml;utf8," + encodeURIComponent(`
                            <svg viewBox="0 0 24 24" width="28" height="28" xmlns="http://www.w3.org/2000/svg">
                                <circle cx="12" cy="12" r="10" fill="${hexBase}" stroke="#fff" stroke-width="2"/>
                                <text x="12" y="15" fill="#fff" font-size="10" font-family="Arial" font-weight="bold" text-anchor="middle">F</text>
                            </svg>
                        `);

                        let pathLine = L.polyline(rota.historicoLocalizacoes, { color: hexBase, weight: 5, opacity: 0.8 }).addTo(historicoMap);
                        historicoLayers.push(pathLine);
                        
                        if (rota.ultimaLocalizacao) {
                            let endIcon = L.icon({ iconUrl: svgUrl, iconSize: [28,28], iconAnchor: [14,14] });
                            let markerFinal = L.marker(rota.ultimaLocalizacao, { icon: endIcon }).addTo(historicoMap);
                            historicoLayers.push(markerFinal);
                        }
                        
                        // Paradas
                        if (rota.paradas) {
                            rota.paradas.forEach((p, idx) => {
                                let paradaCss = `border: 2px solid #ccc; background: ${p.entregue ? '#2ecc71' : '#e74c3c'}; color: #fff; border-radius: 50%; width: 22px; height: 22px; text-align: center; line-height: 18px; font-weight: bold; font-size: 11px;`;
                                let paradaIcon = L.divIcon({ className: '', html: `<div style="${paradaCss}">P${idx+1}</div>`, iconSize: [26, 26], iconAnchor: [13, 13] });
                                let pMarker = L.marker([p.latitude, p.longitude], { icon: paradaIcon }).addTo(historicoMap);
                                pMarker.bindPopup(`<h6>${p.nome}</h6>${p.entregue ? 'Entregue' : 'Pend.'}`);
                                historicoLayers.push(pMarker);
                            });
                        }
                        
                        historicoMap.fitBounds(pathLine.getBounds(), { padding: [20, 20] });
                    }
                }
            );
        });
    });
}

function voltarParaHistorico() {
    $('#map-historico-container').fadeOut('fast', function () {
        $('#tabela-execucoes-container').fadeIn('fast');
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

