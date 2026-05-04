var map = window.monitoramentoRotasMap || null;
var rotasMonitoramentoResumoData = window.monitoramentoRotasResumoData || window.monitoramentoRotasData || [];
var rotasMonitoramentoMapaData = window.monitoramentoRotasMapaData || window.monitoramentoRotasData || [];
var manutencoesVeiculosMonitoramento = window.monitoramentoManutencoesVeiculos || [];
var routeLayers = window.monitoramentoRotasRouteLayers || {};
var markerUnidadesLayers = window.monitoramentoRotasUnidadesLayers || [];
var rotaSelecionadaExecucaoId = window.monitoramentoRotasSelecionada || null;
var rotaPrevistaLayer = null;
var eventoSelecionadoMarker = null;
var comparativoPrevistoAtivo = false;
var chatRotaResumoAtual = null;
var chatNaoLidasPorExecucao = window.monitoramentoRotasChatNaoLidas || {};
var chatUltimaQuantidadeAlertadaPorExecucao = {};
var chatNaoLidasBaselineInicializada = false;
var chatAlertasMensagemFila = [];
var chatAlertaMensagemAtivo = false;
var chatAbertoInterval = null;
var chatCarregando = false;
var monitoramentoFiltroRapido = window.monitoramentoRotasFiltroRapido || 'todos';
var timelinePontosMapa = [];

$(document).ready(function () {
    $("#modal-chat-rota").on("hidden.bs.modal", function () {
        pararPollingChatAberto();
    });
    setTimeout(function () {
        initMap();
    }, 200);
});

function initMap() {
    var lat = typeof mapLatCentro !== 'undefined' ? mapLatCentro : -15.7942;
    var lon = typeof mapLonCentro !== 'undefined' ? mapLonCentro : -47.8821;

    if (window.monitoramentoRotasInterval) {
        clearInterval(window.monitoramentoRotasInterval);
        window.monitoramentoRotasInterval = null;
    }

    if (window.monitoramentoRotasChatInterval) {
        clearInterval(window.monitoramentoRotasChatInterval);
        window.monitoramentoRotasChatInterval = null;
    }

    if (map) {
        map.remove();
        map = null;
    }

    routeLayers = {};
    window.monitoramentoRotasRouteLayers = routeLayers;
    window.monitoramentoRotasChatInicializado = false;
    chatNaoLidasBaselineInicializada = false;
    chatUltimaQuantidadeAlertadaPorExecucao = {};
    chatAlertasMensagemFila = [];
    chatAlertaMensagemAtivo = false;

    map = L.map('map').setView([lat, lon], 12);
    window.monitoramentoRotasMap = map;
    map.on('click', function () {
        removerPinEventoSelecionado();
    });

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    renderizarUnidadesNoMapa();
    obterDadosMonitoramento();
    window.monitoramentoRotasInterval = setInterval(obterDadosMonitoramento, 15000);
    window.monitoramentoRotasChatInterval = setInterval(atualizarNaoLidasChatRotas, 45000);
}

function renderizarUnidadesNoMapa() {
    markerUnidadesLayers.forEach(function (layer) {
        if (map && map.hasLayer(layer)) map.removeLayer(layer);
    });
    markerUnidadesLayers = [];
    window.monitoramentoRotasUnidadesLayers = markerUnidadesLayers;

    if (typeof mapUnidades === 'undefined' || !mapUnidades || mapUnidades.length === 0) return;

    mapUnidades.forEach(function (un) {
        var iconHtml = '<i class="bx bxs-building-house"></i>';
        var corBg = '#2563eb';

        switch (un.TipoId) {
            case 0: iconHtml = '<i class="bx bxs-bank"></i>'; corBg = '#1f2937'; break;
            case 1: iconHtml = '<i class="bx bxs-buildings"></i>'; corBg = '#64748b'; break;
            case 2: iconHtml = '<i class="bx bx-plus-medical"></i>'; corBg = '#dc2626'; break;
            case 3: iconHtml = '<i class="bx bxs-graduation"></i>'; corBg = '#d97706'; break;
            case 4: iconHtml = '<i class="bx bx-building"></i>'; corBg = '#475569'; break;
        }

        var markerCss = 'background:' + corBg + ';color:white;border:2px solid white;border-radius:50%;width:30px;height:30px;display:flex;align-items:center;justify-content:center;box-shadow:0 4px 8px rgba(0,0,0,.32);font-size:16px;';
        var customIcon = L.divIcon({
            className: 'custom-unidade-icon',
            html: '<div style="' + markerCss + '">' + iconHtml + '</div>',
            iconSize: [30, 30],
            iconAnchor: [15, 15]
        });

        var marker = L.marker([un.Latitude, un.Longitude], { icon: customIcon }).addTo(map);
        marker.bindPopup(
            '<div style="min-width:180px;padding:4px;">' +
            '<h6 style="margin:0 0 6px;color:' + corBg + ';font-weight:800;">' + iconHtml + ' ' + escapeHtml(un.Nome) + '</h6>' +
            '<div style="font-size:.85rem;color:#475569;"><strong>Tipo:</strong> ' + escapeHtml(un.Tipo || 'Unidade') + '</div>' +
            '</div>'
        );
        markerUnidadesLayers.push(marker);
    });
}

function obterDadosMonitoramento() {
    var dataFiltro = $("#M_FiltroData").length ? $("#M_FiltroData").val() : "";

    $.getJSON('/Rota/ObterDadosMonitoramento?dataFiltro=' + encodeURIComponent(dataFiltro) + '&exibirFinalizadas=true', function (response) {
        if (!response.sucesso) {
            console.error("Erro ao buscar dados de monitoramento:", response.mensagem);
            return;
        }

        var rotas = response.dados || [];
        manutencoesVeiculosMonitoramento = response.manutencoesVeiculos || [];
        rotasMonitoramentoResumoData = rotas;
        rotasMonitoramentoMapaData = rotas;
        window.monitoramentoRotasResumoData = rotasMonitoramentoResumoData;
        window.monitoramentoRotasMapaData = rotasMonitoramentoMapaData;
        window.monitoramentoRotasData = rotasMonitoramentoResumoData;
        window.monitoramentoManutencoesVeiculos = manutencoesVeiculosMonitoramento;

        atualizarDashboard();
        renderizarRotasNoMapa();
        restaurarSelecaoAposAtualizacao();
        if (!window.monitoramentoRotasChatInicializado) {
            window.monitoramentoRotasChatInicializado = true;
            atualizarNaoLidasChatRotas();
        }

        $("#ultima-sincronizacao").text("Sincronizado às " + new Date().toLocaleTimeString('pt-BR'));
    });
}

function aplicarFiltrosMap() {
    renderizarRotasNoMapa();
    atualizarTabelaRotas(obterRotasVisiveisNoMapa());
    centralizarMapaTodo();
}

function aplicarFiltroRapidoMonitoramento(filtro) {
    monitoramentoFiltroRapido = filtro || 'todos';
    window.monitoramentoRotasFiltroRapido = monitoramentoFiltroRapido;

    $(".monitor-filter-chip").removeClass("active");
    $('.monitor-filter-chip[data-monitor-filter="' + monitoramentoFiltroRapido + '"]').addClass("active");

    renderizarRotasNoMapa();
    atualizarTabelaRotas(obterRotasVisiveisNoMapa());
    centralizarMapaTodo();
}

function atualizarDashboard() {
    atualizarKpis(rotasMonitoramentoResumoData);
    atualizarContadorPossivelmenteOffline(rotasMonitoramentoResumoData);
    atualizarAlertasGerais(rotasMonitoramentoResumoData);
    atualizarTabelaRotas(obterRotasVisiveisNoMapa());
    atualizarManutencoes(manutencoesVeiculosMonitoramento);
}

function atualizarKpis(rotas) {
    rotas = rotas || [];
    var emRota = rotas.filter(function (r) { return !ehFinalizada(r); }).length;
    var ocorrencias = rotas.filter(function (r) { return r.sujeitoADesvio === true || possuiAlertaSeveridade(r, ['critico', 'alto']); }).length;
    var manutencoes = (manutencoesVeiculosMonitoramento || []).filter(manutencaoDeveAparecer).length;
    var finalizadas = rotas.filter(ehFinalizada).length;
    var offline = rotas.filter(function (r) { return r.possivelmenteOffline === true; }).length;
    var chatNaoLidas = rotas.filter(function (r) {
        return (chatNaoLidasPorExecucao[r.execucaoId || obterLayerId(r)] || 0) > 0;
    }).length;
    var atrasadas = rotas.filter(rotaPossuiAtrasoOperacional).length;

    $("#kpi-em-rota").text(emRota);
    $("#kpi-ocorrencias").text(ocorrencias);
    $("#kpi-manutencoes").text(manutencoes);
    $("#kpi-finalizadas").text(finalizadas);
    $("#kpi-offline").text(offline);
    $("#kpi-chat-nao-lidas").text(chatNaoLidas);
    $("#kpi-atrasadas").text(atrasadas);
}

function atualizarContadorPossivelmenteOffline(rotas) {
    var total = (rotas || []).filter(function (r) { return r.possivelmenteOffline === true; }).length;
    var $contador = $("#contador-possivelmente-offline");
    var $texto = $("#contador-possivelmente-offline-texto");

    if (!$contador.length || !$texto.length) return;

    if (total <= 0) {
        $contador.hide();
        $texto.text("0 rotas possivelmente offline");
        return;
    }

    $texto.text(total === 1 ? "1 rota possivelmente offline" : total + " rotas possivelmente offline");
    $contador.css("display", "flex");
}

function obterRotasVisiveisNoMapa() {
    var exibirFinalizadas = $("#M_FiltroFinalizadas").length ? $("#M_FiltroFinalizadas").is(":checked") : false;

    return (rotasMonitoramentoMapaData || []).filter(function (r) {
        if (!exibirFinalizadas && monitoramentoFiltroRapido !== 'finalizadas' && ehFinalizada(r)) return false;
        return rotaPassaFiltroRapido(r);
    });
}

function rotaPassaFiltroRapido(rota) {
    var filtro = monitoramentoFiltroRapido || 'todos';
    if (filtro === 'todos') return true;
    if (filtro === 'em-rota') return !ehFinalizada(rota);
    if (filtro === 'ocorrencia') return rota.sujeitoADesvio === true || possuiAlertaSeveridade(rota, ['critico', 'alto']);
    if (filtro === 'sem-sinal') return rota.possivelmenteOffline === true;
    if (filtro === 'com-mensagem') return (chatNaoLidasPorExecucao[rota.execucaoId || obterLayerId(rota)] || 0) > 0;
    if (filtro === 'finalizadas') return ehFinalizada(rota);
    if (filtro === 'atrasadas') return rotaPossuiAtrasoOperacional(rota);
    return true;
}

