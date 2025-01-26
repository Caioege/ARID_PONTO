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

function carregarFolhaDePonto() {
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
		RequisicaoAjaxComCarregamento(
			'/FolhaDePonto/CarregarFolhaDePonto',
			'GET',
			{ vinculoDeTrabalhoId: vinculoId, mesDeReferencia: mesAno, unidadeId },
			function (data) {
				if (data.sucesso) {
					$('#div-ponto').html(data.html);

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