var map = window.monitoramentoRotasMap || null;
var allRoutesData = window.monitoramentoRotasData || [];
var routeLayers = window.monitoramentoRotasRouteLayers || {};

$(document).ready(function() {
    if ($.fn.select2) {
        $('#M_FiltroRota, #M_FiltroMotorista, #M_FiltroVeiculo, #M_FiltroTipoVeiculo').select2({
            width: '100%',
            placeholder: 'Escolha uma opção',
            allowClear: true,
            dropdownParent: $('#filtros-overlay')
        });
    }
});

function initMap() {
    let lat = typeof mapLatCentro !== 'undefined' ? mapLatCentro : -15.7942;
    let lon = typeof mapLonCentro !== 'undefined' ? mapLonCentro : -47.8821;

    if (window.monitoramentoRotasInterval) {
        clearInterval(window.monitoramentoRotasInterval);
        window.monitoramentoRotasInterval = null;
    }

    if (map) {
        map.remove();
        map = null;
    }
    routeLayers = {};
    window.monitoramentoRotasRouteLayers = routeLayers;

    map = L.map('map').setView([lat, lon], 12); // Default to Configured Location
    window.monitoramentoRotasMap = map;
    
    // Using the standard OpenStreetMap tiles which are more vibrant and colorful
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    obterDadosMonitoramento();
    // Poll data every 15 seconds for real time map update
    window.monitoramentoRotasInterval = setInterval(obterDadosMonitoramento, 15000);
    
    // Draw building indicators
    renderizarUnidadesNoMapa();
}

var markerUnidadesLayers = [];
function renderizarUnidadesNoMapa() {
    if (typeof mapUnidades === 'undefined' || !mapUnidades || mapUnidades.length === 0) return;

    mapUnidades.forEach(un => {
        let iconHtml = '<i class="bx bxs-building-house"></i>';
        let corBg = '#3498db';
        
        switch(un.TipoId) {
            case 0: iconHtml = '<i class="bx bxs-bank"></i>'; corBg = '#2c3e50'; break; // Instituicao Publica
            case 1: iconHtml = '<i class="bx bxs-buildings"></i>'; corBg = '#7f8c8d'; break; // Instituicao Privada
            case 2: iconHtml = '<i class="bx bx-plus-medical"></i>'; corBg = '#e74c3c'; break; // Saude
            case 3: iconHtml = '<i class="bx bxs-graduation"></i>'; corBg = '#f39c12'; break; // Educacao
            case 4: iconHtml = '<i class="bx bx-building"></i>'; corBg = '#9b59b6'; break; // Fundacao
        }

        let markerCss = `background: ${corBg}; color: white; border: 2px solid white; border-radius: 50%; width: 30px; height: 30px; display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 8px rgba(0,0,0,0.4); font-size: 16px;`;
        
        let customIcon = L.divIcon({
            className: 'custom-unidade-icon',
            html: `<div style="${markerCss}">${iconHtml}</div>`,
            iconSize: [30, 30],
            iconAnchor: [15, 15]
        });

        let marker = L.marker([un.Latitude, un.Longitude], { icon: customIcon }).addTo(map);
        
        let popupContent = `
            <div style="min-width: 180px; font-family: sans-serif; padding: 4px;">
                <h6 style="margin: 0; padding-bottom: 5px; border-bottom: 1px solid #ccc; color: ${corBg}; display: flex; align-items: center; gap: 5px;">
                    ${iconHtml} <strong style="font-size: 1.1em;">${un.Nome}</strong>
                </h6>
                <div style="margin-top: 8px; font-size: 0.9em; color: #555;">
                    <div><strong>Tipo:</strong> ${un.Tipo}</div>
                </div>
            </div>
        `;
        marker.bindPopup(popupContent);
        markerUnidadesLayers.push(marker);
    });
}