function renderizarRotasNoMapa() {
    if (!map) return;

    var rotaFocada = rotaSelecionadaExecucaoId ? encontrarRota(rotaSelecionadaExecucaoId) : null;
    var dadosFiltrados = rotaFocada ? [rotaFocada] : obterRotasVisiveisNoMapa();

    Object.keys(routeLayers).forEach(function (id) {
        removerCamada(routeLayers[id].polyline);
        removerCamada(routeLayers[id].marker);
        removerCamada(routeLayers[id].inicio);
        removerCamada(routeLayers[id].fim);
        (routeLayers[id].paradas || []).forEach(removerCamada);
        (routeLayers[id].offlineSegments || []).forEach(removerCamada);
    });
    routeLayers = {};
    window.monitoramentoRotasRouteLayers = routeLayers;
    removerRotaPrevista();

    var group = new L.featureGroup();

    dadosFiltrados
        .slice()
        .sort(function (a, b) {
            if (ehFinalizada(a) === ehFinalizada(b)) return 0;
            return ehFinalizada(a) ? -1 : 1;
        })
        .forEach(function (rota) {
            if (!rota.ultimaLocalizacao) return;

            var layerId = obterLayerId(rota);
            var corViva = getPredefinedColor(rota.rotaId);
            var status = obterStatusVisual(rota);
            var cor = status.cor || corViva;
            var isFinalizada = ehFinalizada(rota);
            var isPossivelmenteOffline = rota.possivelmenteOffline === true;
            var lineClass = isPossivelmenteOffline ? 'route-path possivelmente-offline-route' : (rota.possuiRegistroOffline ? 'route-path offline-route' : (isFinalizada ? 'route-path' : 'route-path animated-route'));
            var lineOpacity = isPossivelmenteOffline ? .95 : (isFinalizada ? .58 : .82);

            var pathLine = L.polyline(rota.historicoLocalizacoes || [], {
                color: cor,
                weight: 5,
                opacity: lineOpacity,
                smoothFactor: 1,
                className: lineClass
            }).addTo(map);

            var offlineSegments = [];
            if (rota.historicoLocalizacoesDetalhado && rota.historicoLocalizacoesDetalhado.length > 1) {
                montarSegmentosOfflineMonitoramento(rota.historicoLocalizacoesDetalhado).forEach(function (seg) {
                    if (seg.length < 2) return;
                    var offlineLine = L.polyline(seg, {
                        color: '#f59e0b',
                        weight: 6,
                        opacity: .9,
                        dashArray: '8, 8'
                    }).addTo(map);
                    offlineSegments.push(offlineLine);
                });
            }

            var inicioMarker = montarMarcadorInicio(rota, corViva);
            if (inicioMarker) {
                inicioMarker.addTo(map);
                group.addLayer(inicioMarker);
            }

            var marker = null;
            var fimMarker = null;
            var paradasMarkers = [];

            if (isFinalizada) {
                fimMarker = montarMarcadorFim(rota, corViva);
                if (fimMarker) {
                    fimMarker.addTo(map);
                    fimMarker.on('click', function () {
                        selecionarRota(rota.execucaoId || layerId, true);
                    });
                    group.addLayer(fimMarker);
                }
            } else {
                marker = L.marker(rota.ultimaLocalizacao, { icon: montarIconeVeiculo(rota, cor, status) }).addTo(map);
                marker.bindPopup(montarPopupResumo(rota, status), { offset: [0, -5], closeButton: false });
                marker.on('click', function () {
                    selecionarRota(rota.execucaoId || layerId, true);
                });
                paradasMarkers = montarMarcadoresParadas(rota, cor, corViva, isFinalizada);
                group.addLayer(marker);
                paradasMarkers.forEach(function (pMarker) { group.addLayer(pMarker); });
            }

            routeLayers[layerId] = {
                polyline: pathLine,
                marker: marker,
                inicio: inicioMarker,
                fim: fimMarker,
                paradas: paradasMarkers,
                offlineSegments: offlineSegments
            };

            group.addLayer(pathLine);
        });

    if (dadosFiltrados.length > 0 && Object.keys(routeLayers).length > 0 && !window.monitoramentoRotasJaCentralizou) {
        try {
            map.fitBounds(group.getBounds(), { padding: [40, 40] });
            window.monitoramentoRotasJaCentralizou = true;
        } catch (e) { }
    }

    if (comparativoPrevistoAtivo && rotaFocada) {
        renderizarRotaPrevista(rotaFocada);
    }
}

function montarMarcadorInicio(rota, corViva) {
    if (!rota.historicoLocalizacoes || rota.historicoLocalizacoes.length === 0) return null;

    var inicioCss = 'background:' + corViva + ';color:white;border:2px solid white;border-radius:50% 50% 50% 0;width:26px;height:26px;transform:rotate(-45deg);display:flex;align-items:center;justify-content:center;box-shadow:0 4px 8px rgba(0,0,0,.35);';
    var inicioIcon = L.divIcon({
        className: 'custom-inicio-rota-container',
        html: '<div style="' + inicioCss + '"><i class="bx bx-play" style="transform:rotate(45deg);font-size:15px;"></i></div>',
        iconSize: [30, 30],
        iconAnchor: [5, 25]
    });

    var marker = L.marker(rota.historicoLocalizacoes[0], { icon: inicioIcon, zIndexOffset: 50 });
    marker.bindPopup('<strong>Início da rota</strong><br>' + escapeHtml(rota.descricao || '') + '<br><small>' + escapeHtml(rota.horaInicio || '') + '</small>', { closeButton: false });
    return marker;
}

function montarMarcadorFim(rota, corViva) {
    if (!rota.ultimaLocalizacao) return null;

    var fimCss = 'background:#ef4444;color:white;border:2px solid white;border-radius:50% 50% 50% 0;width:28px;height:28px;transform:rotate(-45deg);display:flex;align-items:center;justify-content:center;box-shadow:0 4px 8px rgba(0,0,0,.35);';
    var fimIcon = L.divIcon({
        className: 'custom-fim-rota-container',
        html: '<div style="' + fimCss + '"><i class="bx bxs-flag-checkered" style="transform:rotate(45deg);font-size:15px;"></i></div>',
        iconSize: [32, 32],
        iconAnchor: [6, 27]
    });

    var marker = L.marker(rota.ultimaLocalizacao, { icon: fimIcon, zIndexOffset: 60 });
    marker.bindPopup('<strong>Fim da rota</strong><br>' + escapeHtml(rota.descricao || '') + '<br><small>' + escapeHtml(rota.horaFim || rota.ultimaAtualizacao || '') + '</small>', { closeButton: false });
    return marker;
}

function montarMarcadoresParadas(rota, cor, corViva, isFinalizada) {
    var markers = [];
    var proximaEncontrada = false;

    (rota.paradas || []).forEach(function (p, idx) {
        var bgStyle;
        if (p.entregue) {
            bgStyle = 'border:2px solid #16a34a;color:#fff;background:#16a34a;';
        } else if (!proximaEncontrada && !isFinalizada) {
            proximaEncontrada = true;
            bgStyle = 'border:3px solid ' + corViva + ';color:' + corViva + ';background:#fff;';
        } else {
            bgStyle = 'border:2px solid ' + cor + ';color:' + cor + ';background:#fff;';
        }

        var markerCss = bgStyle + 'border-radius:50%;width:22px;height:22px;text-align:center;line-height:18px;font-weight:800;font-size:11px;box-shadow:0 3px 6px rgba(0,0,0,.3);';
        var paradaIcon = L.divIcon({
            className: 'custom-parada-container',
            html: '<div style="' + markerCss + '">P' + (idx + 1) + '</div>',
            iconSize: [26, 26],
            iconAnchor: [13, 13]
        });

        var pMarker = L.marker([p.latitude, p.longitude], { icon: paradaIcon, zIndexOffset: -100 }).addTo(map);
        pMarker.bindPopup('<strong>Parada: ' + escapeHtml(p.nome || '') + '</strong><br><small>' + (p.entregue ? 'Concluída ' + escapeHtml(p.concluidoEm || '') : 'Pendente') + '</small>', { closeButton: false });
        markers.push(pMarker);
    });

    return markers;
}

function montarIconeVeiculo(rota, cor, status) {
    var heading = calcularHeading(rota);
    var isFinalizada = ehFinalizada(rota);
    var isPossivelmenteOffline = rota.possivelmenteOffline === true;
    var strokeColor = isPossivelmenteOffline ? "#fff3cd" : (isFinalizada ? "#94a3b8" : "white");
    var shadowColor = isPossivelmenteOffline ? "rgba(220,38,38,.55)" : "rgba(15,23,42,.35)";
    var opacityCar = isFinalizada ? ".72" : "1";
    var innerSvg = montarSvgVeiculo(rota.tipoVeiculo, cor, strokeColor);

    var carSvg =
        '<div style="position:relative;width:46px;height:46px;display:flex;align-items:center;justify-content:center;' + (isPossivelmenteOffline ? 'border:3px solid #f59e0b;border-radius:50%;background:rgba(255,251,235,.94);' : '') + '">' +
        (isPossivelmenteOffline ? '<span style="position:absolute;right:-4px;top:-4px;width:14px;height:14px;border-radius:50%;background:#dc2626;border:2px solid #fff;"></span>' : '') +
        '<div style="transform:rotate(' + heading + 'deg);width:36px;height:36px;display:flex;align-items:center;justify-content:center;filter:drop-shadow(0 4px 6px ' + shadowColor + ');opacity:' + opacityCar + ';">' +
        '<svg viewBox="0 0 24 24" width="36" height="36" xmlns="http://www.w3.org/2000/svg">' + innerSvg + '</svg>' +
        '</div></div>';

    return L.divIcon({
        className: 'custom-car-icon status-' + status.slug,
        html: carSvg,
        iconSize: [46, 46],
        iconAnchor: [23, 23]
    });
}

