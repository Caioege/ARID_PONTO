function assineEventosCadastro() {
    $('#btn-adicionar-parada').on('click', function () {
        let endereco = $('#novaParadaEndereco').val();
        let lat = $('#novaParadaLat').val();
        let lng = $('#novaParadaLng').val();

        if (!endereco) {
            MensagemRodape('warning', 'Informe o endereço da parada!');
            return;
        }

        let idTemporario = 0;
        $('#empty-paradas-tr').remove();
        let count = $('#tabela-paradas tbody tr').length + 1;

        let geoCoordHtml = (lat && lng) ? `<a href="https://www.google.com/maps/search/?api=1&query=${lat},${lng}" target="_blank" title="Abrir no Google Maps"><code class="small text-muted">${lat}, ${lng}</code></a>` : `<span class="text-warning small"><i class="bx bx-error"></i> Geofence Offline</span>`;

        let novaLinha = `
            <tr data-id="${idTemporario}" data-endereco="${endereco}" data-lat="${lat}" data-lng="${lng}" data-link="" data-observacao-cadastro="">
                <td class="text-center font-weight-bold"><span class="badge bg-label-primary rounded-circle p-2">${count}</span></td>
                <td>${endereco}</td>
                <td>${geoCoordHtml}</td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-icon btn-danger btn-remover-parada" onclick="removerLinhaParada(this)"><i class='bx bx-trash'></i></button>
                </td>
            </tr>
        `;

        $('#tabela-paradas tbody').append(novaLinha);
        $('#novaParadaEndereco, #novaParadaLat, #novaParadaLng').val('');
    });

    $('#btn-salvar-cadastro').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-rota');

        let motoristaId = $('#MotoristaId').val();
        let motoristaSecundarioId = $('#MotoristaSecundarioId').val();
        let descricao = $('#Descricao').val();
        let veiculos = $('#veiculosSelecionados').val();

        if (!motoristaId || !descricao || !veiculos || veiculos.length === 0) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos (Motorista Principal, Descrição, Veículos Associados)!');
            return;
        }

        let payload = dadosFormulario.dados;

        payload['MotoristaId'] = motoristaId;
        payload['MotoristaSecundarioId'] = motoristaSecundarioId;
        payload['Descricao'] = descricao;
        payload['UnidadeOrigemId'] = $('#UnidadeOrigemId').val();
        payload['UnidadeDestinoId'] = $('#UnidadeDestinoId').val();

        // Paradas
        $('#tabela-paradas tbody tr:not(#empty-paradas-tr)').each(function (index) {
            let row = $(this);
            payload[`paradas[${index}].Id`] = row.attr('data-id');
            payload[`paradas[${index}].Endereco`] = row.attr('data-endereco');
            payload[`paradas[${index}].Latitude`] = row.attr('data-lat');
            payload[`paradas[${index}].Longitude`] = row.attr('data-lng');
            payload[`paradas[${index}].Link`] = row.attr('data-link');
            payload[`paradas[${index}].ObservacaoCadastro`] = row.attr('data-observacao-cadastro');
            payload[`paradas[${index}].UnidadeId`] = row.attr('data-unidade-id');
            payload[`paradas[${index}].Ordem`] = index;
        });

        // Pacientes
        let pacientesList = [];
        $('#tabela-pacientes-selecionados tbody tr').each(function () {
            pacientesList.push({
                PacienteId: $(this).attr('data-id'),
                PossuiAcompanhante: $(this).attr('data-acompanhante') === 'true'
            });
        });
        payload['pacientesJson'] = JSON.stringify(pacientesList);

        // Profissionais
        let profissionaisList = [];
        $('#tabela-profissionais-selecionados tbody tr').each(function () {
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

    $('#PermitePausa').on('change', function () {
        if ($(this).is(':checked')) {
            $('#box-quantidade-pausas').show();
        } else {
            $('#box-quantidade-pausas').hide();
            $('#QuantidadePausas').val('0');
        }
    });

    $('#btn-preview-rota').on('click', AcionarPreviewDeRota);
}

// Modal de Pontos logic
var modalMap = null;
var modalMarker = null;

function abrirModalPonto(button) {
    $('#modal-ponto-index').val('');
    $('#modal-ponto-id').val('0');
    $('#modal-ponto-endereco, #modal-ponto-lat, #modal-ponto-lng, #modal-ponto-observacao').val('');

    if (button) {
        $('#row-tipo-pesquisa').hide();
        let row = $(button).closest('tr');
        $('#modal-ponto-index').val(row.index());
        $('#modal-ponto-id').val(row.attr('data-id'));
        $('#modal-ponto-endereco').val(row.attr('data-endereco'));
        $('#modal-ponto-lat').val(row.attr('data-lat'));
        $('#modal-ponto-lng').val(row.attr('data-lng'));
        $('#modal-ponto-observacao').val(row.attr('data-observacao-cadastro') || '');

        let unidadeId = row.attr('data-unidade-id');
        let tipoPesquisa = row.attr('data-tipo-pesquisa');

        if (unidadeId && unidadeId !== "undefined" && unidadeId !== "") {
            $(`input[name="tipo-pesquisa"][value="${tipoPesquisa}"]`).prop('checked', true).trigger('change');
            $('#modal-selecao-unidade').val(unidadeId).trigger('change');
        } else {
            $('#tipo-manual').prop('checked', true).trigger('change');
        }
    } else {
        $('#row-tipo-pesquisa').show();
        $('#tipo-unidade-rota').prop('checked', true).trigger('change');
    }

    $('#modal-ponto').modal('show');

    // Initialize map in modal after it's shown
    setTimeout(function () {
        if (!modalMap) {
            modalMap = L.map('modal-map').setView([-15.7942, -47.8821], 13);
            L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(modalMap);

            modalMap.on('click', function (e) {
                $('#modal-ponto-lat').val(e.latlng.lat.toFixed(6));
                $('#modal-ponto-lng').val(e.latlng.lng.toFixed(6));
                if (typeof atualizarLinkEDadosPonto === 'function') atualizarLinkEDadosPonto(e.latlng.lat, e.latlng.lng);
                atualizarMarkerModal(e.latlng.lat, e.latlng.lng);
            });
        }

        let lat = $('#modal-ponto-lat').val();
        let lng = $('#modal-ponto-lng').val();
        if (lat && lng) {
            atualizarMarkerModal(lat, lng);
            modalMap.setView([lat, lng], 15);
        } else {
            modalMap.setView([-15.7942, -47.8821], 13);
            if (modalMarker) modalMap.removeLayer(modalMarker);
        }
        modalMap.invalidateSize();
    }, 400);
}

function atualizarMarkerModal(lat, lng) {
    if (modalMarker) modalMap.removeLayer(modalMarker);
    modalMarker = L.marker([lat, lng]).addTo(modalMap);
}

function atualizarMapaModal(lat, lng) {
    if (modalMap) {
        modalMap.setView([lat, lng], 15);
        atualizarMarkerModal(lat, lng);
    }
}

function geocodificarEndereco(endereco, callbackSucesso) {
    if (!endereco) return;

    let q = endereco.includes(', Brasil') ? endereco : `${endereco}, Brasil`;
    fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(q)}`, {
        headers: { 'User-Agent': 'AriD-GestaoFrota/1.0' }
    })
        .then(response => response.json())
        .then(geo => {
            if (geo && geo.length > 0) {
                let lat = parseFloat(geo[0].lat).toFixed(6);
                let lon = parseFloat(geo[0].lon).toFixed(6);
                $('#modal-ponto-lat').val(lat);
                $('#modal-ponto-lng').val(lon);
                atualizarMapaModal(lat, lon);
                if (callbackSucesso) callbackSucesso(lat, lon);
            }
        })
        .catch(err => console.error("Erro na geocodificação:", err));
}

function pesquisarCEPModal() {
    let cep = $('#modal-cep').val();
    if (!cep) return;

    RequisicaoAjaxComCarregamento('/Cep/ConsulteCEP', 'GET', { cep: cep }, function (res) {
        if (res.sucesso && res.dados) {
            let end = `${res.dados.logradouro}, ${res.dados.bairro}, ${res.dados.localidade}-${res.dados.uf}`;
            $('#modal-ponto-endereco').val(end);

            geocodificarEndereco(end, function () {
                MensagemRodape('success', 'Endereço e Coordenadas aproximadas carregados!');
            });
        }
    });
}

function confirmarPontoModal() {
    const validacao = ObtenhaFormularioSerializado('form-ponto-parada');
    if (!validacao.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios não preenchidos!');
        return;
    }

    const { 
        'modal-ponto-id': id = '0', 
        'modal-ponto-index': index, 
        'modal-ponto-endereco': endereco, 
        'modal-ponto-lat': lat, 
        'modal-ponto-lng': lng,
        'modal-ponto-observacao': observacaoCadastro = ''
    } = validacao.dados;

    let unidadeId = '';
    let tipoPesquisa = $('input[name="tipo-pesquisa"]:checked').val();

    if (tipoPesquisa && tipoPesquisa.startsWith('tipo-')) {
        unidadeId = $('#modal-selecao-unidade').val();
    }

    let geoCoordHtml = (lat && lng) ? `<a href="https://www.google.com/maps/search/?api=1&query=${lat},${lng}" target="_blank" title="Abrir no Google Maps"><code class="small text-muted">${lat}, ${lng}</code></a>` : `<span class="text-warning small"><i class="bx bx-error"></i> Geofence Offline</span>`;

    let html = `
        <td class="text-center cursor-move"><i class="bx bx-menu"></i></td>
        <td class="text-center font-weight-bold"><span class="badge bg-label-primary rounded-circle p-2 label-ordem">0</span></td>
        <td>${endereco}${observacaoCadastro ? `<br /><small class="text-muted"><i class="bx bx-note"></i> ${observacaoCadastro}</small>` : ''}</td>
        <td>${geoCoordHtml}</td>
        <td class="text-center">
            <button type="button" class="btn btn-sm btn-icon btn-outline-primary me-1" onclick="abrirModalPonto(this)"><i class='bx bx-edit'></i></button>
            <button type="button" class="btn btn-sm btn-icon btn-outline-danger" onclick="removerLinhaParada(this)"><i class='bx bx-trash'></i></button>
        </td>
    `;

    if (index !== '') {
        $(`#sortable-paradas tr:eq(${index})`)
            .attr('data-endereco', endereco)
            .attr('data-lat', lat)
            .attr('data-lng', lng)
            .attr('data-observacao-cadastro', observacaoCadastro)
            .attr('data-unidade-id', unidadeId)
            .attr('data-tipo-pesquisa', tipoPesquisa)
            .html(html);
    } else {
        $('#empty-paradas-tr').remove();
        let novaLinha = `<tr data-id="0" data-endereco="${endereco}" data-lat="${lat}" data-lng="${lng}" data-link="" data-observacao-cadastro="${observacaoCadastro}" data-unidade-id="${unidadeId}" data-tipo-pesquisa="${tipoPesquisa}">${html}</tr>`;
        $('#sortable-paradas').append(novaLinha);
    }

    recalcularOrdens();
    $('#modal-ponto').modal('hide');
}

