let map;
let allRoutesData = [];
let routeLayers = {};

function initMap() {
    let lat = typeof mapLatCentro !== 'undefined' ? mapLatCentro : -15.7942;
    let lon = typeof mapLonCentro !== 'undefined' ? mapLonCentro : -47.8821;

    map = L.map('map').setView([lat, lon], 12); // Default to Configured Location
    
    // Using the standard OpenStreetMap tiles which are more vibrant and colorful
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    obterDadosMonitoramento();
    // Poll data every 10 seconds for real time map update
    setInterval(obterDadosMonitoramento, 10000);
}

function obterDadosMonitoramento() {
    $.getJSON('/Rota/ObterDadosMonitoramento', function(response) {
        if (response.sucesso) {
            allRoutesData = response.dados;
            
            // Centraliza o mapa se tivermos dados e ainda não tivermos desenhado as rotas
            if (allRoutesData.length > 0 && Object.keys(routeLayers).length === 0) {
                 map.setView(allRoutesData[0].ultimaLocalizacao, 13);
            }
            
            renderizarRotasNoMapa();
            
            $("#ultima-sincronizacao").text("Sincronizado às " + new Date().toLocaleTimeString('pt-BR'));
        } else {
            console.error("Erro ao buscar dados de monitoramento:", response.mensagem);
        }
    });
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

function renderizarRotasNoMapa() {
    let filtroMotorista = $("#M_FiltroMotorista").val();
    let filtroVeiculo = $("#M_FiltroVeiculo").val();
    let filtroRota = $("#M_FiltroRota").val();

    let dadosFiltrados = allRoutesData.filter(r => {
        if (filtroMotorista && r.motoristaId.toString() !== filtroMotorista) return false;
        if (filtroVeiculo && r.veiculoId.toString() !== filtroVeiculo) return false;
        if (filtroRota && r.rotaId.toString() !== filtroRota) return false;
        return true;
    });

    // Remove old polylines and markers correctly
    for (let id in routeLayers) {
        map.removeLayer(routeLayers[id].polyline);
        map.removeLayer(routeLayers[id].marker);
        if (routeLayers[id].paradas) {
            routeLayers[id].paradas.forEach(p => map.removeLayer(p));
        }
    }
    routeLayers = {};

    let group = new L.featureGroup();

    dadosFiltrados.forEach(rota => {
        let cor = getPredefinedColor(rota.rotaId);
        
        // Polyline drawing the path
        let pathLine = L.polyline(rota.historicoLocalizacoes, {
            color: cor,
            weight: 5,
            opacity: 0.8,
            smoothFactor: 1,
            className: 'animated-route'
        }).addTo(map);

        // Calcula o "rumo" (heading) para rotacionar o carro na direção correta
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

        // SVG Premium (Carro vista superior, dinâmico e sombreado)
        let carSvg = `
            <div style="transform: rotate(${heading}deg); width: 28px; height: 28px; display: flex; align-items: center; justify-content: center; filter: drop-shadow(0px 3px 5px rgba(0,0,0,0.5));">
                <svg viewBox="0 0 24 24" width="28" height="28" xmlns="http://www.w3.org/2000/svg">
                    <rect x="5" y="2" width="14" height="20" rx="4" fill="${cor}" stroke="white" stroke-width="1.5"/>
                    <rect x="7" y="5" width="10" height="4" fill="rgba(0,0,0,0.7)" rx="1"/>
                    <rect x="7" y="15" width="10" height="4" fill="rgba(0,0,0,0.6)" rx="1"/>
                    <rect x="4" y="8" width="2" height="4" rx="1" fill="${cor}" />
                    <rect x="18" y="8" width="2" height="4" rx="1" fill="${cor}" />
                </svg>
            </div>
        `;

        let customIcon = L.divIcon({
            className: 'custom-car-icon',
            html: carSvg,
            iconSize: [28, 28],
            iconAnchor: [14, 14]
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
                } else if (!proximaEncontrada) {
                    statusText = `<span style="font-weight:bold; color:#f39c12;">⏳ Próxima</span>`;
                    proximaEncontrada = true;
                    // Proxima: Usa a cor da rota e destaca com pulso do CSS (se quisesse adicionar uma classe)
                    bgStyle = `border: 2px solid ${cor}; color: ${cor}; background: #fff; border-width: 3px;`;
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
                        <h6 style="margin-bottom: 8px; border-bottom: 1px solid #ccc; padding-bottom: 5px;"><strong>${p.nome}</strong></h6>
                        <div>Status: ${statusText}</div>
                    </div>
                `, { closeButton: false });

                paradasMarkers.push(pMarker);
                group.addLayer(pMarker);
            });
            paradasListHtml += '</ul></div>';
        }

        let popupContent = `
            <div class="popup-content" style="min-width: 200px;">
                <h6 class="mb-2 pb-2" style="border-bottom: 1px solid #eee; margin-top: 5px;"><strong>Rota:</strong> <br/>${rota.descricao}</h6>
                <div class="small" style="line-height:1.6;">
                    <div><strong>Motorista:</strong> ${rota.motoristaNome}</div>
                    <div><strong>Veículo:</strong> ${rota.placaModelo}</div>
                    <div class="mt-2 text-muted" style="font-size: 0.85em;">
                        <i class="bx bx-time"></i> Atual. às ${rota.ultimaAtualizacao.substring(11, 19)}
                    </div>
                </div>
                ${paradasListHtml}
            </div>`;

        marker.bindPopup(popupContent, {
            offset: [0, -5],
            autoPanPadding: [20, 20],
            closeButton: false
        });

        routeLayers[rota.rotaId] = {
            polyline: pathLine,
            marker: marker,
            paradas: paradasMarkers
        };

        group.addLayer(marker);
        group.addLayer(pathLine);
    });

    // Auto fit bounds to show all markers if there is any data shown and user has filtered
    if (Object.keys(routeLayers).length > 0) {
        // If we are applying filter manually and want to adjust the camera, we could: 
        // map.fitBounds(group.getBounds(), { padding: [50, 50] });
    }
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