function montarSvgVeiculo(tipoVeiculo, cor, strokeColor) {
    switch (tipoVeiculo) {
        case 1:
            return '<rect x="11" y="2" width="2" height="4" fill="#333" rx="1"/><path d="M7 8 Q12 6 17 8" fill="none" stroke="#222" stroke-width="1.5" stroke-linecap="round"/><ellipse cx="12" cy="11" rx="3" ry="4" fill="' + cor + '" stroke="' + strokeColor + '" stroke-width="1"/><ellipse cx="12" cy="16" rx="2.5" ry="4" fill="#111"/><rect x="11" y="18" width="2" height="4" fill="#333" rx="1"/>';
        case 2:
            return '<rect x="7" y="2" width="10" height="5" rx="2" fill="' + cor + '" stroke="' + strokeColor + '" stroke-width="1"/><rect x="8" y="4" width="8" height="2" fill="rgba(0,0,0,.8)" rx=".5"/><rect x="11" y="7" width="2" height="2" fill="#333"/><rect x="7" y="9" width="10" height="13" rx="1" fill="' + cor + '" stroke="' + strokeColor + '" stroke-width="1"/><rect x="8" y="10" width="8" height="11" fill="rgba(255,255,255,.2)" rx=".5"/>';
        case 3:
            return '<rect x="6" y="1" width="12" height="22" rx="2" fill="' + cor + '" stroke="' + strokeColor + '" stroke-width="1"/><rect x="7" y="2" width="10" height="2" fill="rgba(0,0,0,.8)" rx=".5"/><rect x="7" y="20" width="10" height="1.5" fill="rgba(0,0,0,.8)" rx=".5"/><rect x="9" y="6" width="6" height="3" fill="#eee" rx="1"/><rect x="9" y="14" width="6" height="3" fill="#eee" rx="1"/>';
        case 4:
            return '<rect x="6" y="2" width="12" height="20" rx="3" fill="white" stroke="' + strokeColor + '" stroke-width="1"/><rect x="7" y="5" width="10" height="3" fill="rgba(0,0,0,.8)" rx=".5"/><rect x="7" y="3" width="10" height="1.5" fill="' + cor + '" rx=".5"/><rect x="10" y="8" width="4" height="2" fill="red" rx="1"/><path d="M12 12V18M9 15H15" stroke="red" stroke-width="2.5" stroke-linecap="square"/>';
        default:
            return '<rect x="5" y="2" width="14" height="20" rx="4" fill="' + cor + '" stroke="' + strokeColor + '" stroke-width="1.5"/><rect x="7" y="5" width="10" height="4" fill="rgba(0,0,0,.7)" rx="1"/><rect x="7" y="15" width="10" height="4" fill="rgba(0,0,0,.6)" rx="1"/>';
    }
}

function montarPopupResumo(rota, status) {
    return '<div class="popup-content" style="min-width:210px;">' +
        '<h6 style="margin:.25rem 0 .45rem;font-weight:800;">' + escapeHtml(rota.descricao || 'Rota') + '</h6>' +
        '<div style="font-size:.82rem;line-height:1.55;color:#475569;">' +
        '<div><strong>Status:</strong> ' + escapeHtml(status.label) + '</div>' +
        '<div><strong>Motorista:</strong> ' + montarMotoristaComBadge(rota) + '</div>' +
        '<div><strong>Veículo:</strong> ' + escapeHtml(rota.placaModelo || '-') + '</div>' +
        '<div><strong>Atualização:</strong> ' + escapeHtml(rota.ultimaAtualizacao || '-') + '</div>' +
        '</div></div>';
}

function selecionarRota(execucaoId, centralizar) {
    var rota = encontrarRota(execucaoId);
    if (!rota) return;

    rotaSelecionadaExecucaoId = rota.execucaoId || obterLayerId(rota);
    window.monitoramentoRotasSelecionada = rotaSelecionadaExecucaoId;
    comparativoPrevistoAtivo = false;
    removerPinEventoSelecionado();

    atualizarBarraLateral(rota);
    renderizarRotasNoMapa();
    atualizarTabelaRotas(obterRotasVisiveisNoMapa());
    atualizarLinhaSelecionada();

    if (centralizar) centralizarRotaSelecionada();
}

function atualizarBarraLateral(rota) {
    var status = obterStatusVisual(rota);
    $("#side-rota-titulo").text(rota.descricao || "Rota");
    $("#side-rota-subtitulo").html(escapeHtml(rota.placaModelo || "Veículo não informado") + " | " + montarMotoristaComBadge(rota));
    $("#side-rota-status").attr("class", "status-chip " + status.slug).text(status.label);
    $("#btn-chat-rota").prop("disabled", false);
    $("#btn-centralizar-rota").prop("disabled", false);
    $("#btn-limpar-foco-rota").prop("disabled", false);
    $("#btn-comparar-rota").prop("disabled", false).html('<i class="bx bx-git-compare"></i> Prevista x executada');
    $("#btn-checklist-rota").prop("disabled", !rota.checklistExecucaoId);

    var pacientes = formatarPessoas(rota.pacientes, "");
    var acompanhantes = formatarPessoas(rota.acompanhantes, "");
    var profissionais = formatarPessoas(rota.profissionais, "");
    var manutencao = rota.proximaManutencao ? montarTextoManutencao(rota.proximaManutencao) : "Sem alerta";

    $("#side-detalhes-rota").html(
        detailItemHtml("Motorista", montarMotoristaComBadge(rota)) +
        detailItem("Veículo", rota.placaModelo || "-") +
        detailItem("Paciente", pacientes || "-") +
        detailItem("Acompanhante", acompanhantes || "-") +
        detailItem("Profissional", profissionais || "-") +
        detailItem("Observação", rota.observacaoRota || "-") +
        detailItem("Início", rota.horaInicio || "-") +
        detailItem("Fim", rota.horaFim || "-") +
        detailItem("Última atualização", rota.ultimaAtualizacao || "-") +
        detailItem("Última comunicação", rota.ultimaComunicacaoApp || "-") +
        detailItem("Velocidade média", rota.velocidadeMediaKmH !== null && rota.velocidadeMediaKmH !== undefined ? Number(rota.velocidadeMediaKmH).toFixed(1).replace('.', ',') + " km/h" : "-") +
        detailItem("Manutenção", manutencao)
    );

    renderizarParadas(rota.paradas || []);
    renderizarLinhaDoTempoRota(rota);
    renderizarBadgeChatSelecionado();
}

function abrirChecklistExecucaoRota(execucaoId, event) {
    if (event && event.stopPropagation) event.stopPropagation();

    execucaoId = execucaoId || rotaSelecionadaExecucaoId;
    if (!execucaoId) return;

    $("#checklist-modal-titulo").text("Checklist da execução");
    $("#checklist-modal-subtitulo").text("Preenchido antes do início da rota");
    $("#checklist-modal-conteudo").html('<div class="empty-state">Carregando checklist...</div>');

    var modalEl = document.getElementById('modal-checklist-execucao');
    if (modalEl && window.bootstrap) {
        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    }

    $.getJSON('/Rota/ObterChecklistExecucao', { execucaoId: execucaoId }, function (response) {
        if (!response || !response.sucesso) {
            $("#checklist-modal-conteudo").html('<div class="empty-state">' + escapeHtml((response && response.mensagem) || 'Não foi possível carregar o checklist desta execução.') + '</div>');
            return;
        }

        renderizarChecklistExecucaoModal(response.dados);
    }).fail(function () {
        $("#checklist-modal-conteudo").html('<div class="empty-state">Não foi possível carregar o checklist desta execução.</div>');
    });
}

function renderizarChecklistExecucaoModal(checklist) {
    checklist = checklist || {};
    var itens = checklist.itens || [];
    var resumo = (checklist.totalMarcados || 0) + " de " + (checklist.totalItens || itens.length || 0) + " itens marcados";

    $("#checklist-modal-titulo").text(checklist.rotaDescricao || "Checklist da execução");
    $("#checklist-modal-subtitulo").text((checklist.veiculoDescricao || "Veículo não informado") + " | " + (checklist.motoristaNome || "Motorista não informado"));

    var itensHtml = itens.length === 0
        ? '<div class="empty-state">Nenhum item de checklist cadastrado para o veículo desta execução.</div>'
        : '<div class="list-group list-group-flush border rounded">' + itens.map(function (item) {
            var classe = item.marcado ? 'text-success' : 'text-danger';
            var icone = item.marcado ? 'bx-check-circle' : 'bx-x-circle';
            var texto = item.marcado ? 'Marcado' : 'Não marcado';

            return '<div class="list-group-item d-flex justify-content-between align-items-center gap-2">' +
                '<span>' + escapeHtml(item.descricao || '-') + '</span>' +
                '<span class="' + classe + ' fw-bold text-nowrap"><i class="bx ' + icone + '"></i> ' + texto + '</span>' +
                '</div>';
        }).join('') + '</div>';

    $("#checklist-modal-conteudo").html(
        '<div class="detail-grid mb-3">' +
        detailItem('Data/hora', checklist.dataHora || '-') +
        detailItem('Motorista', checklist.motoristaNome || '-') +
        detailItem('Veículo', checklist.veiculoDescricao || '-') +
        detailItem('Resumo', resumo) +
        '</div>' +
        itensHtml
    );
}

function obterExecucoesAtivasParaChat() {
    return (rotasMonitoramentoResumoData || [])
        .filter(function (rota) { return !ehFinalizada(rota); })
        .map(function (rota) { return rota.execucaoId || obterLayerId(rota); })
        .filter(function (id) { return !!id; });
}

function atualizarNaoLidasChatRotas() {
    var execucoesIds = obterExecucoesAtivasParaChat();
    if (execucoesIds.length === 0) {
        chatNaoLidasPorExecucao = {};
            window.monitoramentoRotasChatNaoLidas = chatNaoLidasPorExecucao;
            renderizarBadgeChatSelecionado();
            atualizarKpis(rotasMonitoramentoResumoData);
            atualizarTabelaRotas(obterRotasVisiveisNoMapa());
            if (monitoramentoFiltroRapido === 'com-mensagem') renderizarRotasNoMapa();
            return;
        }

    $.ajax({
        url: '/Rota/ObterNaoLidasChatRotas',
        method: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(execucoesIds),
        success: function (response) {
            if (!response.sucesso) return;

            var mapa = {};
            (response.dados || []).forEach(function (item) {
                mapa[item.rotaExecucaoId] = item.quantidade || 0;
            });

            detectarMensagensRecebidasChat(mapa);
            chatNaoLidasPorExecucao = mapa;
            window.monitoramentoRotasChatNaoLidas = chatNaoLidasPorExecucao;
            renderizarBadgeChatSelecionado();
            atualizarKpis(rotasMonitoramentoResumoData);
            atualizarTabelaRotas(obterRotasVisiveisNoMapa());
            if (monitoramentoFiltroRapido === 'com-mensagem') renderizarRotasNoMapa();
        }
    });
}

