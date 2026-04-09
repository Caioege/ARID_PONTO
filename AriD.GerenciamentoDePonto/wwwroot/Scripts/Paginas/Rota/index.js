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

function assineEventosCadastro() {
    $('#btn-adicionar-parada').on('click', function () {
        let endereco = $('#novaParadaEndereco').val();
        let lat = $('#novaParadaLat').val();
        let lng = $('#novaParadaLng').val();
        let obs = $('#novaParadaObs').val();

        if (!endereco) {
            MensagemRodape('warning', 'Informe o endereço da parada!');
            return;
        }

        let idTemporario = 0; 
        $('#empty-paradas-tr').remove();
        let count = $('#tabela-paradas tbody tr').length + 1;
        
        let geoCoordHtml = (lat && lng) ? `<code class="small text-muted">${lat}, ${lng}</code>` : `<span class="text-warning small"><i class="bx bx-error"></i> Geofence Offline</span>`;

        let novaLinha = `
            <tr data-id="${idTemporario}" data-entregue="false" data-endereco="${endereco}" data-lat="${lat}" data-lng="${lng}" data-obs="${obs}" data-link="">
                <td class="text-center font-weight-bold"><span class="badge bg-label-primary rounded-circle p-2">${count}</span></td>
                <td>${endereco}</td>
                <td>${geoCoordHtml}</td>
                <td>${obs}</td>
                <td><span class="badge bg-secondary">Pendente</span></td>
                <td>
                    <button type="button" class="btn btn-sm btn-icon btn-danger btn-remover-parada" onclick="removerLinhaParada(this)"><i class='bx bx-trash'></i></button>
                </td>
            </tr>
        `;

        $('#tabela-paradas tbody').append(novaLinha);
        $('#novaParadaEndereco, #novaParadaLat, #novaParadaLng, #novaParadaObs').val('');
    });

    $('#btn-salvar-cadastro').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-rota');
        
        let motoristaId = $('#MotoristaId').val();
        let descricao = $('#Descricao').val();
         
        if (!motoristaId || !descricao) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos (Motorista, Descrição)!');
            return;
        }

        let payload = dadosFormulario.dados;

        // Paradas
        $('#tabela-paradas tbody tr:not(#empty-paradas-tr)').each(function(index) {
            let row = $(this);
            payload[`paradas[${index}].Id`] = row.attr('data-id');
            payload[`paradas[${index}].Endereco`] = row.attr('data-endereco');
            payload[`paradas[${index}].Latitude`] = row.attr('data-lat');
            payload[`paradas[${index}].Longitude`] = row.attr('data-lng');
            payload[`paradas[${index}].Link`] = row.attr('data-link');
            payload[`paradas[${index}].Observacao`] = row.attr('data-obs');
            payload[`paradas[${index}].Entregue`] = row.attr('data-entregue');
        });

        // Pacientes
        let pacientesList = [];
        $('#tabela-pacientes-selecionados tbody tr').each(function() {
            pacientesList.push({
                PacienteId: $(this).attr('data-id'),
                PossuiAcompanhante: $(this).attr('data-acompanhante') === 'true'
            });
        });
        payload['pacientesJson'] = JSON.stringify(pacientesList);

        // Profissionais
        let profissionaisList = [];
        $('#tabela-profissionais-selecionados tbody tr').each(function() {
            profissionaisList.push({
                ServidorId: $(this).attr('data-id')
            });
        });
        payload['profissionaisJson'] = JSON.stringify(profissionaisList);

        RequisicaoAjaxComCarregamento(
            '/Rota/Salvar/',
            'POST',
            payload,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Rota/Index');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            }
        );
    });

    $('#btn-preview-rota').on('click', AcionarPreviewDeRota);
}

