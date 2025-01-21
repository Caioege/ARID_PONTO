$(document).ready(() => {
	assineChangeUnidadeOrganizacional();
	$('#UnidadeOrganizacionalId').trigger('change');
});

function assineChangeUnidadeOrganizacional() {
	$('#UnidadeOrganizacionalId').on('change', function () {
		let campoFuncao = $('#FuncaoId');
		let campoDepartamento = $('#DepartamentoId');
		let campoHorarioDeTrabalho = $('#HorarioDeTrabalhoId');

		campoFuncao.empty().trigger("change");
		campoDepartamento.empty().trigger("change");
		campoHorarioDeTrabalho.empty().trigger("change");

		if ($(this).val()) {
			$.ajax({
				url: '/FolhaDePonto/FiltrosPontoDoDia',
				type: 'GET',
				data: { unidadeId: $(this).val() }
			}).done(function (data) {
				if (data.sucesso) {
					if (data.funcoes.length > 0) {
						adicioneItemNoCampoSelecionavel(campoFuncao, '', 'Todas');

						$.each(data.funcoes, function (i, item) {
							adicioneItemNoCampoSelecionavel(campoFuncao, item.codigo, item.descricao);
						});

						campoFuncao.val('').trigger('change');
					}

					if (data.departamentos.length > 0) {
						adicioneItemNoCampoSelecionavel(campoDepartamento, '', 'Todos');

						$.each(data.departamentos, function (i, item) {
							adicioneItemNoCampoSelecionavel(campoDepartamento, item.codigo, item.descricao);
						});

						campoDepartamento.val('').trigger('change');
					}

					if (data.horarios.length > 0) {
						adicioneItemNoCampoSelecionavel(campoHorarioDeTrabalho, '', '');

						$.each(data.horarios, function (i, item) {
							adicioneItemNoCampoSelecionavel(campoHorarioDeTrabalho, item.codigo, item.descricao);
						});

						campoHorarioDeTrabalho.val('').trigger('change');
					}
					else {
						MensagemRodape('warning', 'Nenhum servidor lotado nessa unidade.');
					}
				}
			});
		}
	});
}

function carregarPontoDoDia() {
	let unidadeId = $('#UnidadeOrganizacionalId').val() || '';
	let horarioId = $('#HorarioDeTrabalhoId').val() || '';
	let departamentoId = $('#DepartamentoId').val() || '';
	let funcaoId = $('#FuncaoId').val() || '';
	let data = $('#Data').val() || '';


	ajusteValidacaoDeCampo($('#UnidadeOrganizacionalId'), unidadeId != '');
	ajusteValidacaoDeCampo($('#HorarioDeTrabalhoId'), horarioId != '');
	ajusteValidacaoDeCampo($('#Data'), data != '');

	$('#div-ponto-dia').html('');

	if (unidadeId && horarioId && data) {
		RequisicaoAjaxComCarregamento(
			'/FolhaDePonto/CarregarPontoDoDia',
			'GET',
			{
				unidadeId,
				horarioId,
				departamentoId,
				funcaoId,
				data
			},
			function (data) {
				if (data.sucesso) {
					$('#div-ponto-dia').html(data.html);
				} else {
					MensagemRodape('warning', data.mensagem);
				}
			});
	}
}