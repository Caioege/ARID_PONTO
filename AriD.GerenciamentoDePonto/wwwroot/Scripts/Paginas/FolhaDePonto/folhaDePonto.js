$(document).ready(() => {
	assineChangeUnidadeOrganizacional();
	assineChangeServidor();

	$('#UnidadeOrganizacionalId').trigger('change');
});

function assineChangeUnidadeOrganizacional() {
	$('#UnidadeOrganizacionalId').on('change', function () {
		let campoServidores = $('#ServidorId');
		campoServidores.empty().trigger("change");

		$('#div-ponto').html('');

		$('#btn-acoes-container').hide();
		$('#btn-imprimir').attr("style", "display: none !important");
		$('#btn-gerenciar-app').attr("style", "display: none !important");
		$('#btn-visualizarhistoricofolha').attr("style", "display: none !important");
		$('#btn-resetar').attr("style", "display: none !important");
		$('#btn-fechar-ponto').attr("style", "display: none !important");

		if ($(this).val()) {
			$.ajax({
				url: '/FolhaDePonto/ServidoresLotadosNaUnidade',
				type: 'GET',
				data: { unidadeId: $(this).val() }
			}).done(function (data) {
				if (data.sucesso) {
					if (data.servidores.length > 0) {
						adicioneItemNoCampoSelecionavel(campoServidores, '', '');

						$.each(data.servidores, function (i, item) {
							adicioneItemNoCampoSelecionavel(campoServidores, item.codigo, item.descricao);
						});

						campoServidores.val('').trigger('change');
					}
					else {
						MensagemRodape('Não existe nenhum servidor lotado na unidade selecionada.');
					}
				}
			});
		}
	});
}

function assineChangeServidor() {
	$('#ServidorId').on('change', function () {
		let campoVinculos = $('#VinculoDeTrabalhoId');
		campoVinculos.empty().trigger("change");

		$('#div-ponto').html('');

		$('#btn-acoes-container').hide();
		$('#btn-imprimir').attr("style", "display: none !important");
		$('#btn-gerenciar-app').attr("style", "display: none !important");
		$('#btn-visualizarhistoricofolha').attr("style", "display: none !important");
		$('#btn-ocorrencias-espelho').attr("style", "display: none !important");
		$('#btn-resetar').attr("style", "display: none !important");
		$('#btn-fechar-ponto').attr("style", "display: none !important");

		if ($(this).val()) {
			$.ajax({
				url: '/FolhaDePonto/VinculosDoServidor',
				type: 'GET',
				data: { servidorId: $(this).val(), unidadeId: $('#UnidadeOrganizacionalId').val() }
			}).done(function (data) {
				if (data.sucesso) {
					if (data.vinculos.length > 0) {
						adicioneItemNoCampoSelecionavel(campoVinculos, '', '');

						$.each(data.vinculos, function (i, item) {
							adicioneItemNoCampoSelecionavel(campoVinculos, item.codigo, item.descricao);
						});

						campoVinculos.val(data.vinculos.length == 0 ? data.vinculos[0].codigo : '').trigger('change');
					}
				}
			});
		}
	});
}

