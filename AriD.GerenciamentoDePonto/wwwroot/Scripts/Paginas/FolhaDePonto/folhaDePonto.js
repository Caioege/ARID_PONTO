$(document).ready(() => {
	assineChangeUnidadeOrganizacional();
	assineChangeServidor();

	$('#UnidadeOrganizacionalId').trigger('change');
});

function assineChangeUnidadeOrganizacional() {
	$('#UnidadeOrganizacionalId').on('change', function () {
		let campoServidores = $('#ServidorId');
		campoServidores.empty().trigger("change");

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

				$('#btn-imprimir').attr("style", "display: inline !important; margin-right: 5px;");
				$('#btn-gerenciar-app').attr("style", "display: inline !important; margin-right: 5px;");

				if (!data.exibirAbrir) {
					$('#btn-resetar').attr("style", "display: inline !important; margin-right: 5px;");
				} else {
					$('#btn-resetar').attr("style", "display: none !important; margin-right: 5px;");
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
		$('#btn-fechar-ponto').attr("style", "display: inline !important");
	} else {
		$('#btn-fechar-ponto').attr("style", "display: none !important");
	}
}

function ajustarExibicaoBotaoAbrirPonto(exibir) {
	if (exibir) {
		$('#btn-abrir-ponto').attr("style", "display: inline !important");
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