function detectarMensagensRecebidasChat(mapaAtual) {
    mapaAtual = mapaAtual || {};

    if (!chatNaoLidasBaselineInicializada) {
        chatNaoLidasBaselineInicializada = true;
        chatUltimaQuantidadeAlertadaPorExecucao = Object.assign({}, mapaAtual);
        return;
    }

    Object.keys(chatUltimaQuantidadeAlertadaPorExecucao).forEach(function (id) {
        if (!mapaAtual[id] || mapaAtual[id] <= 0) {
            chatUltimaQuantidadeAlertadaPorExecucao[id] = 0;
        }
    });

    Object.keys(mapaAtual).forEach(function (id) {
        var quantidadeAtual = mapaAtual[id] || 0;
        var ultimaAlertada = chatUltimaQuantidadeAlertadaPorExecucao[id] || 0;

        if (quantidadeAtual <= ultimaAlertada) return;
        chatUltimaQuantidadeAlertadaPorExecucao[id] = quantidadeAtual;

        if (rotaSelecionadaExecucaoId && String(rotaSelecionadaExecucaoId) === String(id)) return;

        var rota = encontrarRota(id);
        if (!rota) return;

        enfileirarAlertaMensagemChat(rota);
    });
}

function enfileirarAlertaMensagemChat(rota) {
    chatAlertasMensagemFila.push(rota);
    mostrarProximoAlertaMensagemChat();
}

function mostrarProximoAlertaMensagemChat() {
    if (chatAlertaMensagemAtivo || chatAlertasMensagemFila.length === 0) return;

    var rota = chatAlertasMensagemFila.shift();
    var execucaoId = rota.execucaoId || obterLayerId(rota);
    var motorista = obterPrimeiroNome(rota.motoristaNome || "Motorista");
    var veiculo = montarVeiculoAlertaChat(rota);
    var descricao = rota.descricao || "Rota";
    var titulo = "MENSAGEM RECEBIDA DE " + motorista.toUpperCase();
    var detalhes = veiculo + " - " + descricao;

    chatAlertaMensagemAtivo = true;

    if (typeof Swal === "undefined") {
        if (confirm(titulo + "\n" + detalhes + "\n\nDeseja focar nesta rota?")) {
            selecionarRota(execucaoId, true);
        }
        chatAlertaMensagemAtivo = false;
        mostrarProximoAlertaMensagemChat();
        return;
    }

    Swal.fire({
        toast: true,
        position: "top-end",
        html: montarHtmlAlertaMensagemChat(titulo, detalhes),
        customClass: {
            popup: "monitor-chat-toast-popup",
            htmlContainer: "monitor-chat-toast-html"
        },
        showConfirmButton: false,
        timer: 12000,
        timerProgressBar: true,
        didOpen: function (popup) {
            popup.style.cursor = "pointer";
            popup.addEventListener("click", function () {
                selecionarRota(execucaoId, true);
                Swal.close();
            });
        }
    }).then(function () {
        chatAlertaMensagemAtivo = false;
        mostrarProximoAlertaMensagemChat();
    });
}

function montarHtmlAlertaMensagemChat(titulo, detalhes) {
    return '<div class="monitor-chat-toast-content">' +
        '<div class="monitor-chat-toast-icon"><i class="bx bx-message-rounded-dots"></i></div>' +
        '<div class="monitor-chat-toast-text">' +
        '<strong>' + escapeHtml(titulo) + '</strong>' +
        '<span>' + escapeHtml(detalhes) + '</span>' +
        '<small>Clique no alerta para focar a rota.</small>' +
        '</div>' +
        '</div>';
}

function obterPrimeiroNome(nome) {
    var partes = String(nome || "").trim().split(/\s+/).filter(function (p) { return !!p; });
    return partes.length > 0 ? partes[0] : "Motorista";
}

function montarVeiculoAlertaChat(rota) {
    var texto = rota.placaModelo || rota.veiculoDescricao || rota.veiculoNome || "Veículo não informado";
    return String(texto).replace(/\s+-\s+/, " / ");
}

function renderizarBadgeChatSelecionado() {
    var $badge = $("#badge-chat-rota");
    if (!$badge.length) return;

    var quantidade = rotaSelecionadaExecucaoId ? (chatNaoLidasPorExecucao[rotaSelecionadaExecucaoId] || 0) : 0;
    if (quantidade > 0) {
        $badge.text(quantidade > 99 ? "99+" : quantidade).removeClass("d-none");
    } else {
        $badge.text("0").addClass("d-none");
    }
}

function iniciarPollingChatAberto() {
    pararPollingChatAberto();
    chatAbertoInterval = setInterval(function () {
        carregarChatRota(true);
    }, 5000);
}

function pararPollingChatAberto() {
    if (chatAbertoInterval) {
        clearInterval(chatAbertoInterval);
        chatAbertoInterval = null;
    }
}

function abrirChatRota() {
    if (!rotaSelecionadaExecucaoId) return;

    var rota = encontrarRota(rotaSelecionadaExecucaoId);
    if (!rota) return;

    $("#chat-rota-titulo").text("Chat - " + (rota.descricao || "Rota"));
    $("#chat-rota-subtitulo").text((rota.placaModelo || "Veículo não informado") + " | " + (rota.motoristaNome || "Motorista não informado"));
    $("#chat-rota-mensagens").html('<div class="empty-state">Carregando mensagens...</div>');
    $("#chat-rota-texto").val("");

    var modal = bootstrap.Modal.getOrCreateInstance(document.getElementById("modal-chat-rota"));
    modal.show();

    carregarChatRota(true);
    iniciarPollingChatAberto();
}

function carregarChatRota(silencioso) {
    if (!rotaSelecionadaExecucaoId || chatCarregando) return;

    chatCarregando = true;
    if (!silencioso) {
        $("#chat-rota-mensagens").html('<div class="empty-state">Carregando mensagens...</div>');
    }
    $.getJSON('/Rota/ObterChatRota?execucaoId=' + encodeURIComponent(rotaSelecionadaExecucaoId), function (response) {
        if (!response.sucesso) {
            $("#chat-rota-mensagens").html('<div class="empty-state">' + escapeHtml(response.mensagem || 'Não foi possível carregar o chat.') + '</div>');
            return;
        }

        chatRotaResumoAtual = response.dados;
        chatNaoLidasPorExecucao[rotaSelecionadaExecucaoId] = 0;
        window.monitoramentoRotasChatNaoLidas = chatNaoLidasPorExecucao;
        renderizarBadgeChatSelecionado();
        renderizarChatRota(response.dados);
    }).always(function () {
        chatCarregando = false;
    });
}

function renderizarChatRota(chat) {
    var mensagens = (chat && chat.mensagens) || [];
    var finalizada = chat && chat.finalizada === true;

    $("#chat-rota-finalizada").toggleClass("d-none", !finalizada);
    $("#chat-rota-texto").prop("disabled", finalizada);
    $("#chat-rota-enviar").prop("disabled", finalizada);

    if (mensagens.length === 0) {
        $("#chat-rota-mensagens").html('<div class="empty-state">Nenhuma mensagem enviada nesta rota.</div>');
        return;
    }

    $("#chat-rota-mensagens").html(mensagens.map(function (m) {
        var classe = m.origem === 1 ? 'sistema' : 'aplicativo';
        return '<div class="route-chat-message ' + classe + '">' +
            '<strong>' + escapeHtml(m.remetenteNome || m.origemDescricao || 'Usuário') + '</strong>' +
            '<span>' + escapeHtml(m.mensagem || '') + '</span>' +
            '<small>' + escapeHtml(m.dataHoraEnvioFormatada || '') + '</small>' +
            '</div>';
    }).join(''));

    var box = document.getElementById("chat-rota-mensagens");
    if (box) box.scrollTop = box.scrollHeight;
}

function enviarMensagemChatRota() {
    if (!rotaSelecionadaExecucaoId || (chatRotaResumoAtual && chatRotaResumoAtual.finalizada)) return;

    var texto = ($("#chat-rota-texto").val() || "").trim();
    if (!texto) return;

    $("#chat-rota-enviar").prop("disabled", true);

    $.ajax({
        url: '/Rota/EnviarMensagemChatRota',
        method: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({
            rotaExecucaoId: rotaSelecionadaExecucaoId,
            mensagem: texto
        }),
        success: function (response) {
            if (!response.sucesso) {
                alert(response.mensagem || 'Não foi possível enviar a mensagem.');
                return;
            }

            $("#chat-rota-texto").val("");
            carregarChatRota(true);
        },
        error: function () {
            alert('Não foi possível enviar a mensagem.');
        },
        complete: function () {
            if (!chatRotaResumoAtual || !chatRotaResumoAtual.finalizada) {
                $("#chat-rota-enviar").prop("disabled", false);
            }
        }
    });
}

function atualizarAlertasGerais(rotas) {
    renderizarAlertasPorRota(montarUltimoAlertaPorRota(rotas));
}

function renderizarAlertasPorRota(alertas) {
    var $lista = $("#alertas-rotas-lista");
    var $count = $("#alerts-routes-count");

    if (!alertas || alertas.length === 0) {
        $count.text("0 alertas");
        $lista.html('<div class="empty-state">Sem alertas para exibir.</div>');
        return;
    }

    $count.text(alertas.length === 1 ? "1 alerta" : alertas.length + " alertas");
    $lista.html(alertas.map(function (a) {
        var click = a.execucaoId ? ' onclick="selecionarRota(\'' + escapeAttr(a.execucaoId) + '\', true)"' : '';
        return '<div class="alert-route-card ' + escapeAttr(a.severidade || 'baixo') + '"' + click + '>' +
            '<strong>' + escapeHtml(a.titulo || 'Alerta') + '</strong>' +
            '<span>' + escapeHtml(a.mensagem || '') + '</span>' +
            '</div>';
    }).join(''));
}