function adicionarParadaManualmente() {
    let endereco = $('#novaParadaEndereco').val();
    let lat = $('#novaParadaLat').val();
    let lng = $('#novaParadaLng').val();
    let obs = $('#novaParadaObs').val();

    if (!endereco) {
        MensagemRodape('warning', 'Informe o endereço da parada!');
        return;
    }

    let idTemporario = 0; 
    $('#empty-paradas-tr').remove();
    let count = $('#tabela-paradas tbody tr').length + 1;
    
    let geoCoordHtml = (lat && lng) ? `<code class="small text-muted">${lat}, ${lng}</code>` : `<span class="text-warning small"><i class="bx bx-error"></i> Geofence Offline</span>`;

    let novaLinha = `
        <tr data-id="${idTemporario}" data-entregue="false" data-endereco="${endereco}" data-lat="${lat}" data-lng="${lng}" data-obs="${obs}" data-link="">
            <td class="text-center font-weight-bold"><span class="badge bg-label-primary rounded-circle p-2">${count}</span></td>
            <td>${endereco}</td>
            <td>${geoCoordHtml}</td>
            <td>${obs}</td>
            <td><span class="badge bg-secondary">Pendente</span></td>
            <td>
                <button type="button" class="btn btn-sm btn-icon btn-danger btn-remover-parada" onclick="removerLinhaParada(this)"><i class='bx bx-trash'></i></button>
            </td>
        </tr>
    `;

    $('#tabela-paradas tbody').append(novaLinha);
    $('#novaParadaEndereco, #novaParadaLat, #novaParadaLng, #novaParadaObs').val('');
}

// Inicia automaticamente o assineEventos se estiver na página Cadastro
$(document).ready(function() {
    if($('#btn-salvar-cadastro').length > 0) {
        assineEventosCadastro();
    }
});

function adicionarPacienteLista() {
    let $select = $('#selecao-paciente');
    let id = $select.val();
    let nome = $select.find(':selected').data('nome');
    let acomp = $('#paciente-possui-acompanhante').is(':checked');

    if (!id) return;
    
    // Evitar duplicados
    if ($(`#tabela-pacientes-selecionados tbody tr[data-id="${id}"]`).length > 0) {
        MensagemRodape('warning', 'Paciente já adicionado.');
        return;
    }

    let linha = `<tr data-id="${id}" data-acompanhante="${acomp}">
        <td>${nome}</td>
        <td class="text-center">${acomp ? 'Sim' : 'Não'}</td>
        <td class="text-center">
            <button type="button" class="btn btn-xs btn-icon btn-outline-danger" onclick="removerLinha(this)"><i class='bx bx-trash'></i></button>
        </td>
    </tr>`;

    $('#tabela-pacientes-selecionados tbody').append(linha);
    $select.val('').trigger('change');
    $('#paciente-possui-acompanhante').prop('checked', false);
}

function salvarPacienteNovo() {
    let nome = $('#novo-paciente-nome').val();
    let cpf = $('#novo-paciente-cpf').val();
    let tel = $('#novo-paciente-telefone').val();

    if (!nome) {
        MensagemRodape('warning', 'Informe pelo menos o nome do paciente!');
        return;
    }

    RequisicaoAjaxComCarregamento('/Paciente/Salvar', 'POST', { Nome: nome, CPF: cpf, Telefone: tel, Ativo: true }, function(res) {
        if (res.sucesso) {
            MensagemRodape('success', 'Paciente cadastrado com sucesso!');
            
            // Adicionar ao Select2 e selecionar
            let novaOpcao = new Option(`${nome} (${cpf || 'S/ CPF'})`, res.id, true, true);
            $(novaOpcao).data('nome', nome);
            $('#selecao-paciente').append(novaOpcao).trigger('change');
            
            // Limpar e fechar área
            $('#novo-paciente-nome, #novo-paciente-cpf, #novo-paciente-telefone').val('');
            $('#area-novo-paciente').slideUp();
            
            // Adicionar automaticamente à lista da rota
            adicionarPacienteLista();
        } else {
            MensagemRodape('warning', res.mensagem);
        }
    });
}