function recalcularOrdens() {
    $('#sortable-paradas tr:not(#empty-paradas-tr)').each(function (i) {
        $(this).find('.label-ordem').text(i + 1);
    });
}

function adicionarPacienteLista() {
    let $select = $('#selecao-paciente');
    let id = $select.val();
    let option = $select.find(':selected');
    let nome = option.data('nome');
    let tel = option.data('tel') || '';
    let cpf = option.data('cpf') || '';
    let acomp = $('#paciente-possui-acompanhante').is(':checked');

    if (!id) return;

    if ($(`#tabela-pacientes-selecionados tbody tr[data-id="${id}"]`).length > 0) {
        MensagemRodape('warning', 'Essa pessoa já foi adicionada nessa rota.');
        return;
    }

    adicionarPacienteAListaInterna(id, nome, tel, cpf, acomp);

    $select.val('').trigger('change');
    $('#paciente-possui-acompanhante').prop('checked', false);
}

function adicionarPacienteAListaInterna(id, nome, tel, cpf, ehAcompanhante) {
    let badge = ehAcompanhante 
        ? '<span class="badge bg-label-secondary">Acompanhante</span>' 
        : '<span class="badge bg-label-info">Paciente</span>';

    let linha = `<tr data-id="${id}" data-acompanhante="${ehAcompanhante}">
        <td>${nome}</td>
        <td>${cpf}</td>
        <td>${tel}</td>
        <td class="text-center">${badge}</td>
        <td class="text-center">
            <button type="button" class="btn btn-xs btn-icon btn-outline-danger" onclick="removerLinha(this)"><i class='bx bx-trash'></i></button>
        </td>
    </tr>`;

    $('#tabela-pacientes-selecionados tbody').append(linha);
}