function renderizarLinhaDoTempoRota(rota) {
    var itens = [];
    var dataBase = obterDataBaseTimeline(rota);
    var eventos = rota.eventosRecentes || [];
    var eventosParada = eventos
        .map(function (evento, idx) { return { evento: evento, index: idx }; })
        .filter(function (item) { return item.evento.tipoEvento === 3 || item.evento.tipoDescricao === 'Parada confirmada'; })
        .sort(function (a, b) {
            return extrairDataOrdenacaoTimeline(a.evento.dataHora, dataBase) - extrairDataOrdenacaoTimeline(b.evento.dataHora, dataBase);
        });
    var indiceEventoParada = 0;
    timelinePontosMapa = [];

    if (rota.horaInicio) {
        var dataInicioTimeline = formatarDataHoraTimeline(rota.horaInicio, dataBase);
        itens.push({
            titulo: 'Rota iniciada',
            detalhe: dataInicioTimeline,
            ordem: extrairDataOrdenacaoTimeline(rota.horaInicio, dataBase),
            tipo: 'inicio',
            icone: 'bx-play'
        });
    }

    eventos.forEach(function (evento, idx) {
        if (evento.tipoEvento === 1 || evento.tipoDescricao === 'Rota iniciada') return;
        if (evento.tipoEvento === 3 || evento.tipoDescricao === 'Parada confirmada') return;
        if ((evento.tipoEvento === 5 || evento.tipoDescricao === 'Rota finalizada') && !ehFinalizada(rota)) return;

        var unidade = obterApresentacaoEventoUnidade(evento, rota, dataBase);
        var latitudeEvento = possuiCoordenadaValida(evento.latitude) ? evento.latitude : unidade.latitude;
        var longitudeEvento = possuiCoordenadaValida(evento.longitude) ? evento.longitude : unidade.longitude;
        var dataEventoTimeline = formatarDataHoraTimeline(evento.dataHora, dataBase);
        itens.push({
            titulo: unidade.titulo || evento.tipoDescricao || 'Evento',
            detalhe: unidade.detalhe || (dataEventoTimeline + (evento.observacao ? ' | ' + evento.observacao : '')),
            ordem: extrairDataOrdenacaoTimeline(evento.dataHora, dataBase),
            tipo: unidade.tipo || 'evento',
            icone: unidade.icone || 'bxs-bell-ring',
            severidade: obterSeveridadeEvento(evento),
            pontoMapaIndex: registrarPontoTimelineMapa(unidade.titulo || evento.tipoDescricao || 'Evento', dataEventoTimeline, unidade.observacaoMapa || evento.observacao, latitudeEvento, longitudeEvento)
        });
    });

    (rota.paradas || []).forEach(function (parada, idx) {
        if (!parada.entregue && !parada.concluidoEm) return;
        var eventoParada = eventosParada[indiceEventoParada] ? eventosParada[indiceEventoParada].evento : null;
        indiceEventoParada++;
        var dataParada = eventoParada && eventoParada.dataHora ? eventoParada.dataHora : parada.concluidoEm;
        var observacaoParada = eventoParada && eventoParada.observacao ? eventoParada.observacao : '';
        var latParada = eventoParada && possuiCoordenadaValida(eventoParada.latitude) ? eventoParada.latitude : parada.latitude;
        var lngParada = eventoParada && possuiCoordenadaValida(eventoParada.longitude) ? eventoParada.longitude : parada.longitude;
        var dataParadaTimeline = formatarDataHoraTimeline(dataParada, dataBase);
        var detalheParada = (parada.entregue ? 'Concluída' : 'Não realizada') + (dataParadaTimeline ? ' | ' + dataParadaTimeline : '') + (observacaoParada ? ' | ' + observacaoParada : '') + (parada.registradoOffline ? ' | offline' : '');

        itens.push({
            titulo: 'P' + (idx + 1) + ' - ' + (parada.nome || 'Parada'),
            detalhe: (parada.entregue ? 'Concluída' : 'Não realizada') + (dataParada ? ' | ' + dataParada : '') + (observacaoParada ? ' | ' + observacaoParada : '') + (parada.registradoOffline ? ' | offline' : ''),
            detalhe: detalheParada,
            ordem: extrairDataOrdenacaoTimeline(dataParada, dataBase),
            tipo: parada.entregue ? 'parada-realizada' : 'parada-nao-realizada',
            icone: parada.entregue ? 'bx-check' : 'bx-x',
            pontoMapaIndex: registrarPontoTimelineMapa('P' + (idx + 1) + ' - ' + (parada.nome || 'Parada'), dataParadaTimeline, observacaoParada, latParada, lngParada)
        });
    });

    if (ehFinalizada(rota) && rota.horaFim && rota.horaFim !== 'Em Execução') {
        itens.push({
            titulo: 'Rota finalizada',
            detalhe: rota.horaFim,
            detalhe: formatarDataHoraTimeline(rota.horaFim, dataBase),
            ordem: extrairDataOrdenacaoTimeline(rota.horaFim, dataBase),
            tipo: 'fim',
            icone: 'bxs-flag-checkered'
        });
    }

    itens = itens
        .filter(function (item) { return !!item.titulo; })
        .sort(function (a, b) { return (a.ordem || 0) - (b.ordem || 0); });

    if (itens.length === 0) {
        $("#side-timeline").html('<div class="empty-state">Nenhum marco registrado nesta rota.</div>');
        return;
    }

    $("#side-timeline").html(itens.map(function (item) {
        var classePonto = item.tipo === 'evento' ? (item.severidade || 'evento') : item.tipo;
        var botaoPin = item.pontoMapaIndex !== null && item.pontoMapaIndex !== undefined
            ? '<button type="button" class="btn btn-outline-danger btn-sm timeline-pin-btn" onclick="destacarPontoLinhaDoTempoNoMapa(' + item.pontoMapaIndex + ', event)" title="Exibir local no mapa"><i class="bx bxs-map-pin"></i></button>'
            : '';

        return '<div class="timeline-item">' +
            '<div class="timeline-dot ' + escapeAttr(classePonto) + '"><i class="bx ' + escapeAttr(item.icone || 'bx-circle') + '"></i></div>' +
            '<div class="timeline-content">' +
            '<strong>' + escapeHtml(item.titulo) + '</strong>' +
            '<span>' + escapeHtml(item.detalhe || '-') + '</span>' +
            '</div>' +
            botaoPin +
            '</div>';
    }).join(''));
}

function registrarPontoTimelineMapa(titulo, dataHora, observacao, latitude, longitude) {
    if (!possuiCoordenadaValida(latitude) || !possuiCoordenadaValida(longitude)) return null;

    timelinePontosMapa.push({
        titulo: titulo || 'Evento',
        dataHora: dataHora || '',
        observacao: observacao || '',
        latitude: parseFloat(latitude),
        longitude: parseFloat(longitude)
    });

    return timelinePontosMapa.length - 1;
}

function obterApresentacaoEventoUnidade(evento, rota, dataBase) {
    if (!evento) return {};

    if (evento.tipoEvento === 2 || evento.tipoDescricao === 'Origem confirmada') {
        var origem = rota && rota.unidadeOrigem ? rota.unidadeOrigem : null;
        return {
            titulo: montarTituloUnidadeLinhaDoTempo(origem, 'Origem'),
            detalhe: montarDetalheUnidadeLinhaDoTempo(evento, origem, dataBase),
            observacaoMapa: obterEnderecoUnidade(origem) || evento.observacao,
            latitude: origem ? origem.latitude : null,
            longitude: origem ? origem.longitude : null,
            tipo: 'parada',
            icone: 'bx-check'
        };
    }

    if (evento.tipoEvento === 4 || evento.tipoDescricao === 'Chegada ao destino') {
        var destino = rota && rota.unidadeDestino ? rota.unidadeDestino : null;
        return {
            titulo: montarTituloUnidadeLinhaDoTempo(destino, 'Destino'),
            detalhe: montarDetalheUnidadeLinhaDoTempo(evento, destino, dataBase),
            observacaoMapa: obterEnderecoUnidade(destino) || evento.observacao,
            latitude: destino ? destino.latitude : null,
            longitude: destino ? destino.longitude : null,
            tipo: 'parada',
            icone: 'bx-check'
        };
    }

    return {};
}

function montarTituloUnidadeLinhaDoTempo(unidade, fallback) {
    var nome = unidade && unidade.nome ? unidade.nome : fallback;
    var endereco = obterEnderecoUnidade(unidade);

    return endereco ? nome + ' - ' + endereco : nome;
}

function obterEnderecoUnidade(unidade) {
    return unidade && unidade.endereco ? unidade.endereco : '';
}

function montarDetalheUnidadeLinhaDoTempo(evento, unidade, dataBase) {
    var partes = ['Concluída'];
    if (evento && evento.dataHora) partes.push(formatarDataHoraTimeline(evento.dataHora, dataBase));
    if (evento && evento.observacao) partes.push(evento.observacao);
    if (evento && evento.registradoOffline) partes.push('offline');

    return partes.join(' | ');
}

function destacarPontoLinhaDoTempoNoMapa(indice, ev) {
    if (ev && ev.stopPropagation) ev.stopPropagation();
    var ponto = timelinePontosMapa[indice];
    if (!ponto || !map) return;
    destacarPontoNoMapa(ponto.titulo, ponto.dataHora, ponto.observacao, ponto.latitude, ponto.longitude);
}

function destacarEventoNoMapa(indiceEvento, ev) {
    if (ev && ev.stopPropagation) ev.stopPropagation();
    if (!rotaSelecionadaExecucaoId || !map) return;

    var rota = encontrarRota(rotaSelecionadaExecucaoId);
    if (!rota) return;

    var evento = (rota.eventosRecentes || [])[indiceEvento];
    if (!evento || !possuiCoordenadaValida(evento.latitude) || !possuiCoordenadaValida(evento.longitude)) return;

    var lat = parseFloat(evento.latitude);
    var lng = parseFloat(evento.longitude);

    destacarPontoNoMapa(evento.tipoDescricao || 'Evento', evento.dataHora, evento.observacao, lat, lng);
}

function destacarPontoNoMapa(titulo, dataHora, observacao, lat, lng) {
    removerPinEventoSelecionado();

    var icon = L.divIcon({
        className: 'route-event-pin',
        html: '<div class="route-event-pin-inner"><i class="bx bxs-map-pin"></i></div>',
        iconSize: [34, 34],
        iconAnchor: [17, 32],
        popupAnchor: [0, -30]
    });

    eventoSelecionadoMarker = L.marker([lat, lng], { icon: icon, zIndexOffset: 1200 }).addTo(map);
    eventoSelecionadoMarker.on('popupclose', function () {
        if (eventoSelecionadoMarker) removerPinEventoSelecionado();
    });
    eventoSelecionadoMarker.bindPopup(
        '<div style="min-width:210px;padding:4px;">' +
        '<strong>' + escapeHtml(titulo || 'Evento') + '</strong>' +
        '<div style="font-size:.82rem;color:#475569;margin-top:4px;">' + escapeHtml(dataHora || '') + '</div>' +
        (observacao ? '<div style="font-size:.82rem;color:#475569;margin-top:4px;">' + escapeHtml(observacao) + '</div>' : '') +
        '</div>'
    ).openPopup();

    map.setView([lat, lng], Math.max(map.getZoom(), 16));
}