function carregarFolhaDePonto(mensagem) {
	$('#div-ponto').html('');

	let unidadeId = $('#UnidadeOrganizacionalId').val() || '';
	let servidorId = $('#ServidorId').val() || '';
	let vinculoId = $('#VinculoDeTrabalhoId').val() || '';
	let mesAno = $('#MesDeReferencia').val() || '';

	ajusteValidacaoDeCampo($('#UnidadeOrganizacionalId'), unidadeId != '');
	ajusteValidacaoDeCampo($('#ServidorId'), servidorId != '');
	ajusteValidacaoDeCampo($('#VinculoDeTrabalhoId'), vinculoId != '');
	ajusteValidacaoDeCampo($('#MesDeReferencia'), mesAno != '');

	if (unidadeId && servidorId && vinculoId && mesAno) {
		$.ajax({
			url: '/FolhaDePonto/CarregarFolhaDePonto',
			type: 'GET',
			data: { vinculoDeTrabalhoId: vinculoId, mesDeReferencia: mesAno, unidadeId },
		}).done(function (data) {
			if (data.sucesso) {
				$('#div-ponto').html(data.html);

				$('#btn-acoes-container').show();
				$('#btn-imprimir').attr("style", "display: block !important");
				$('#btn-gerenciar-app').attr("style", "display: block !important");
				$('#btn-visualizarhistoricofolha').attr("style", "display: block !important");
				$('#btn-ocorrencias-espelho').attr("style", "display: block !important");

				if (!data.exibirAbrir) {
					$('#btn-resetar').attr("style", "display: block !important");
				} else {
					$('#btn-resetar').attr("style", "display: none !important");
				}

				assineEventoAvancarRecuarPonto();

				ajustarExibicaoBotaoFecharPonto(data.exibirAcoes && !data.exibirAbrir);
				ajustarExibicaoBotaoAbrirPonto(data.exibirAcoes && data.exibirAbrir);
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		});
	}
}

function ajustarExibicaoBotaoFecharPonto(exibir) {
	if (exibir) {
		$('#btn-fechar-ponto').attr("style", "display: block !important");
	} else {
		$('#btn-fechar-ponto').attr("style", "display: none !important");
	}
}

function ajustarExibicaoBotaoAbrirPonto(exibir) {
	if (exibir) {
		$('#btn-abrir-ponto').attr("style", "display: block !important");
	} else {
		$('#btn-abrir-ponto').attr("style", "display: none !important");
	}
}

function fecharOuAbrirFolhaDePonto(fechar) {
	let unidadeId = $('#UnidadeOrganizacionalId').val() || '';
	let vinculoId = $('#VinculoDeTrabalhoId').val() || '';
	let mesAno = $('#MesDeReferencia').val() || '';

	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/FecharAbrirFolhaDePonto',
		'POST',
		{ vinculoDeTrabalhoId: vinculoId, mesDeReferencia: mesAno, unidadeId, fechar },
		function (data) {
			if (data.sucesso) {
				carregarFolhaDePonto();
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		});
}

function fecharPeriodoPonto() {
	fecharOuAbrirFolhaDePonto(true);
}
function abrirPeriodoPonto() {
	fecharOuAbrirFolhaDePonto(false);
}

function imprimirFolhaDePonto() {
	let unidadeId = $('#UnidadeOrganizacionalId').val() || '';
	let servidorId = $('#ServidorId').val() || '';
	let vinculoId = $('#VinculoDeTrabalhoId').val() || '';
	let mesAno = $('#MesDeReferencia').val() || '';

	ajusteValidacaoDeCampo($('#UnidadeOrganizacionalId'), unidadeId != '');
	ajusteValidacaoDeCampo($('#ServidorId'), servidorId != '');
	ajusteValidacaoDeCampo($('#VinculoDeTrabalhoId'), vinculoId != '');
	ajusteValidacaoDeCampo($('#MesDeReferencia'), mesAno != '');

	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/ImprimirFolha',
		'POST',
		{ vinculoDeTrabalhoId: vinculoId, mesDeReferencia: mesAno, unidadeId },
		function (data) {
			if (data.sucesso) {
				MensagemRodape('success', 'O arquivo será baixado...');
				downloadBase64File(data.base64, data.fileName, data.mimeType);
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		});
}

function resetarFolhaDePonto() {
	Swal.fire({
		html: "Tem certeza que deseja restaurar essa folha de ponto?<br><br><b>Todas as alterações manuais e lançamentos serão removidos.</b>",
		icon: "question",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "SIM",
		cancelButtonText: 'NÃO'
	}).then((result) => {
		if (result.isConfirmed) {
			let unidadeId = $('#UnidadeOrganizacionalId').val() || '';
			let vinculoId = $('#VinculoDeTrabalhoId').val() || '';
			let mesAno = $('#MesDeReferencia').val() || '';

			RequisicaoAjaxComCarregamento(
				'/FolhaDePonto/ResetarFolha',
				'POST',
				{ vinculoDeTrabalhoId: vinculoId, mesDeReferencia: mesAno, unidadeId },
				function (data) {
					if (data.sucesso) {
						MensagemRodape('success', data.mensagem);
						carregarFolhaDePonto('Recarregando folha.');
					} else {
						MensagemRodape('warning', data.mensagem);
					}
				});
		}
	});
}

function abrirModalGerenciarRegistrosApp() {
	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/ModalSolicitacoesApp',
		'GET',
		{
			vinculoDeTrabalhoId: $('#VinculoDeTrabalhoId').val(),
			mesDeReferencia: $('#MesDeReferencia').val()
		},
		function (data) {
			if (data.sucesso) {
				$('#div-modal').html(data.html);
				assineMascarasDoComponente($('#_ModalSolicitacoesApp'));

				$('#_ModalSolicitacoesApp').on('hidden.bs.modal', function () {
					var deveRecarregar = ($('#RecarregarFolhaDePontoAoFechar').val() || '').toLowerCase();
					if (deveRecarregar === 'true') {
						carregarFolhaDePonto();
					}
				});

				$('#_ModalSolicitacoesApp').modal('show');
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		});
}

function abrirModalOcorrenciasEspelho() {
	RequisicaoAjaxComCarregamento('/FolhaDePonto/CarregarOcorrenciasRodape', 'GET', {
		vinculoDeTrabalhoId: $('#VinculoDeTrabalhoId').val(),
		mesAno: $('#MesDeReferencia').val()
	}, function (data) {
		if (data.sucesso) {
			var html = `
				<div class="modal fade" id="modalOcorrenciasEspelho" tabindex="-1" aria-hidden="true">
					<div class="modal-dialog modal-lg">
						<div class="modal-content">
							<div class="modal-header">
								<h5 class="modal-title">Ocorrências do Espelho (Rodapé do PDF)</h5>
								<button type="button" class="btn-close" data-bs-dismiss="modal"></button>
							</div>
							<div class="modal-body">
								<div class="mb-3">
									<label class="form-label">Nova Ocorrência</label>
									<textarea id="txtNovaOcorrencia" class="form-control" rows="3" placeholder="Digite uma ocorrência para adicionar ao rodapé do espelho e clique em Salvar"></textarea>
									<div class="mt-2 text-end">
										<button type="button" class="btn btn-primary" onclick="salvarOcorrenciaEspelho()">Adicionar Ocorrência</button>
									</div>
								</div>
								
								<hr />
								<h6 class="mb-3">Ocorrências Cadastradas no Período</h6>
								<div class="table-responsive">
									<table class="table table-bordered table-striped" id="tabelaOcorrenciasEspelho">
										<thead>
											<tr>
												<th>Data Cadastro</th>
												<th>Usuário</th>
												<th>Ocorrência</th>
												<th width="80" class="text-center">Ações</th>
											</tr>
										</thead>
										<tbody>`;

			if (data.ocorrencias && data.ocorrencias.length > 0) {
				data.ocorrencias.forEach(function (oc) {
					var dataFormatada = new Date(oc.dataHoraCadastro).toLocaleString('pt-BR');
					html += `
											<tr>
												<td>${dataFormatada}</td>
												<td>${oc.usuarioCadastroNome}</td>
												<td>${oc.descricao}</td>
												<td class="text-center">
													<button type="button" class="btn btn-sm btn-danger" onclick="excluirOcorrenciaEspelho(${oc.id})" title="Excluir Ocorrência">
														<i class='bx bx-trash'></i>
													</button>
												</td>
											</tr>`;
				});
			} else {
				html += `<tr><td colspan="4" class="text-center">Nenhuma ocorrência cadastrada.</td></tr>`;
			}

			html += `
										</tbody>
									</table>
								</div>
							</div>
						</div>
					</div>
				</div>`;

			$('#div-modal').html(html);
			$('#modalOcorrenciasEspelho').modal('show');
		} else {
			MensagemRodape('warning', data.mensagem);
		}
	});
}

function salvarOcorrenciaEspelho() {
	var descricao = $('#txtNovaOcorrencia').val();

	if (!descricao || descricao.trim() === '') {
		MensagemRodape('warning', 'Digite a ocorrência antes de salvar.');
		return;
	}

	RequisicaoAjaxComCarregamento('/FolhaDePonto/SalvarOcorrenciaRodape', 'POST', {
		vinculoDeTrabalhoId: $('#VinculoDeTrabalhoId').val(),
		mesAno: $('#MesDeReferencia').val(),
		descricao: descricao
	}, function (data) {
		if (data.sucesso) {
			$('#modalOcorrenciasEspelho').modal('hide');
			abrirModalOcorrenciasEspelho();
			MensagemRodape('success', 'Ocorrência inserida com sucesso.');
		} else {
			MensagemRodape('warning', data.mensagem);
		}
	});
}

function excluirOcorrenciaEspelho(id) {
	Swal.fire({
		text: "Tem certeza que deseja remover esta ocorrência do espelho?",
		icon: "question",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "SIM",
		cancelButtonText: 'NÃO'
	}).then((result) => {
		if (result.isConfirmed) {
			RequisicaoAjaxComCarregamento('/FolhaDePonto/ExcluirOcorrenciaRodape', 'POST', { id: id }, function (data) {
				if (data.sucesso) {
					$('#modalOcorrenciasEspelho').modal('hide');
					abrirModalOcorrenciasEspelho();
					MensagemRodape('success', 'Ocorrência removida com sucesso.');
				} else {
					MensagemRodape('warning', data.mensagem);
				}
			});
		}
	});
}