function obterDadosMonitoramento() {
    let dataFiltro = $("#M_FiltroData").length ? $("#M_FiltroData").val() : "";
    let finalizadas = $("#M_FiltroFinalizadas").length ? $("#M_FiltroFinalizadas").is(":checked") : false;

    $.getJSON(`/Rota/ObterDadosMonitoramento?dataFiltro=${dataFiltro}&exibirFinalizadas=${finalizadas}`, function(response) {
        if (response.sucesso) {
            allRoutesData = response.dados;
            window.monitoramentoRotasData = allRoutesData;
            atualizarContadorPossivelmenteOffline(allRoutesData);
            
            // Centraliza o mapa se tivermos dados e ainda não tivermos desenhado as rotas
            if (allRoutesData.length > 0 && allRoutesData[0].ultimaLocalizacao && Object.keys(routeLayers).length === 0) {
                 map.setView(allRoutesData[0].ultimaLocalizacao, 13);
            }
            
            renderizarRotasNoMapa();
            
            $("#ultima-sincronizacao").text("Sincronizado às " + new Date().toLocaleTimeString('pt-BR'));
        } else {
            console.error("Erro ao buscar dados de monitoramento:", response.mensagem);
        }
    });
}

function atualizarContadorPossivelmenteOffline(rotas) {
    let total = (rotas || []).filter(r => r.possivelmenteOffline === true).length;
    let $contador = $("#contador-possivelmente-offline");
    let $texto = $("#contador-possivelmente-offline-texto");

    if (!$contador.length || !$texto.length) return;

    if (total <= 0) {
        $contador.hide();
        $texto.text("0 rotas possivelmente offline");
        return;
    }

    $texto.text(total === 1 ? "1 rota possivelmente offline" : `${total} rotas possivelmente offline`);
    $contador.css("display", "flex");
}

function aplicarFiltrosMap() {
    renderizarRotasNoMapa();
}

function getPredefinedColor(id) {
    const colors = [
        '#e74c3c', '#2980b9', '#27ae60', '#f39c12', '#8e44ad', 
        '#16a085', '#d35400', '#c0392b', '#2c3e50', '#f1c40f', 
        '#3498db', '#9b59b6', '#34495e', '#1abc9c', '#e67e22',
        '#e84393', '#00cec9', '#fdcb6e', '#d63031', '#6c5ce7',
        '#00b894', '#0984e3', '#b2bec3', '#ff7675', '#a29bfe'
    ];
    return colors[id % colors.length];
}

function hexToGrayscale(hex) {
    hex = hex.replace('#', '');
    if (hex.length === 3) hex = hex.split('').map(x => x + x).join('');
    let r = parseInt(hex.substring(0, 2), 16);
    let g = parseInt(hex.substring(2, 4), 16);
    let b = parseInt(hex.substring(4, 6), 16);
    
    // Calculate luminance and push it safely to a gray range
    let gray = Math.round(r * 0.299 + g * 0.587 + b * 0.114);
    // Make sure it doesn't get too white or too black to be visible
    gray = Math.min(Math.max(gray, 80), 180);
    
    let grayHex = gray.toString(16).padStart(2, '0');
    return `#${grayHex}${grayHex}${grayHex}`;
}

function deixarCorMaisCinza(hex, intensidade) {
    hex = hex.replace('#', '');
    if (hex.length === 3) hex = hex.split('').map(x => x + x).join('');

    let r = parseInt(hex.substring(0, 2), 16);
    let g = parseInt(hex.substring(2, 4), 16);
    let b = parseInt(hex.substring(4, 6), 16);
    let gray = Math.round(r * 0.299 + g * 0.587 + b * 0.114);
    let blend = intensidade || 0.45;

    r = Math.round(r * (1 - blend) + gray * blend);
    g = Math.round(g * (1 - blend) + gray * blend);
    b = Math.round(b * (1 - blend) + gray * blend);

    return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
}