function removerPinEventoSelecionado() {
    if (eventoSelecionadoMarker && map && map.hasLayer(eventoSelecionadoMarker)) {
        map.removeLayer(eventoSelecionadoMarker);
    }
    eventoSelecionadoMarker = null;
}

function possuiCoordenadaValida(valor) {
    if (valor === null || valor === undefined || valor === '') return false;
    var numero = parseFloat(valor);
    return !isNaN(numero) && numero !== 0;
}

function renderizarParadas(paradas) {
    if (!paradas || paradas.length === 0) {
        $("#side-paradas").html('<div class="empty-state">Nenhuma parada cadastrada.</div>');
        return;
    }

    $("#side-paradas").html(paradas.map(function (p, idx) {
        return '<div class="stop-item">' +
            '<strong>P' + (idx + 1) + ' - ' + escapeHtml(p.nome || 'Parada') + '</strong>' +
            '<span>' + (p.entregue ? 'Concluída ' + escapeHtml(p.concluidoEm || '') : 'Pendente') + (p.registradoOffline ? ' | offline' : '') + '</span>' +
            '</div>';
    }).join(''));
}

function montarAlertasGerais(rotas) {
    var alertas = [];

    (rotas || []).forEach(function (rota) {
        var execucaoId = rota.execucaoId || obterLayerId(rota);
        var rotaLinha = montarLinhaIdentificacaoAlerta(rota);

        (rota.alertas || []).forEach(function (alerta) {
            alertas.push({
                execucaoId: execucaoId,
                dataOrdenacao: extrairDataOrdenacao(rota.ultimaAtualizacao),
                severidade: alerta.severidade || 'baixo',
                titulo: rotaLinha,
                mensagem: alerta.titulo || alerta.mensagem || 'Alerta da rota'
            });
        });

        (rota.eventosRecentes || []).forEach(function (evento) {
            var mensagem = montarMensagemEventoAlerta(evento);
            if (!mensagem) return;

            alertas.push({
                execucaoId: execucaoId,
                dataOrdenacao: extrairDataOrdenacao(evento.dataHora),
                severidade: obterSeveridadeEvento(evento),
                titulo: rotaLinha,
                mensagem: mensagem
            });
        });
    });

    return alertas
        .sort(function (a, b) {
            var peso = obterPesoSeveridade(b.severidade) - obterPesoSeveridade(a.severidade);
            if (peso !== 0) return peso;
            return (b.dataOrdenacao || 0) - (a.dataOrdenacao || 0);
        });
}

function obterPesoSeveridade(severidade) {
    if (severidade === 'critico') return 4;
    if (severidade === 'alto') return 3;
    if (severidade === 'medio') return 2;
    return 1;
}

function montarUltimoAlertaPorRota(rotas) {
    var alertas = montarAlertasGerais(rotas).sort(function (a, b) {
        return (b.dataOrdenacao || 0) - (a.dataOrdenacao || 0);
    });
    var porRota = {};

    alertas.forEach(function (alerta) {
        if (!alerta.execucaoId) return;
        if (!porRota[alerta.execucaoId]) {
            porRota[alerta.execucaoId] = alerta;
        }
    });

    return Object.keys(porRota).map(function (key) { return porRota[key]; });
}

function montarLinhaIdentificacaoAlerta(rota) {
    var partes = [
        rota.descricao || 'Rota',
        rota.placaModelo || 'Veículo não informado',
        rota.motoristaNome || 'Motorista não informado'
    ];

    return partes.join(' - ');
}

function montarMensagemEventoAlerta(evento) {
    var tipo = evento.tipoDescricao || '';
    var quando = formatarQuandoAlerta(evento.dataHora);

    if (tipo === 'Rota iniciada') return 'Rota iniciada ' + quando + '.';
    if (tipo === 'Chegada ao destino') return 'Veículo chegou ao destino ' + quando + '.';
    if (tipo === 'Rota finalizada') return 'Rota finalizada ' + quando + '.';
    if (tipo === 'Origem confirmada') return 'Origem confirmada ' + quando + '.';
    if (tipo === 'Parada confirmada') return 'Parada confirmada ' + quando + '.';

    return '';
}

function obterSeveridadeEvento(evento) {
    if (evento.tipoDescricao === 'Rota finalizada') return 'baixo';
    if (evento.tipoDescricao === 'Chegada ao destino') return 'medio';
    return 'baixo';
}

function formatarQuandoAlerta(dataHora) {
    var data = parseDataPtBr(dataHora);
    if (!data) return '';

    var diffMinutos = Math.floor((new Date().getTime() - data.getTime()) / 60000);
    if (diffMinutos >= 0 && diffMinutos < 2) return 'agora';
    if (diffMinutos >= 2 && diffMinutos < 60) return 'há ' + diffMinutos + ' min';

    return 'às ' + data.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
}

function extrairDataOrdenacao(dataHora) {
    var data = parseDataPtBr(dataHora);
    return data ? data.getTime() : 0;
}

function extrairDataOrdenacaoTimeline(dataHora, dataBase) {
    var data = parseDataPtBr(dataHora);
    if (data) return data.getTime();

    if (!dataHora || !dataBase) return 0;
    var hora = String(dataHora).match(/(\d{2}):(\d{2})(?::(\d{2}))?/);
    if (!hora) return 0;

    return new Date(
        dataBase.getFullYear(),
        dataBase.getMonth(),
        dataBase.getDate(),
        Number(hora[1]),
        Number(hora[2]),
        Number(hora[3] || 0)
    ).getTime();
}

function formatarDataHoraTimeline(dataHora, dataBase) {
    var data = parseDataPtBr(dataHora);

    if (!data && dataHora && dataBase) {
        var hora = String(dataHora).match(/(\d{2}):(\d{2})(?::(\d{2}))?/);
        if (hora) {
            data = new Date(
                dataBase.getFullYear(),
                dataBase.getMonth(),
                dataBase.getDate(),
                Number(hora[1]),
                Number(hora[2]),
                Number(hora[3] || 0)
            );
        }
    }

    if (!data) return dataHora || '';

    return data.toLocaleDateString('pt-BR') + ' ' + data.toLocaleTimeString('pt-BR', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });
}

function obterDataBaseTimeline(rota) {
    var datas = [];
    if (rota) {
        datas.push(rota.horaInicio, rota.horaFim, rota.ultimaAtualizacao, rota.ultimaComunicacaoApp);
        (rota.eventosRecentes || []).forEach(function (e) { datas.push(e.dataHora); });
        (rota.paradas || []).forEach(function (p) { datas.push(p.concluidoEm); });
    }

    for (var i = 0; i < datas.length; i++) {
        var data = parseDataPtBr(datas[i]);
        if (data) return data;
    }

    var dataFiltro = $("#M_FiltroData").length ? $("#M_FiltroData").val() : "";
    var partes = String(dataFiltro).match(/^(\d{4})-(\d{2})-(\d{2})$/);
    if (partes) {
        return new Date(Number(partes[1]), Number(partes[2]) - 1, Number(partes[3]));
    }

    return new Date();
}

function parseDataPtBr(dataHora) {
    if (!dataHora) return null;
    var partes = String(dataHora).match(/^(\d{2})\/(\d{2})\/(\d{2}|\d{4})\s+(\d{2}):(\d{2})(?::(\d{2}))?/);
    if (!partes) return null;
    var ano = Number(partes[3]);
    if (ano < 100) ano += 2000;

    return new Date(
        ano,
        Number(partes[2]) - 1,
        Number(partes[1]),
        Number(partes[4]),
        Number(partes[5]),
        Number(partes[6] || 0)
    );
}

function atualizarTabelaRotas(rotas) {
    var $lista = $("#rotas-monitoramento-lista");
    var $count = $("#routes-list-count");
    rotas = rotas || [];

    $count.text(rotas.length === 1 ? "1 rota" : rotas.length + " rotas");

    var m = {};
    $("#_manutencao-modal-preview-inativo").append(
        detailItem('Categoria', m.categoria || '-') +
        detailItem('Agendamento', m.dataAgendamento || '-') +
        detailItem('Conclusão', m.dataConclusao || '-') +
        detailItem('Garantia', m.garantiaAte || '-') +
        detailItem('Local', m.localExecucaoServico || '-') +
        detailItem('Fornecedor', m.fornecedor || '-') +
        detailItem('Responsável', m.responsavelServico || '-') +
        detailItem('Contato', m.contatoFornecedor || '-') +
        detailItem('Documento', m.numeroDocumento || '-') +
        detailItem('Custo previsto', formatarMoedaMonitoramento(m.custoPrevisto)) +
        detailItem('Valor máximo', formatarMoedaMonitoramento(m.valorMaximoAutorizado)) +
        detailItem('Total gasto', formatarMoedaMonitoramento(m.valorTotalGasto)) +
        detailItem('Observação', m.observacao || '-')
    );

    if (rotas.length === 0) {
        $lista.html('<div class="empty-state">Nenhuma rota encontrada para os filtros atuais.</div>');
        return;
    }

    $lista.html(rotas.map(function (rota) {
        var status = obterStatusVisual(rota);
        var pacientes = formatarPessoas(rota.pacientes, "");
        var acompanhantes = formatarPessoas(rota.acompanhantes, "");
        var profissionais = formatarPessoas(rota.profissionais, "");
        var alertas = (rota.alertas || []).length;
        var id = rota.execucaoId || obterLayerId(rota);
        var chatNaoLidas = chatNaoLidasPorExecucao[id] || 0;

        return '<div class="route-summary-card" data-execucao-id="' + escapeAttr(id) + '" onclick="selecionarRota(\'' + escapeAttr(id) + '\', true)">' +
            '<div class="route-summary-head">' +
            '<strong>' + escapeHtml(rota.descricao || '-') + '</strong>' +
            '<span class="status-chip ' + status.slug + '">' + escapeHtml(status.label) + '</span>' +
            '</div>' +
            '<div class="route-summary-meta">' +
            '<div><i class="bx bxs-car"></i> ' + escapeHtml(rota.placaModelo || '-') + '</div>' +
            '<div><i class="bx bx-user"></i> ' + montarMotoristaComBadge(rota) + '</div>' +
            '</div>' +
            '<div class="route-summary-people">' +
            '<strong>Paciente:</strong> ' + escapeHtml(pacientes || '-') + '<br>' +
            '<strong>Acompanhante:</strong> ' + escapeHtml(acompanhantes || '-') + '<br>' +
            '<strong>Profissional:</strong> ' + escapeHtml(profissionais || '-') +
            '</div>' +
            '<div class="route-summary-footer">' +
            '<span>' + escapeHtml(rota.ultimaAtualizacao || '-') + '</span>' +
            '<span>' +
            (chatNaoLidas > 0 ? '<span class="badge bg-primary me-1">' + chatNaoLidas + ' chat</span>' : '') +
            (alertas > 0 ? '<span class="badge bg-danger">' + alertas + ' alertas</span>' : '<span class="badge bg-label-secondary">sem alertas</span>') +
            '</span>' +
            '</div>' +
            '</div>';
    }).join(''));

    atualizarLinhaSelecionada();
}