function abrirModalNovoPaciente() {
    $('#novo-paciente-nome, #novo-paciente-cpf, #novo-paciente-datanasc, #novo-paciente-telefone').val('');
    $('#modal-novo-paciente').modal('show');
    assineMascarasDoComponente($('#modal-novo-paciente'));
}

function salvarPacienteNovo() {
    const validacao = ObtenhaFormularioSerializado('form-novo-paciente');
    if (!validacao.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios não preenchidos!');
        return;
    }

    const {
        'novo-paciente-nome': nome,
        'novo-paciente-cpf': cpf,
        'novo-paciente-datanasc': dataNasc,
        'novo-paciente-telefone': tel
    } = validacao.dados;

    if (!cpfValido(cpf)) {
        document.getElementById('novo-paciente-cpf').classList.add('campo-invalido');
        MensagemRodape('warning', 'Informe um CPF válido!');
        return;
    }

    RequisicaoAjaxComCarregamento('/Paciente/Salvar', 'POST', {
        Nome: nome,
        CPF: cpf,
        DataNascimento: dataNasc,
        Telefone: tel,
        Ativo: true
    }, function (res) {
        if (res.sucesso) {
            $('#modal-novo-paciente').modal('hide');
            MensagemRodape('success', 'Cadastro realizado com sucesso!');

            // Adicionar ao Select2 (cache local para futuras seleções)
            let novaOpcao = new Option(`${nome} (${cpf || 'S/ CPF'}) - ${tel || ''}`, res.id, false, false);
            $(novaOpcao).data('nome', nome);
            $(novaOpcao).data('tel', tel);
            $(novaOpcao).data('cpf', cpf);
            $('#selecao-paciente').append(novaOpcao).trigger('change');

            // Perguntar se é acompanhante para adicionar direto na rota
            Swal.fire({
                title: 'Paciente ou Acompanhante?',
                text: `Deseja adicionar ${nome} à rota como ACOMPANHANTE?`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#5a6268',
                confirmButtonText: 'Sim, Acompanhante',
                cancelButtonText: 'Não, apenas Paciente'
            }).then((result) => {
                let ehAcompanhante = result.isConfirmed;
                adicionarPacienteAListaInterna(res.id, nome, tel, cpf, ehAcompanhante);
            });

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
    recalcularOrdens();
    if ($('#sortable-paradas tr').length === 0) {
        $('#sortable-paradas').append('<tr id="empty-paradas-tr"><td colspan="5" class="text-center text-muted">Ainda não há paradas registradas na Rota.</td></tr>');
    }
}

function carregarHistoricoExecucoes() {
    let id = $('#Id').val();
    $.ajax({
        url: '/Rota/ObtenhaHistoricoExecucoes',
        type: 'GET',
        data: { rotaId: id },
        success: function (data) {
            if (data.sucesso && data.historico) {
                let tbody = $('#tabela-historico-execucoes tbody');
                tbody.empty();
                if (data.historico.length === 0) {
                    tbody.append('<tr><td colspan="5" class="text-center">Nenhuma execução registrada.</td></tr>');
                    return;
                }
                data.historico.forEach(function (h) {
                    let botaoMapa = h.dataHoraFim ? `<button type="button" class="btn btn-sm btn-icon btn-primary" title="Ver Rota" onclick="visualizarRotaNoMapa(${h.id})"><i class='bx bx-map-alt'></i></button>` : '';
                    let auditoria = montarBadgeOfflineHistorico(h);
                    tbody.append(`<tr><td>${h.dataHoraInicio}</td><td>${h.usuarioInicio}</td><td>${h.dataHoraFim || '-'}</td><td>${h.usuarioFim || '-'}</td><td>${auditoria}</td><td>${botaoMapa}</td></tr>`);
                });
            }
        }
    });
}

var historicoMap = null;
var historicoLayers = [];
var historicoAtual = null;

function montarBadgeOfflineHistorico(h) {
    if (!h.possuiRegistroOffline) return '<span class="text-muted">Online</span>';
    let classe = h.execucaoOfflineCompleta ? 'bg-label-danger' : 'bg-label-warning';
    let detalhe = h.dataHoraPrimeiroRegistroOffline && h.dataHoraUltimoRegistroOffline
        ? ` title="${h.dataHoraPrimeiroRegistroOffline} - ${h.dataHoraUltimoRegistroOffline}"`
        : '';
    return `<span class="badge ${classe}"${detalhe}>${h.classificacaoOffline}</span>`;
}

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
                        historicoAtual = rota;
                        $('#filtro-historico-offline').val('todos');
                        renderizarHistoricoAtual();
                        return;
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
                            let endIcon = L.icon({ iconUrl: svgUrl, iconSize: [28, 28], iconAnchor: [14, 14] });
                            let markerFinal = L.marker(rota.ultimaLocalizacao, { icon: endIcon }).addTo(historicoMap);
                            historicoLayers.push(markerFinal);
                        }

                        // Paradas
                        if (rota.paradas) {
                            rota.paradas.forEach((p, idx) => {
                                let paradaCss = `border: 2px solid #ccc; background: ${p.entregue ? '#2ecc71' : '#e74c3c'}; color: #fff; border-radius: 50%; width: 22px; height: 22px; text-align: center; line-height: 18px; font-weight: bold; font-size: 11px;`;
                                let paradaIcon = L.divIcon({ className: '', html: `<div style="${paradaCss}">P${idx + 1}</div>`, iconSize: [26, 26], iconAnchor: [13, 13] });
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

function renderizarHistoricoAtual() {
    if (!historicoMap || !historicoAtual) return;

    historicoLayers.forEach(l => historicoMap.removeLayer(l));
    historicoLayers = [];

    let rota = historicoAtual;
    let filtro = $('#filtro-historico-offline').val() || 'todos';
    let pontosDetalhados = rota.historicoLocalizacoesDetalhado || [];
    let pontosFiltrados = pontosDetalhados.filter(p => {
        if (filtro === 'offline') return p.registradoOffline === true;
        if (filtro === 'online') return p.registradoOffline !== true;
        return true;
    });
    let pontos = pontosFiltrados.map(p => [p.latitude, p.longitude]);

    let hexBase = '#34495e';
    let offlineColor = '#f39c12';
    let pathLine = null;

    $('#historico-offline-badge').html(montarResumoOfflineExecucao(rota));
    $('#historico-offline-detalhes').html(montarDetalhesAuditoriaOffline(rota, filtro));

    if (pontos.length > 1) {
        pathLine = L.polyline(pontos, {
            color: filtro === 'offline' ? offlineColor : hexBase,
            weight: 5,
            opacity: 0.82,
            dashArray: filtro === 'offline' ? '8, 8' : null
        }).addTo(historicoMap);
        historicoLayers.push(pathLine);
    }

    if (filtro === 'todos') {
        let segmentosOffline = montarSegmentosPorTipo(pontosDetalhados, true);
        segmentosOffline.forEach(seg => {
            if (seg.length < 2) return;
            let layer = L.polyline(seg, {
                color: offlineColor,
                weight: 6,
                opacity: 0.9,
                dashArray: '8, 8'
            }).addTo(historicoMap);
            historicoLayers.push(layer);
        });
    }

    pontosFiltrados.forEach(p => {
        if (p.registradoOffline !== true) return;
        let marker = L.circleMarker([p.latitude, p.longitude], {
            radius: 5,
            color: '#fff',
            weight: 2,
            fillColor: offlineColor,
            fillOpacity: 1
        }).addTo(historicoMap);
        marker.bindPopup(montarPopupLocalizacaoOffline(p), { closeButton: false });
        historicoLayers.push(marker);
    });

    let ultimaLocalizacao = pontos.length > 0 ? pontos[pontos.length - 1] : rota.ultimaLocalizacao;
    if (ultimaLocalizacao) {
        let svgUrl = "data:image/svg+xml;utf8," + encodeURIComponent(`
            <svg viewBox="0 0 24 24" width="28" height="28" xmlns="http://www.w3.org/2000/svg">
                <circle cx="12" cy="12" r="10" fill="${hexBase}" stroke="#fff" stroke-width="2"/>
                <text x="12" y="15" fill="#fff" font-size="10" font-family="Arial" font-weight="bold" text-anchor="middle">F</text>
            </svg>
        `);
        let endIcon = L.icon({ iconUrl: svgUrl, iconSize: [28, 28], iconAnchor: [14, 14] });
        let markerFinal = L.marker(ultimaLocalizacao, { icon: endIcon }).addTo(historicoMap);
        historicoLayers.push(markerFinal);
    }

    if (rota.paradas) {
        rota.paradas
            .filter(p => filtro === 'todos' || (filtro === 'offline' ? p.registradoOffline === true : p.registradoOffline !== true))
            .forEach((p, idx) => {
                let paradaColor = p.registradoOffline ? offlineColor : (p.entregue ? '#2ecc71' : '#e74c3c');
                let paradaCss = `border: 2px solid #ccc; background: ${paradaColor}; color: #fff; border-radius: 50%; width: 22px; height: 22px; text-align: center; line-height: 18px; font-weight: bold; font-size: 11px;`;
                let paradaIcon = L.divIcon({ className: '', html: `<div style="${paradaCss}">P${idx + 1}</div>`, iconSize: [26, 26], iconAnchor: [13, 13] });
                let pMarker = L.marker([p.latitude, p.longitude], { icon: paradaIcon }).addTo(historicoMap);
                let offlineInfo = p.registradoOffline ? `<br/><small><strong>Offline:</strong> ${p.dataHoraRegistroLocal || '-'}</small>` : '';
                pMarker.bindPopup(`<h6>${p.nome}</h6>${p.entregue ? 'Entregue' : 'Pend.'}${offlineInfo}`);
                historicoLayers.push(pMarker);
            });
    }

    if (pathLine) {
        historicoMap.fitBounds(pathLine.getBounds(), { padding: [20, 20] });
    } else if (pontos.length === 1) {
        historicoMap.setView(pontos[0], 15);
    }
}

function montarSegmentosPorTipo(pontos, offline) {
    let segmentos = [];
    let atual = [];

    pontos.forEach(p => {
        if ((p.registradoOffline === true) === offline) {
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

function montarResumoOfflineExecucao(rota) {
    if (!rota.possuiRegistroOffline) {
        return '<span class="badge bg-label-success">Rota executada online</span>';
    }

    let classe = rota.execucaoOfflineCompleta ? 'bg-label-danger' : 'bg-label-warning';
    let datas = rota.dataHoraPrimeiroRegistroOffline && rota.dataHoraUltimoRegistroOffline
        ? `<small class="text-muted ms-2">${rota.dataHoraPrimeiroRegistroOffline} - ${rota.dataHoraUltimoRegistroOffline}</small>`
        : '';

    return `<span class="badge ${classe}">${rota.classificacaoOffline}</span>${datas}`;
}

function montarDetalhesAuditoriaOffline(rota, filtro) {
    if (!rota.possuiRegistroOffline) return '';

    let eventos = (rota.eventosAuditoria || []).filter(e => filtro === 'todos' || (filtro === 'offline' ? e.registradoOffline === true : e.registradoOffline !== true));
    let pausas = (rota.pausasAuditoria || []).filter(p => filtro === 'todos' || (filtro === 'offline' ? p.registradoOffline === true : p.registradoOffline !== true));
    let pontosOffline = (rota.historicoLocalizacoesDetalhado || []).filter(p => p.registradoOffline === true).length;

    let linhasEventos = eventos.map(e => {
        let origem = e.registradoOffline ? '<span class="badge bg-label-warning">offline</span>' : '<span class="badge bg-label-secondary">online</span>';
        return `<li>${origem} ${e.dataHora} - evento ${e.tipoEvento}${e.clientEventId ? ` <small class="text-muted">(${e.clientEventId})</small>` : ''}</li>`;
    }).join('');

    let linhasPausas = pausas.map(p => {
        let origem = p.registradoOffline ? '<span class="badge bg-label-warning">offline</span>' : '<span class="badge bg-label-secondary">online</span>';
        return `<li>${origem} ${p.dataHoraInicio}${p.dataHoraFim ? ` ate ${p.dataHoraFim}` : ''} - ${p.motivo || 'Pausa'}</li>`;
    }).join('');

    return `
        <div class="alert alert-light border py-2 mb-0">
            <div class="d-flex flex-wrap gap-2 mb-1">
                <span><strong>Pontos GPS offline:</strong> ${pontosOffline}</span>
                ${rota.localExecucaoId ? `<span><strong>Execucao local:</strong> ${rota.localExecucaoId}</span>` : ''}
                ${rota.identificadorDispositivo ? `<span><strong>Dispositivo:</strong> ${rota.identificadorDispositivo}</span>` : ''}
            </div>
            <div class="small" style="max-height: 120px; overflow-y: auto;">
                ${linhasEventos ? `<strong>Eventos:</strong><ul class="mb-1">${linhasEventos}</ul>` : ''}
                ${linhasPausas ? `<strong>Pausas:</strong><ul class="mb-0">${linhasPausas}</ul>` : ''}
            </div>
        </div>
    `;
}

function montarPopupLocalizacaoOffline(p) {
    return `
        <div style="min-width: 180px; font-size: 0.9em;">
            <strong>Localizacao offline</strong><br/>
            <small>Captura local: ${p.dataHoraRegistroLocal || p.dataHora || '-'}</small><br/>
            <small>Sincronizacao: ${p.dataHoraSincronizacao || '-'}</small><br/>
            ${p.clientEventId ? `<small>Evento: ${p.clientEventId}</small>` : ''}
        </div>
    `;
}

function voltarParaHistorico() {
    $('#map-historico-container').fadeOut('fast', function () {
        $('#tabela-execucoes-container').fadeIn('fast');
    });
}

var previewMap = null;
var previewLayers = [];

function decodePolylineOSR(encoded) {
    if (!encoded) return [];
    var points = [];
    var index = 0, len = encoded.length;
    var lat = 0, lng = 0;
    while (index < len) {
        var b, shift = 0, result = 0;
        do {
            b = encoded.charCodeAt(index++) - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);
        var dlat = ((result & 1) ? ~(result >> 1) : (result >> 1));
        lat += dlat;
        shift = 0; result = 0;
        do {
            b = encoded.charCodeAt(index++) - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);
        var dlng = ((result & 1) ? ~(result >> 1) : (result >> 1));
        lng += dlng;
        points.push([lat / 1e5, lng / 1e5]);
    }
    return points;
}

function AcionarPreviewDeRota() {
    let unidadeOrigemId = $('#UnidadeOrigemId').val();
    let unidadeDestinoId = $('#UnidadeDestinoId').val();
    let paradasCount = $('#tabela-paradas tbody tr:not(#empty-paradas-tr)').length;

    if (!unidadeOrigemId && !unidadeDestinoId && paradasCount === 0) {
        MensagemRodape('warning', 'Adicione paradas ou selecione uma Unidade (Origem/Destino) para visualizar o traçado.');
        return;
    }

    let payload_rota = {
        UnidadeOrigemId: unidadeOrigemId,
        UnidadeDestinoId: unidadeDestinoId,
        Descricao: $('#Descricao').val()
    };

    let paradasArray = [];
    $('#tabela-paradas tbody tr:not(#empty-paradas-tr)').each(function () {
        let row = $(this);
        paradasArray.push({
            Id: row.attr('data-id'),
            Endereco: row.attr('data-endereco'),
            Latitude: row.attr('data-lat'),
            Longitude: row.attr('data-lng')
        });
    });

    let payload = { Rota: payload_rota, Paradas: paradasArray };

    RequisicaoAjaxComCarregamento('/Rota/PreviaDeRotaGeo', 'POST', payload, function (res) {
        if (res.sucesso) {
            let modalEl = document.getElementById('modal-preview-rota');
            let modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();

            $(modalEl).off('shown.bs.modal').on('shown.bs.modal', function () {
                if (!previewMap) {
                    previewMap = L.map('preview-map-container').setView([-15.7942, -47.8821], 12);
                    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(previewMap);
                }

                // Limpar camadas anteriores
                previewLayers.forEach(l => previewMap.removeLayer(l));
                previewLayers = [];

                let bounds = L.latLngBounds();

                let latO = res.unidadeOrigem && res.unidadeOrigem.latitude ? parseFloat(res.unidadeOrigem.latitude.toString().replace(',', '.')) : 0;
                let lngO = res.unidadeOrigem && res.unidadeOrigem.longitude ? parseFloat(res.unidadeOrigem.longitude.toString().replace(',', '.')) : 0;
                let latD = res.unidadeDestino && res.unidadeDestino.latitude ? parseFloat(res.unidadeDestino.latitude.toString().replace(',', '.')) : 0;
                let lngD = res.unidadeDestino && res.unidadeDestino.longitude ? parseFloat(res.unidadeDestino.longitude.toString().replace(',', '.')) : 0;

                let mesmaLocalizacao = (latO === latD && lngO === lngD && latO !== 0);

                if (mesmaLocalizacao) {
                    // Ponto de Partida e Chegada são o mesmo
                    let iconSF = L.divIcon({
                        className: '',
                        html: `<div style="background: linear-gradient(135deg, #3498db 50%, #27ae60 50%); color: #fff; border: 2px solid #fff; border-radius: 50%; width: 24px; height: 24px; text-align: center; line-height: 20px; font-weight: bold; font-size: 13px; box-shadow: 0 3px 6px rgba(0,0,0,0.3);">SF</div>`,
                        iconSize: [28, 28], iconAnchor: [14, 14]
                    });
                    let marker = L.marker([latO, lngO], { icon: iconSF, zIndexOffset: 1000 }).addTo(previewMap);
                    marker.bindPopup(`<b>Partida e Chegada: ${res.unidadeOrigem.nome}</b><br>${res.unidadeOrigem.endereco}`);
                    previewLayers.push(marker);
                    bounds.extend([latO, lngO]);
                } else {
                    // 1. Adicionar Marcador de Origem (Partida)
                    if (latO !== 0) {
                        let iconOrigem = L.divIcon({
                            className: '',
                            html: `<div style="background: #3498db; color: #fff; border: 2px solid #fff; border-radius: 50%; width: 22px; height: 22px; text-align: center; line-height: 18px; font-weight: bold; font-size: 14px; box-shadow: 0 3px 6px rgba(0,0,0,0.3);">S</div>`,
                            iconSize: [26, 26], iconAnchor: [13, 13]
                        });
                        let marker = L.marker([latO, lngO], { icon: iconOrigem }).addTo(previewMap);
                        marker.bindPopup(`<b>Partida: ${res.unidadeOrigem.nome}</b><br>${res.unidadeOrigem.endereco}`);
                        previewLayers.push(marker);
                        bounds.extend([latO, lngO]);
                    }

                    // 3. Adicionar Marcador de Destino (Chegada)
                    if (latD !== 0) {
                        let iconDestino = L.divIcon({
                            className: '',
                            html: `<div style="background: #27ae60; color: #fff; border: 2px solid #fff; border-radius: 50%; width: 22px; height: 22px; text-align: center; line-height: 18px; font-weight: bold; font-size: 14px; box-shadow: 0 3px 6px rgba(0,0,0,0.3);">F</div>`,
                            iconSize: [26, 26], iconAnchor: [13, 13]
                        });
                        let marker = L.marker([latD, lngD], { icon: iconDestino }).addTo(previewMap);
                        marker.bindPopup(`<b>Chegada: ${res.unidadeDestino.nome}</b><br>${res.unidadeDestino.endereco}`);
                        previewLayers.push(marker);
                        bounds.extend([latD, lngD]);
                    }
                }

                // 2. Adicionar Marcadores das Paradas
                if (res.paradas && res.paradas.length > 0) {
                    res.paradas.forEach((p, i) => {
                        if (p.latitude && p.longitude && p.latitude != "0") {
                            let lat = parseFloat(p.latitude.toString().replace(',', '.'));
                            let lng = parseFloat(p.longitude.toString().replace(',', '.'));
                            let markerCss = `border: 2px solid #e74c3c; color: #fff; background: #e74c3c; border-radius: 50%; width: 22px; height: 22px; text-align: center; line-height: 18px; font-weight: bold; font-size: 11px; box-shadow: 0 3px 6px rgba(0,0,0,0.3);`;
                            let paradaIcon = L.divIcon({
                                className: '',
                                html: `<div style="${markerCss}">P${i + 1}</div>`,
                                iconSize: [26, 26],
                                iconAnchor: [13, 13]
                            });
                            let marker = L.marker([lat, lng], { icon: paradaIcon }).addTo(previewMap);
                            marker.bindPopup(`<b>Parada ${i + 1}</b><br>${p.endereco}`);
                            previewLayers.push(marker);
                            bounds.extend([lat, lng]);
                        }
                    });
                }

                // 4. Adicionar Polyline (Traçado)
                if (res.polyline) {
                    try {
                        let decoded = decodePolylineOSR(res.polyline);
                        let polylineLayer = L.polyline(decoded, { color: '#e74c3c', weight: 4 }).addTo(previewMap);
                        previewLayers.push(polylineLayer);
                        
                        // Adicionar Setas de Sentido
                        if (typeof L.polylineDecorator === 'function') {
                            let decorator = L.polylineDecorator(polylineLayer, {
                                patterns: [
                                    { 
                                        offset: '10%', 
                                        repeat: '120px', 
                                        symbol: L.Symbol.arrowHead({ 
                                            pixelSize: 10, 
                                            polygon: false, 
                                            pathOptions: { stroke: true, color: '#e74c3c', weight: 2 } 
                                        }) 
                                    }
                                ]
                            }).addTo(previewMap);
                            previewLayers.push(decorator);
                        }

                        bounds.extend(polylineLayer.getBounds());
                    } catch (e) {
                        console.error("Erro ao decodificar polyline:", e);
                    }
                }

                // Forçar recálculo de tamanho e ajustar zoom
                previewMap.invalidateSize();
                if (bounds.isValid()) {
                    previewMap.fitBounds(bounds, { padding: [30, 30] });
                }
            });
            MensagemRodape('success', 'Traçado OSRM calculado!');
        } else {
            MensagemRodape('warning', 'Falha ao prever rota: ' + (res.mensagem || 'Erro desconhecido'));
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

function carregarDadosUnidadeRota() {
    if (!UNIDADE_ROTA_ID) return;
    var unid = UNIDADES_SISTEMA.find(u => u.Id == UNIDADE_ROTA_ID);
    if (unid) {
        preencherCamposModal(unid.Endereco, unid.Latitude, unid.Longitude);
    }
}

function preencherCamposModal(end, lat, lng) {
    if (end) $('#modal-ponto-endereco').val(end);

    if (lat && lng && lat != "0" && lng != "0") {
        $('#modal-ponto-lat').val(lat);
        $('#modal-ponto-lng').val(lng);
        atualizarMapaModal(lat, lng);
    } else if (end) {
        // Unidade sem coordenadas, buscar via geocode
        $('#modal-ponto-lat, #modal-ponto-lng').val('');
        geocodificarEndereco(end);
    }
}

// Behavioral Logic from Cadastro.cshtml
$('input[name="tipo-pesquisa"]').on('change', function () {
    var tipo = $(this).val();
    $('#section-unidade-sistema, #section-cep').hide();

    if (tipo.startsWith('tipo-')) {
        $('#section-unidade-sistema').show();
        let label = $('label[for="tipo_' + tipo.replace('tipo-', '') + '"]').text().trim();
        $('#label-unidade-sistema').text('Selecione a ' + label);

        // Repopulate select using the global list to ensure Select2 updates correctly
        let $select = $('#modal-selecao-unidade');
        $select.empty().append('<option value="">Selecione...</option>');

        let tipoId = parseInt(tipo.replace('tipo-', ''));
        if (typeof UNIDADES_SISTEMA !== 'undefined') {
            UNIDADES_SISTEMA.forEach(function (u) {
                if (u.Tipo === tipoId) {
                    let opt = $('<option></option>')
                        .val(u.Id)
                        .text(u.Nome)
                        .attr('data-lat', u.Latitude)
                        .attr('data-lng', u.Longitude)
                        .attr('data-end', u.Endereco);
                    $select.append(opt);
                }
            });
        }
        $select.trigger('change');
    }
    else if (tipo === 'cep') {
        $('#section-cep').show();
    }
});

$('#modal-selecao-unidade').on('change', function () {
    var opt = $(this).find(':selected');
    preencherCamposModal(opt.data('end'), opt.data('lat'), opt.data('lng'));
});

$(document).ready(function () {
    if ($('#btn-salvar-cadastro').length > 0) {
        assineEventosCadastro();
    }

    if ($.fn.select2) {
        $('.select2-cadastro').select2({ width: '100%' });
        $('.select2-modal').select2({ width: '100%', dropdownParent: $('#modal-ponto') });
        $('#MedicoResponsavel').select2({ tags: true, placeholder: 'Selecione ou digite...' });
    }

    function toggleSpecificBox() {
        var val = String($('#Recorrente').val() || '').toLowerCase();
        var isRecorrente = (val === 'true' || ($('#Recorrente').is('input[type=checkbox]') && $('#Recorrente').is(':checked')));

        $('#box-config-execucao').slideDown();

        if (isRecorrente) {
            $('#col-data-especifica').hide();
            $('#col-periodo-recorrente').fadeIn();
            $('#DataParaExecucao').val('');
        } else {
            $('#col-data-especifica').fadeIn();
            $('#col-periodo-recorrente').hide();
            $('#DataInicio, #DataFim').val('');
            $('.check-dia-semana').prop('checked', false);
            $('#DiasSemana').val(0);
        }
    }

    $(document).on('change', '.check-dia-semana', function () {
        let total = 0;
        $('.check-dia-semana:checked').each(function () {
            total += parseInt($(this).val());
        });
        $('#DiasSemana').val(total);
    });

    $('#Recorrente').on('change', toggleSpecificBox);
    setTimeout(toggleSpecificBox, 200);

    // Sortable initialization with safety check for Pseudo-SPA/AJAX loading
    var tentarInicializarSortable = function () {
        if (typeof Sortable !== 'undefined') {
            if (document.getElementById('sortable-paradas')) {
                new Sortable(document.getElementById('sortable-paradas'), {
                    animation: 150,
                    handle: '.cursor-move',
                    onEnd: function () {
                        recalcularOrdens();
                    }
                });
            }
        } else {
            setTimeout(tentarInicializarSortable, 100);
        }
    };
    tentarInicializarSortable();
});