function renderizarRotasNoMapa() {
    let filtroMotorista = $("#M_FiltroMotorista").val();
    let filtroVeiculo = $("#M_FiltroVeiculo").val();
    let filtroRota = $("#M_FiltroRota").val();
    let filtroTipoVeiculo = $("#M_FiltroTipoVeiculo").val();

    let dadosFiltrados = allRoutesData.filter(r => {
        if (filtroMotorista && r.motoristaId.toString() !== filtroMotorista) return false;
        if (filtroVeiculo && r.veiculoId.toString() !== filtroVeiculo) return false;
        if (filtroRota && r.rotaId.toString() !== filtroRota) return false;
        if (filtroTipoVeiculo && r.tipoVeiculo !== null && r.tipoVeiculo !== undefined && r.tipoVeiculo.toString() !== filtroTipoVeiculo) return false;
        return true;
    });

    // Remove old polylines and markers correctly
    for (let id in routeLayers) {
        map.removeLayer(routeLayers[id].polyline);
        map.removeLayer(routeLayers[id].marker);
        if (routeLayers[id].inicio) {
            map.removeLayer(routeLayers[id].inicio);
        }
        if (routeLayers[id].paradas) {
            routeLayers[id].paradas.forEach(p => map.removeLayer(p));
        }
        if (routeLayers[id].offlineSegments) {
            routeLayers[id].offlineSegments.forEach(p => map.removeLayer(p));
        }
    }
    routeLayers = {};
    window.monitoramentoRotasRouteLayers = routeLayers;

    let group = new L.featureGroup();

    dadosFiltrados.sort((a, b) => {
        let aFin = (a.finalizada === true || String(a.finalizada).toLowerCase() === 'true');
        let bFin = (b.finalizada === true || String(b.finalizada).toLowerCase() === 'true');
        if (aFin === bFin) return 0;
        return aFin ? -1 : 1; // True comes first (under)
    });

    dadosFiltrados.forEach(rota => {
        let isFinalizada = (rota.finalizada === true || String(rota.finalizada).toLowerCase() === 'true');
        let isPossivelmenteOffline = rota.possivelmenteOffline === true;

        if (!rota.ultimaLocalizacao) {
            return;
        }
        
        let isPausada = false;
        if (!isFinalizada && rota.pausas && rota.pausas.length > 0) {
            let ultimaPausa = rota.pausas[rota.pausas.length - 1];
            if (!ultimaPausa.dataHoraFim) {
                isPausada = true;
            }
        }

        let layerId = rota.execucaoId || (rota.rotaId.toString() + (isFinalizada ? '_fin' : '_act'));
        
        let corViva = getPredefinedColor(rota.rotaId);
        let cor = isPossivelmenteOffline ? '#d63031' : (isFinalizada ? deixarCorMaisCinza(corViva, 0.45) : (isPausada ? '#e67e22' : corViva));
        let lineOpacity = isPossivelmenteOffline ? 0.95 : (isFinalizada ? 0.62 : (isPausada ? 0.6 : 0.8));
        let lineClass = isPossivelmenteOffline ? 'route-path possivelmente-offline-route' : (rota.possuiRegistroOffline ? 'route-path offline-route' : (isFinalizada ? 'route-path' : (isPausada ? 'route-path paused-route' : 'route-path animated-route')));
        
        // Polyline drawing the path
        let pathLine = L.polyline(rota.historicoLocalizacoes, {
            color: cor,
            weight: 5,
            opacity: lineOpacity,
            smoothFactor: 1,
            className: lineClass
        }).addTo(map);

        let offlineSegments = [];
        if (rota.historicoLocalizacoesDetalhado && rota.historicoLocalizacoesDetalhado.length > 1) {
            montarSegmentosOfflineMonitoramento(rota.historicoLocalizacoesDetalhado).forEach(seg => {
                if (seg.length < 2) return;
                let offlineLine = L.polyline(seg, {
                    color: '#f39c12',
                    weight: 6,
                    opacity: 0.9,
                    dashArray: '8, 8'
                }).addTo(map);
                offlineSegments.push(offlineLine);
            });
        }

        // Calcula o "rumo" (heading) para rotacionar o carro na direção correta
        let inicioMarker = null;
        if (rota.historicoLocalizacoes && rota.historicoLocalizacoes.length > 0) {
            let inicioCss = `background: ${corViva}; color: white; border: 2px solid white; border-radius: 50% 50% 50% 0; width: 26px; height: 26px; transform: rotate(-45deg); display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 8px rgba(0,0,0,0.35);`;
            let inicioIcon = L.divIcon({
                className: 'custom-inicio-rota-container',
                html: `<div style="${inicioCss}"><i class="bx bx-play" style="transform: rotate(45deg); font-size: 15px;"></i></div>`,
                iconSize: [30, 30],
                iconAnchor: [5, 25]
            });

            inicioMarker = L.marker(rota.historicoLocalizacoes[0], { icon: inicioIcon, zIndexOffset: 50 }).addTo(map);
            inicioMarker.bindPopup(`
                <div style="min-width: 150px; font-size: 0.9em; padding: 4px;">
                    <h6 style="margin-bottom: 8px; border-bottom: 1px solid #ccc; padding-bottom: 5px; color: ${corViva};"><strong>Inicio da rota</strong></h6>
                    <div><strong>Rota:</strong> ${rota.descricao}</div>
                    <div><strong>Inicio:</strong> ${rota.horaInicio || '--/--/--'}</div>
                    <div><strong>Motorista:</strong> ${rota.motoristaNome}</div>
                </div>
            `, { closeButton: false });
            group.addLayer(inicioMarker);
        }

        let heading = 0;
        if (rota.historicoLocalizacoes && rota.historicoLocalizacoes.length > 0) {
            let p1 = rota.historicoLocalizacoes[rota.historicoLocalizacoes.length - 1];
            let p2 = rota.ultimaLocalizacao;
            
            let lat1 = p1[0] * Math.PI / 180;
            let lat2 = p2[0] * Math.PI / 180;
            let dLon = (p2[1] - p1[1]) * Math.PI / 180;
            
            let y = Math.sin(dLon) * Math.cos(lat2);
            let x = Math.cos(lat1) * Math.sin(lat2) - Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);
            heading = (Math.atan2(y, x) * 180 / Math.PI + 360) % 360;
        }

        // SVG Dinâmico baseado no tipo de veículo
        let strokeColor = isPossivelmenteOffline ? "#fff3cd" : (isFinalizada ? "#999" : "white");
        let shadowColor = isPossivelmenteOffline ? "rgba(214,48,49,0.65)" : (isFinalizada ? "rgba(0,0,0,0.2)" : "rgba(0,0,0,0.5)");
        let opacityCar = isFinalizada ? "0.7" : "1";
        
        let innerSvg = "";
        switch(rota.tipoVeiculo) {
            case 1: // Motocicleta
                innerSvg = `
                    <rect x="11" y="2" width="2" height="4" fill="#333" rx="1"/> <!-- Roda Dianteira -->
                    <path d="M7 8 Q12 6 17 8" fill="none" stroke="#222" stroke-width="1.5" stroke-linecap="round"/> <!-- Guidão -->
                    <ellipse cx="12" cy="11" rx="3" ry="4" fill="${cor}" stroke="${strokeColor}" stroke-width="1"/> <!-- Tanque -->
                    <ellipse cx="12" cy="16" rx="2.5" ry="4" fill="#111" /> <!-- Assento -->
                    <rect x="11" y="18" width="2" height="4" fill="#333" rx="1"/> <!-- Roda Traseira -->
                `;
                break;
            case 2: // Caminhao
                innerSvg = `
                    <rect x="7" y="2" width="10" height="5" rx="2" fill="${cor}" stroke="${strokeColor}" stroke-width="1"/>
                    <rect x="8" y="4" width="8" height="2" fill="rgba(0,0,0,0.8)" rx="0.5"/>
                    <rect x="11" y="7" width="2" height="2" fill="#333" />
                    <rect x="7" y="9" width="10" height="13" rx="1" fill="${cor}" stroke="${strokeColor}" stroke-width="1"/>
                    <rect x="8" y="10" width="8" height="11" fill="rgba(255,255,255,0.2)" rx="0.5"/>
                `;
                break;
            case 3: // Onibus
                innerSvg = `
                    <rect x="6" y="1" width="12" height="22" rx="2" fill="${cor}" stroke="${strokeColor}" stroke-width="1"/>
                    <rect x="7" y="2" width="10" height="2" fill="rgba(0,0,0,0.8)" rx="0.5"/>
                    <rect x="7" y="20" width="10" height="1.5" fill="rgba(0,0,0,0.8)" rx="0.5"/>
                    <rect x="9" y="6" width="6" height="3" fill="#eee" rx="1"/>
                    <rect x="9" y="14" width="6" height="3" fill="#eee" rx="1"/>
                    <rect x="10.5" y="10" width="3" height="3" fill="rgba(0,0,0,0.3)" rx="0.5"/>
                `;
                break;
            case 4: // Ambulancia
                innerSvg = `
                    <rect x="6" y="2" width="12" height="20" rx="3" fill="white" stroke="${strokeColor}" stroke-width="1"/>
                    <rect x="7" y="5" width="10" height="3" fill="rgba(0,0,0,0.8)" rx="0.5"/>
                    <rect x="7" y="3" width="10" height="1.5" fill="${cor}" rx="0.5"/>
                    <rect x="10" y="8" width="4" height="2" fill="red" rx="1"/>
                    <path d="M12 12V18M9 15H15" stroke="red" stroke-width="2.5" stroke-linecap="square"/>
                `;
                break;
            default: // Carro (case 0 ou default)
                innerSvg = `
                    <rect x="5" y="2" width="14" height="20" rx="4" fill="${cor}" stroke="${strokeColor}" stroke-width="1.5"/>
                    <rect x="7" y="5" width="10" height="4" fill="rgba(0,0,0,0.7)" rx="1"/>
                    <rect x="7" y="15" width="10" height="4" fill="rgba(0,0,0,0.6)" rx="1"/>
                `;
                break;
        }

        let carSvg = `
            <div style="position: relative; width: 46px; height: 46px; display: flex; align-items: center; justify-content: center; ${isPossivelmenteOffline ? 'border: 3px solid #f39c12; border-radius: 50%; background: rgba(255,243,205,0.92);' : ''}">
                ${isPossivelmenteOffline ? '<span style="position:absolute; right:-4px; top:-4px; width:14px; height:14px; border-radius:50%; background:#d63031; border:2px solid #fff;"></span>' : ''}
                <div style="transform: rotate(${heading}deg); width: 36px; height: 36px; display: flex; align-items: center; justify-content: center; filter: drop-shadow(0px 4px 6px ${shadowColor}); opacity: ${opacityCar};">
                    <svg viewBox="0 0 24 24" width="36" height="36" xmlns="http://www.w3.org/2000/svg">
                        ${innerSvg}
                    </svg>
                </div>
            </div>
        `;

        let customIcon = L.divIcon({
            className: 'custom-car-icon',
            html: carSvg,
            iconSize: [46, 46],
            iconAnchor: [23, 23]
        });

        let marker = L.marker(rota.ultimaLocalizacao, { icon: customIcon }).addTo(map);

        // Paradas / Stops tracking
        let paradasListHtml = "";
        let proximaEncontrada = false;
        let paradasMarkers = [];

        if (rota.paradas && rota.paradas.length > 0) {
            paradasListHtml = '<div style="margin-top: 10px; max-height: 150px; overflow-y: auto; border-top: 1px solid #eee; padding-top: 5px;"><strong>Paradas:</strong><ul style="padding-left: 15px; margin-top: 5px; font-size: 0.85em; list-style-type: none; margin-left: 0; padding-inline-start: 5px;">';
            
            rota.paradas.forEach((p, idx) => {
                let statusText = "";
                let bgStyle = "";
                
                if (p.entregue) {
                    statusText = `<span style="color:#2ecc71;">✔ ${p.concluidoEm}</span>`;
                    bgStyle = `border: 2px solid #2ecc71; color: #fff; background: #2ecc71;`;
                } else if (!proximaEncontrada && !isFinalizada) {
                    statusText = `<span style="font-weight:bold; color:#f39c12;">⏳ Próxima</span>`;
                    proximaEncontrada = true;
                    // Proxima: Usa a cor da rota e destaca com pulso do CSS (se quisesse adicionar uma classe)
                    bgStyle = `border: 2px solid ${corViva}; color: ${corViva}; background: #fff; border-width: 3px;`;
                } else {
                    statusText = `<span style="color:#7f8c8d;">Pendente</span>`;
                    // Pendentes posteriores: usa a cor da rota mas mais "lavado" ou padrão
                    bgStyle = `border: 2px solid ${cor}; color: ${cor}; background: #fff;`;
                }

                let linkHtml = p.link ? ` <a href="${p.link}" target="_blank" title="Abrir no Maps"><i class="bx bx-link-external"></i></a>` : "";
                paradasListHtml += `<li style="margin-bottom: 6px; border-bottom: 1px dotted #ccc; padding-bottom: 3px;"><strong>P${idx+1}:</strong> ${p.nome}${linkHtml} <br/> <small>${statusText}</small></li>`;

                // Configura e Renderiza o marcador no mapa
                let markerCss = `${bgStyle} border-radius: 50%; width: 22px; height: 22px; text-align: center; line-height: 18px; font-weight: bold; font-size: 11px; box-shadow: 0 3px 6px rgba(0,0,0,0.3);`;
                
                let paradaIcon = L.divIcon({
                    className: 'custom-parada-container',
                    html: `<div style="${markerCss}">P${idx+1}</div>`,
                    iconSize: [26, 26],
                    iconAnchor: [13, 13]
                });

                let pMarker = L.marker([p.latitude, p.longitude], { icon: paradaIcon, zIndexOffset: -100 }).addTo(map);
                
                pMarker.bindPopup(`
                    <div style="min-width: 150px; font-size: 0.9em; padding: 4px;">
                        <h6 style="margin-bottom: 8px; border-bottom: 1px solid #ccc; padding-bottom: 5px; color: #2c3e50;"><strong>Parada: ${p.nome}</strong></h6>
                        <div style="margin-bottom: 6px;"><strong>Rota:</strong> <span style="color:#555;">${rota.descricao}</span></div>
                        <div style="font-size: 0.85em; margin-bottom: 6px; padding: 4px; background: #f8f9fa; border-radius: 4px; border: 1px solid #eee;">
                            <div><i class="bx bx-play-circle" style="color: #27ae60;"></i> Início: <strong>${rota.horaInicio || '--/--/--'}</strong></div>
                            <div><i class="bx bx-stop-circle" style="color: #c0392b;"></i> Fim: <strong>${rota.horaFim || '--/--/--'}</strong></div>
                        </div>
                        <div style="margin-top: 5px;">Status: ${statusText}</div>
                    </div>
                `, { closeButton: false });

                paradasMarkers.push(pMarker);
                group.addLayer(pMarker);
            });
            paradasListHtml += '</ul></div>';
        }

        let pausasListHtml = "";
        if (rota.pausas && rota.pausas.length > 0) {
            pausasListHtml = '<div style="margin-top: 10px; max-height: 100px; overflow-y: auto; border-top: 1px solid #eee; padding-top: 5px;"><strong>Pausas:</strong><ul style="padding-left: 15px; margin-top: 5px; font-size: 0.85em; list-style-type: none; margin-left: 0; padding-inline-start: 5px;">';
            rota.pausas.forEach((p, idx) => {
                let dInicio = new Date(p.dataHoraInicio).toLocaleTimeString('pt-BR', {hour: '2-digit', minute:'2-digit'});
                let dFim = p.dataHoraFim ? new Date(p.dataHoraFim).toLocaleTimeString('pt-BR', {hour: '2-digit', minute:'2-digit'}) : 'Em andamento';
                let st = p.dataHoraFim ? `<span style="color:#27ae60;">✔ ${dInicio} às ${dFim}</span>` : `<span style="color:#e67e22; font-weight:bold;">⏳ Pausado (${dInicio})</span>`;
                pausasListHtml += `<li style="margin-bottom: 6px; border-bottom: 1px dotted #ccc; padding-bottom: 3px;"><strong>Pausa ${idx+1}:</strong> ${p.motivo} <br/> <small>${st}</small></li>`;
            });
            pausasListHtml += '</ul></div>';
        }

        let tempoLabel = isFinalizada ? `<span style="color: #c0392b"><i class="bx bx-check-double"></i> Finalizada</span> às` : (isPausada ? `<span style="color: #e67e22; font-weight:bold;"><i class="bx bx-pause-circle"></i> Em Pausa</span> desde` : `<i class="bx bx-time"></i> Atual. às`);
        let pacienteHtml = rota.nomePaciente ? `<div><strong>Paciente:</strong> <span style="color: #2980b9;">${rota.nomePaciente}</span></div>` : "";
        let medicoHtml = rota.medicoResponsavel ? `<div><strong>Médico Resp.:</strong> ${rota.medicoResponsavel}</div>` : "";
        let dataExecucaoHtml = rota.dataParaExecucao ? `<div><strong>Data Agendada:</strong> ${rota.dataParaExecucao}</div>` : "";
        let velocidadeMediaHtml = rota.velocidadeMediaKmH !== null && rota.velocidadeMediaKmH !== undefined ? `<div><strong>Vel. media:</strong> ${Number(rota.velocidadeMediaKmH).toFixed(1).replace('.', ',')} km/h</div>` : "";
        let desvioHtml = rota.sujeitoADesvio === true ? `<div style="margin-top: 6px; padding: 4px; background: #ffeaa7; border-left: 3px solid #e17055; color: #d63031; font-weight: bold; font-size: 0.85em; border-radius:3px;"><i class="bx bx-error-circle"></i> ALERTA: Desvio detectado</div>` : "";
        let possivelmenteOfflineHtml = isPossivelmenteOffline
            ? `<div style="margin-top: 6px; padding: 4px; background: #fff3cd; border-left: 3px solid #f39c12; color: #7a4b00; font-weight: bold; font-size: 0.85em; border-radius:3px;"><i class="bx bx-wifi-off"></i> Possivelmente offline<br><small>Sem comunicacao ha ${rota.minutosSemComunicacao || 0} min. Ultima comunicacao: ${rota.ultimaComunicacaoApp || '--/--/--'}</small></div>`
            : "";
        let offlineHtml = rota.possuiRegistroOffline === true
            ? `<div style="margin-top: 6px; padding: 4px; background: ${rota.execucaoOfflineCompleta ? '#ffe5e5' : '#fff4d6'}; border-left: 3px solid #f39c12; color: #7a4b00; font-weight: bold; font-size: 0.85em; border-radius:3px;"><i class="bx bx-cloud-off"></i> ${rota.classificacaoOffline}</div>`
            : "";

        let popupContent = `
            <div class="popup-content" style="min-width: 200px;">
                <h6 class="mb-1 pb-1" style="border-bottom: 1px solid #eee; margin-top: 5px;"><strong>Rota:</strong> <br/>${rota.descricao}</h6>
                <div class="small" style="line-height:1.6;">
                    ${dataExecucaoHtml}
                    ${pacienteHtml}
                    ${medicoHtml}
                    <div style="${(rota.nomePaciente || rota.medicoResponsavel) ? "margin-top: 5px; border-top: 1px dashed #ddd; padding-top: 4px;" : ""}">
                        <div><strong>Motorista:</strong> ${rota.motoristaNome}</div>
                        <div><strong>Veículo:</strong> ${rota.placaModelo}</div>
                        ${velocidadeMediaHtml}
                    </div>
                    <div class="mt-2 text-muted" style="font-size: 0.85em;">
                        <div><i class="bx bx-play-circle" style="color: #27ae60;"></i> Inicio: <strong>${rota.horaInicio || '--/--/--'}</strong></div>
                        ${tempoLabel} ${rota.ultimaAtualizacao.substring(11, 19)}
                    </div>
                    ${desvioHtml}
                    ${possivelmenteOfflineHtml}
                    ${offlineHtml}
                </div>
                ${pausasListHtml}
                ${paradasListHtml}
            </div>`;

        marker.bindPopup(popupContent, {
            offset: [0, -5],
            autoPanPadding: [20, 20],
            closeButton: false
        });

        routeLayers[layerId] = {
            polyline: pathLine,
            marker: marker,
            inicio: inicioMarker,
            paradas: paradasMarkers,
            offlineSegments: offlineSegments
        };
        window.monitoramentoRotasRouteLayers = routeLayers;

        group.addLayer(marker);
        group.addLayer(pathLine);
    });

    // Auto fit bounds to show all markers if there is any data shown and user has filtered
    if (Object.keys(routeLayers).length > 0) {
        // If we are applying filter manually and want to adjust the camera, we could: 
        // map.fitBounds(group.getBounds(), { padding: [50, 50] });
    }
}

function montarSegmentosOfflineMonitoramento(pontos) {
    let segmentos = [];
    let atual = [];

    pontos.forEach(p => {
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

function toggleFiltros() {
    const overlay = document.getElementById('filtros-overlay');
    const icon = document.getElementById('toggle-icon');
    
    if (overlay.classList.contains('collapsed')) {
        overlay.classList.remove('collapsed');
        icon.classList.remove('bx-chevron-down');
        icon.classList.add('bx-chevron-up');
    } else {
        overlay.classList.add('collapsed');
        icon.classList.remove('bx-chevron-up');
        icon.classList.add('bx-chevron-down');
    }
}

$(document).ready(function() {
    setTimeout(function() {
        initMap();
    }, 200); // slight delay to ensure dom wrapper limits are ready
});