function atualizarManutencoes(manutencoesOrigem) {
    var manutencoes = (manutencoesOrigem || [])
        .filter(manutencaoDeveAparecer)
        .sort(function (a, b) {
            if (a.vencida !== b.vencida) return a.vencida ? -1 : 1;
            return (extrairDataManutencao(a.dataVencimento || a.dataManutencao) || 9999999999999) - (extrairDataManutencao(b.dataVencimento || b.dataManutencao) || 9999999999999);
        });

    manutencoesVeiculosMonitoramento = manutencoes;
    window.monitoramentoManutencoesVeiculos = manutencoesVeiculosMonitoramento;

    var $lista = $("#manutencoes-rotas-lista");
    var $count = $("#maintenance-list-count");

    $count.text(manutencoes.length === 1 ? "1 manutenção" : manutencoes.length + " manutenções");

    if (manutencoes.length === 0) {
        $lista.html('<div class="empty-state">Sem manutenções próximas ou vencidas.</div>');
        return;
    }

    $lista.html(manutencoes.map(function (m, index) {
        var classe = m.vencida ? 'vencida' : 'proxima';
        var status = m.vencida ? 'Manutenção vencida' : 'Manutenção próxima';
        var detalhes = montarTextoManutencao(m);
        var custo = montarTextoCustoManutencao(m);

        var totalRotas = (m.rotasVinculadas || []).length;
        var rotasTexto = totalRotas === 1 ? "1 rota vinculada" : totalRotas + " rotas vinculadas";

        return '<div class="maintenance-card ' + classe + '" onclick="abrirModalManutencaoVeiculo(' + index + ')">' +
            '<strong>' + escapeHtml(status) + '</strong>' +
            '<span>' + escapeHtml(m.veiculoDescricao || 'Veículo não informado') + '</span>' +
            '<span>' + escapeHtml(detalhes) + '</span>' +
            (custo ? '<span><i class="bx bx-money"></i> ' + escapeHtml(custo) + '</span>' : '') +
            (m.localExecucaoServico || m.fornecedor ? '<span><i class="bx bx-map"></i> ' + escapeHtml(m.localExecucaoServico || m.fornecedor) + '</span>' : '') +
            '<span><i class="bx bx-route"></i> ' + escapeHtml(rotasTexto) + '</span>' +
            '</div>';
    }).join(''));
}

function abrirModalManutencaoVeiculo(indice) {
    var m = (manutencoesVeiculosMonitoramento || [])[indice];
    if (!m) return;

    var status = m.vencida ? 'Manutenção vencida' : 'Manutenção próxima';
    var rotas = m.rotasVinculadas || [];

    $("#manutencao-modal-titulo").text(status);
    $("#manutencao-modal-veiculo").text(m.veiculoDescricao || 'Veículo não informado');
    $("#manutencao-modal-preview").html(
        '<div class="maintenance-modal-preview">' +
        detailItem('Descrição', m.descricao || '-') +
        detailItem('Situação', m.situacaoDescricao || '-') +
        detailItem('Data de referência', m.dataVencimento || m.dataManutencao || '-') +
        detailItem('Km atual', m.quilometragemAtual || '-') +
        detailItem('Km próxima', m.kmProximaManutencao || '-') +
        '</div>'
    );

    if (rotas.length === 0) {
        $("#manutencao-modal-rotas").html('<div class="empty-state">Este veículo não está vinculado a nenhuma rota carregada no período selecionado.</div>');
    } else {
        $("#manutencao-modal-rotas").html(rotas.map(function (rota) {
            var execucaoId = rota.execucaoId || '';
            var botao = execucaoId
                ? '<button type="button" class="btn btn-sm btn-outline-primary" onclick="focarRotaPeloModalManutencao(\'' + escapeAttr(execucaoId) + '\')"><i class="bx bx-current-location"></i> Focar</button>'
                : '';

            return '<div class="maintenance-route-item">' +
                '<div>' +
                '<strong>' + escapeHtml(rota.descricao || 'Rota') + '</strong>' +
                '<span>' + escapeHtml((rota.motoristaNome || 'Motorista não informado') + ' | ' + (rota.statusDescricao || '-') + ' | ' + (rota.dataParaExecucao || '-')) + '</span>' +
                '</div>' +
                botao +
                '</div>';
        }).join(''));
    }

    $("#manutencao-modal-preview .maintenance-modal-preview").append(
        detailItem('Categoria', m.categoria || '-') +
        detailItem('Agendamento', m.dataAgendamento || '-') +
        detailItem('Conclusão', m.dataConclusao || '-') +
        detailItem('Garantia', m.garantiaAte || '-') +
        detailItem('Local', m.localExecucaoServico || '-') +
        detailItem('Fornecedor', m.fornecedor || '-') +
        detailItem('Responsável', m.responsavelServico || '-') +
        detailItem('Contato', m.contatoFornecedor || '-') +
        detailItem('Documento', m.numeroDocumento || '-') +
        detailItem('Custo previsto', formatarMoedaMonitoramento(m.custoPrevisto)) +
        detailItem('Valor máximo', formatarMoedaMonitoramento(m.valorMaximoAutorizado)) +
        detailItem('Total gasto', formatarMoedaMonitoramento(m.valorTotalGasto)) +
        detailItem('Observação', m.observacao || '-')
    );

    var modalEl = document.getElementById('modal-manutencao-veiculo');
    if (modalEl && window.bootstrap) {
        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    }
}

function focarRotaPeloModalManutencao(execucaoId) {
    var modalEl = document.getElementById('modal-manutencao-veiculo');
    if (modalEl && window.bootstrap) {
        bootstrap.Modal.getOrCreateInstance(modalEl).hide();
    }
    selecionarRota(execucaoId, true);
}

function manutencaoDeveAparecer(manutencao) {
    if (!manutencao) return false;
    if (manutencao.vencida) return true;

    var data = extrairDataManutencao(manutencao.dataVencimento || manutencao.dataManutencao);
    if (!data) return manutencao.proxima === true;

    var hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    var limite = new Date(hoje.getTime());
    limite.setDate(limite.getDate() + 30);

    return data >= hoje.getTime() && data <= limite.getTime();
}

function centralizarRotaSelecionada() {
    if (!rotaSelecionadaExecucaoId || !map) return;
    var rota = encontrarRota(rotaSelecionadaExecucaoId);
    if (!rota) return;

    var layerId = obterLayerId(rota);
    var layer = routeLayers[layerId];

    if (layer && layer.polyline && layer.polyline.getBounds && layer.polyline.getBounds().isValid()) {
        map.fitBounds(layer.polyline.getBounds(), { padding: [60, 60] });
        return;
    }

    if (rota.ultimaLocalizacao) {
        map.setView(rota.ultimaLocalizacao, 15);
    }
}

function limparFocoRota() {
    rotaSelecionadaExecucaoId = null;
    window.monitoramentoRotasSelecionada = null;
    comparativoPrevistoAtivo = false;
    removerRotaPrevista();
    removerPinEventoSelecionado();
    limparBarraLateral();
    renderizarRotasNoMapa();
    atualizarTabelaRotas(obterRotasVisiveisNoMapa());
    centralizarMapaTodo();
}

function limparBarraLateral() {
    $("#side-rota-titulo").text("Nenhuma rota selecionada");
    $("#side-rota-subtitulo").text("Clique em um veículo no mapa ou selecione uma rota na lista.");
    $("#side-rota-status").attr("class", "status-chip em-rota").text("Aguardando");
    $("#btn-chat-rota").prop("disabled", true);
    $("#btn-centralizar-rota").prop("disabled", true);
    $("#btn-limpar-foco-rota").prop("disabled", true);
    $("#btn-comparar-rota").prop("disabled", true).html('<i class="bx bx-git-compare"></i> Prevista x executada');
    $("#btn-checklist-rota").prop("disabled", true);
    $("#side-detalhes-rota").html('');
    $("#side-timeline").html('<div class="empty-state">Nenhuma rota selecionada.</div>');
    $("#side-paradas").html('<div class="empty-state">Nenhuma parada selecionada.</div>');
    chatRotaResumoAtual = null;
    renderizarBadgeChatSelecionado();
    atualizarLinhaSelecionada();
}

function centralizarMapaTodo() {
    if (!map) return;
    var layers = [];

    Object.keys(routeLayers).forEach(function (id) {
        var layer = routeLayers[id];
        if (layer.polyline) layers.push(layer.polyline);
        if (layer.marker) layers.push(layer.marker);
        if (layer.inicio) layers.push(layer.inicio);
        if (layer.fim) layers.push(layer.fim);
        (layer.paradas || []).forEach(function (p) { layers.push(p); });
    });

    if (layers.length === 0) return;

    try {
        var group = L.featureGroup(layers);
        map.fitBounds(group.getBounds(), { padding: [40, 40] });
    } catch (e) { }
}

function alternarComparativoRota() {
    if (!rotaSelecionadaExecucaoId) return;
    var rota = encontrarRota(rotaSelecionadaExecucaoId);
    if (!rota) return;

    comparativoPrevistoAtivo = !comparativoPrevistoAtivo;

    if (comparativoPrevistoAtivo) {
        renderizarRotaPrevista(rota);
        $("#btn-comparar-rota").html('<i class="bx bx-hide"></i> Ocultar prevista');
        centralizarRotaSelecionada();
    } else {
        removerRotaPrevista();
        $("#btn-comparar-rota").html('<i class="bx bx-git-compare"></i> Prevista x executada');
    }
}