function adicionarProfissionalLista() {
    let $select = $('#selecao-profissional');
    let id = $select.val();
    let option = $select.find(':selected');
    let nome = option.data('nome');
    let cargo = option.data('cargo');
    let crm = option.data('crm');
    let especialidade = option.data('especialidade');

    if (!id) return;
    
    // Evitar duplicados
    if ($(`#tabela-profissionais-selecionados tbody tr[data-id="${id}"]`).length > 0) {
        MensagemRodape('warning', 'Profissional já adicionado.');
        return;
    }

    let crmHtml = crm ? `<span class="badge bg-label-info">CRM: ${crm}</span><br/><small>${especialidade || ''}</small>` : '<span>-</span>';

    let linha = `<tr data-id="${id}" data-crm="${crm}" data-especialidade="${especialidade}">
        <td>${nome}</td>
        <td>${cargo || '-'}</td>
        <td>${crmHtml}</td>
        <td class="text-center">
            <button type="button" class="btn btn-xs btn-icon btn-outline-danger" onclick="removerLinha(this)"><i class='bx bx-trash'></i></button>
        </td>
    </tr>`;

    $('#tabela-profissionais-selecionados tbody').append(linha);
    $select.val('').trigger('change');
}

function aoMudarProfissional() {
    let $select = $('#selecao-profissional');
    let option = $select.find(':selected');
    let crm = option.data('crm');
    let esp = option.data('especialidade');

    if (crm) {
        $('#label-crm').text(crm);
        $('#label-especialidade').text(esp || '-');
        $('#info-medico-selecionado').fadeIn();
    } else {
        $('#info-medico-selecionado').hide();
    }
}

function removerLinha(button) {
    $(button).closest('tr').remove();
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

let previewMap = null;
let previewMapLayer = null;

function AcionarPreviewDeRota() {
    let motoristaId = $('#MotoristaId').val();
    let unidadeDestinoId = $('#UnidadeDestinoId').val();
    let paradasCount = $('#tabela-paradas tbody tr:not(#empty-paradas-tr)').length;

    if (!unidadeDestinoId && paradasCount === 0) {
        MensagemRodape('warning', 'Adicione paradas ou selecione uma Unidade de Destino para visualizar o traçado.');
        return;
    }

    let dadosFormulario = ObtenhaFormularioSerializado('formulario-rota');
    let payload = { Rota: dadosFormulario.dados, Paradas: [] };
    
    $('#tabela-paradas tbody tr:not(#empty-paradas-tr)').each(function() {
        let row = $(this);
        payload.Paradas.push({
            Id: row.attr('data-id'),
            Endereco: row.attr('data-endereco'),
            Latitude: row.attr('data-lat'),
            Longitude: row.attr('data-lng'),
            Observacao: row.attr('data-obs')
        });
    });

    RequisicaoAjaxComCarregamento('/Rota/PreviaDeRotaGeoAsync', 'POST', payload, function(res) {
        if (res.sucesso && res.polyline) {
            $('#preview-map-container').slideDown('fast', function() {
                if (!previewMap) {
                    previewMap = L.map('preview-map-container').setView([-15.7942, -47.8821], 12);
                    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(previewMap);
                }
                
                if (previewMapLayer) previewMap.removeLayer(previewMapLayer);
                
                // Polyline decoding is usually done via a Leaflet plugin or manually. 
                // However, since we return raw OSRM string, we can rely on PolylineUtil if available, or just request it as GeoJSON coordinates.
                // Assuming polyline decoding from frontend or using basic string. We can decode it using L.Polyline.fromEncoded if the library is present.
                // For safety, let's just show a simulated bounds or load if the encoded library isn't there:
                try {
                    previewMapLayer = L.Polyline.fromEncoded(res.polyline, { color: '#e74c3c', weight: 4 }).addTo(previewMap);
                    previewMap.fitBounds(previewMapLayer.getBounds(), { padding: [20, 20] });
                } catch(e) {
                    console.log("Polyline.fromEncoded ausente. Ignorando renderização detalhada.");
                    MensagemRodape('warning', 'O trajeto foi calculado com sucesso via OSRM, porém a biblioteca Polyline client-side está ausente.');
                }
            });
            MensagemRodape('success', 'Traçado OSRM calculado! Verifique o preview.');
        } else {
            MensagemRodape('warning', res.mensagem || 'Falha ao prever rota geométrica. Verifique as Coordenadas Inseridas.');
        }
    });
}

function removerRegistroCadastro() {
    Swal.fire({
        title: 'Você tem certeza?',
        text: "Deseja realmente remover esta rota e todas suas paradas associadas?",
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
                { rotaId: $('#Id').val() },
                function (data) {
                    if (data.sucesso) {
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