function renderizarRotaPrevista(rota) {
    removerRotaPrevista();

    var pontos = montarPontosRotaPrevista(rota);
    if (pontos.length < 2 || !map) return;

    rotaPrevistaLayer = L.polyline(pontos, {
        color: '#111827',
        weight: 4,
        opacity: .85,
        dashArray: '10, 8',
        smoothFactor: 1
    }).addTo(map);

    rotaPrevistaLayer.bindPopup('<strong>Rota prevista</strong><br><small>Traçado previsto pelas paradas cadastradas.</small>', { closeButton: false });
}

function removerRotaPrevista() {
    if (rotaPrevistaLayer && map && map.hasLayer(rotaPrevistaLayer)) {
        map.removeLayer(rotaPrevistaLayer);
    }
    rotaPrevistaLayer = null;
}

function montarPontosRotaPrevista(rota) {
    var pontos = [];

    if (rota.historicoLocalizacoes && rota.historicoLocalizacoes.length > 0) {
        pontos.push(rota.historicoLocalizacoes[0]);
    } else if (rota.ultimaLocalizacao) {
        pontos.push(rota.ultimaLocalizacao);
    }

    (rota.paradas || []).forEach(function (p) {
        if (p.latitude !== null && p.latitude !== undefined && p.longitude !== null && p.longitude !== undefined) {
            pontos.push([p.latitude, p.longitude]);
        }
    });

    return pontos;
}

function alternarFocoMapa() {
    var $dashboard = $("#monitor-dashboard");
    var emFoco = !$dashboard.hasClass("mapa-em-foco");
    $dashboard.toggleClass("mapa-em-foco", emFoco);

    $("#btn-mapa-foco").html(emFoco
        ? '<i class="bx bx-exit-fullscreen"></i> Sair do foco'
        : '<i class="bx bx-fullscreen"></i> Focar mapa');

    setTimeout(function () {
        if (map) map.invalidateSize();
    }, 260);
}

function restaurarSelecaoAposAtualizacao() {
    if (!rotaSelecionadaExecucaoId) {
        return;
    }

    var rota = encontrarRota(rotaSelecionadaExecucaoId);
    if (rota) {
        atualizarBarraLateral(rota);
        renderizarRotasNoMapa();
        atualizarLinhaSelecionada();
    } else {
        rotaSelecionadaExecucaoId = null;
        window.monitoramentoRotasSelecionada = null;
        limparBarraLateral();
    }
}

function atualizarLinhaSelecionada() {
    $("#rotas-monitoramento-lista .route-summary-card").removeClass("active");
    if (!rotaSelecionadaExecucaoId) return;
    $('#rotas-monitoramento-lista .route-summary-card[data-execucao-id="' + rotaSelecionadaExecucaoId + '"]').addClass("active");
}

function encontrarRota(execucaoId) {
    return (rotasMonitoramentoResumoData || []).filter(function (r) {
        return String(r.execucaoId || obterLayerId(r)) === String(execucaoId);
    })[0];
}

function obterLayerId(rota) {
    return rota.execucaoId || (String(rota.rotaId) + (ehFinalizada(rota) ? '_fin' : '_act'));
}

function obterStatusVisual(rota) {
    var statusDescricao = (rota.statusDescricao || '').toLowerCase();
    var temOcorrencia = rota.sujeitoADesvio === true || possuiAlertaTipo(rota, 'desvio');

    if (temOcorrencia && !ehFinalizada(rota)) return { label: 'Em ocorrência', slug: 'em-ocorrencia', cor: '#dc2626' };
    if (ehFinalizada(rota)) return { label: 'Finalizada', slug: 'finalizada', cor: deixarCorMaisCinza(getPredefinedColor(rota.rotaId), .55) };
    if (statusDescricao.indexOf('almoço') >= 0 || statusDescricao.indexOf('pausa') >= 0 || rota.statusExecucao === 2) return { label: 'Em almoço', slug: 'em-almoco', cor: '#f59e0b' };
    return { label: rota.statusDescricao || 'Em rota', slug: 'em-rota', cor: getPredefinedColor(rota.rotaId) };
}

function ehFinalizada(rota) {
    return rota && (rota.finalizada === true || String(rota.finalizada).toLowerCase() === 'true' || rota.statusExecucao === 3);
}

function possuiAlertaTipo(rota, tipo) {
    return (rota.alertas || []).some(function (a) { return a.tipo === tipo; });
}

function possuiAlertaSeveridade(rota, severidades) {
    return (rota.alertas || []).some(function (a) { return severidades.indexOf(a.severidade) >= 0; });
}

function rotaPossuiAtrasoOperacional(rota) {
    if (!rota || ehFinalizada(rota)) return false;
    if (rota.possivelmenteOffline === true) return true;
    if (possuiAlertaTipo(rota, 'desvio') || possuiAlertaSeveridade(rota, ['critico', 'alto'])) return true;

    var ultima = parseDataPtBr(rota.ultimaComunicacaoApp || rota.ultimaAtualizacao);
    if (!ultima) return false;

    var diffMinutos = Math.floor((new Date().getTime() - ultima.getTime()) / 60000);
    return diffMinutos >= 30;
}

function formatarPessoas(lista, fallback) {
    if (lista && lista.length > 0) {
        return lista.map(function (p) {
            var nome = p.nome || 'Não informado';
            return p.complemento ? nome + ' (' + p.complemento + ')' : nome;
        }).join(', ');
    }
    return fallback || "";
}

function montarMotoristaComBadge(rota) {
    var nome = escapeHtml(rota.motoristaNome || 'Motorista não informado');
    return nome + montarBadgePapelMotorista(rota.motoristaPapel);
}

function montarBadgePapelMotorista(papel) {
    var texto = papel || 'Motorista';
    var slug = normalizarPapelMotorista(texto);
    return '<span class="driver-role-badge ' + escapeAttr(slug) + '">' + escapeHtml(texto) + '</span>';
}

function normalizarPapelMotorista(papel) {
    var texto = String(papel || '').toLowerCase();
    if (texto.indexOf('principal') >= 0) return 'principal';
    if (texto.indexOf('secund') >= 0) return 'secundario';
    return 'motorista';
}

function montarTextoManutencao(m) {
    var partes = [m.descricao || "Manutenção"];
    if (m.dataVencimento || m.dataManutencao) partes.push(m.dataVencimento || m.dataManutencao);
    if (m.kmProximaManutencao) partes.push("km " + m.kmProximaManutencao);
    if (m.vencida) partes.push("vencida");
    else if (m.proxima) partes.push("próxima");
    return partes.join(" | ");
}

function montarTextoCustoManutencao(m) {
    if (!m) return "";
    if (m.valorTotalGasto !== null && m.valorTotalGasto !== undefined) return "Gasto: " + formatarMoedaMonitoramento(m.valorTotalGasto);
    if (m.valorMaximoAutorizado !== null && m.valorMaximoAutorizado !== undefined) return "Limite: " + formatarMoedaMonitoramento(m.valorMaximoAutorizado);
    if (m.custoPrevisto !== null && m.custoPrevisto !== undefined) return "Previsto: " + formatarMoedaMonitoramento(m.custoPrevisto);
    return "";
}

function formatarMoedaMonitoramento(valor) {
    if (valor === null || valor === undefined || valor === "") return "-";
    var numero = Number(valor);
    if (isNaN(numero)) return "-";
    return numero.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
}

function extrairDataManutencao(dataTexto) {
    if (!dataTexto) return 0;
    var partes = String(dataTexto).match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
    if (!partes) return 0;
    return new Date(Number(partes[3]), Number(partes[2]) - 1, Number(partes[1])).getTime();
}

function detailItem(label, value) {
    return '<div class="detail-item"><span>' + escapeHtml(label) + '</span><strong>' + escapeHtml(value) + '</strong></div>';
}

function detailItemHtml(label, valueHtml) {
    return '<div class="detail-item"><span>' + escapeHtml(label) + '</span><strong>' + (valueHtml || '-') + '</strong></div>';
}

function removerCamada(layer) {
    if (layer && map && map.hasLayer(layer)) {
        map.removeLayer(layer);
    }
}

function montarSegmentosOfflineMonitoramento(pontos) {
    var segmentos = [];
    var atual = [];

    pontos.forEach(function (p) {
        if (p.registradoOffline === true) {
            atual.push([p.latitude, p.longitude]);
            return;
        }

        if (atual.length > 0) {
            segmentos.push(atual);
            atual = [];
        }
    });

    if (atual.length > 0) segmentos.push(atual);
    return segmentos;
}

function calcularHeading(rota) {
    if (!rota.historicoLocalizacoes || rota.historicoLocalizacoes.length === 0 || !rota.ultimaLocalizacao) return 0;

    var p1 = rota.historicoLocalizacoes[rota.historicoLocalizacoes.length - 1];
    var p2 = rota.ultimaLocalizacao;
    var lat1 = p1[0] * Math.PI / 180;
    var lat2 = p2[0] * Math.PI / 180;
    var dLon = (p2[1] - p1[1]) * Math.PI / 180;
    var y = Math.sin(dLon) * Math.cos(lat2);
    var x = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);
    return (Math.atan2(y, x) * 180 / Math.PI + 360) % 360;
}

function getPredefinedColor(id) {
    var colors = [
        '#2563eb', '#16a34a', '#dc2626', '#f59e0b', '#0891b2',
        '#4f46e5', '#0f766e', '#be123c', '#64748b', '#84cc16',
        '#0284c7', '#ca8a04', '#334155', '#059669', '#ea580c'
    ];
    return colors[Math.abs(id || 0) % colors.length];
}

function deixarCorMaisCinza(hex, intensidade) {
    hex = String(hex || '#64748b').replace('#', '');
    if (hex.length === 3) hex = hex.split('').map(function (x) { return x + x; }).join('');

    var r = parseInt(hex.substring(0, 2), 16);
    var g = parseInt(hex.substring(2, 4), 16);
    var b = parseInt(hex.substring(4, 6), 16);
    var gray = Math.round(r * .299 + g * .587 + b * .114);
    var blend = intensidade || .45;

    r = Math.round(r * (1 - blend) + gray * blend);
    g = Math.round(g * (1 - blend) + gray * blend);
    b = Math.round(b * (1 - blend) + gray * blend);

    return '#' + r.toString(16).padStart(2, '0') + g.toString(16).padStart(2, '0') + b.toString(16).padStart(2, '0');
}

function escapeHtml(value) {
    if (value === null || value === undefined) return '';
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function escapeAttr(value) {
    return escapeHtml(value).replace(/`/g, '&#096;');
}
