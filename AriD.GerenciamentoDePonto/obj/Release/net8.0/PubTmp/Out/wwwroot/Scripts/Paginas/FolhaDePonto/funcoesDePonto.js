function modalPontoDia(vinculoDeTrabalhoId, data, dataTicks, acao) {
	if ($(`#linha-${vinculoDeTrabalhoId}-${dataTicks}`).hasClass('linha-afastamento')) {
		MensagemRodape('info', 'Esse dia não pode ser editado pois o servidor está afastado.');
		return;
	}

	if ($(`#linha-${vinculoDeTrabalhoId}-${dataTicks}`).hasClass('ponto-fechado')) {
		MensagemRodape('info', 'Esse dia não pode ser editado pois a folha de ponto já está fechada.');
		return;
	}

	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/ModalEdicaoPontoDia',
		'GET',
		{
			vinculoDeTrabalhoId,
			acao,
			data
		},
		function (data) {
			if (data.sucesso) {
				$('#div-modal').html(data.html);
				assineMascarasDoComponente($('#_Modal'));
				$('#_Modal').modal('show');
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		});
}

function salvarPontoDia() {
	let form = $('#formulario-pontodia');
	let data = {
		vinculoDeTrabalhoId: form.find('#VinculoDeTrabalhoId').val(),
		data: $('#DataModal').val(),
		valorHora: $('[data-bs-target="#navs-pills-hora"]').hasClass('active') ? form.find('#ValorHora').val() : null,
		justificativaId: $('[data-bs-target="#navs-pills-justificativa"]').hasClass('active') ? form.find('#JustificativaId').val() : null,
		acao: form.find('#Acao').val(),
		folhaDePonto: $('#TelaFolhaDePonto').length > 0
	};

	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/AtualizePontoDia',
		'POST',
		data,
		function (data) {
			if (data.sucesso) {
				$('#_Modal').modal('hide');

				if ($('#TelaFolhaDePonto').length > 0) {
					carregarFolhaDePonto('Os dados foram salvos... Recarregando folha.');
				} else {
					let linha = $(`#linha-${form.find('#VinculoDeTrabalhoId').val()}-${$('#DataTicks').val()}`);;
					linha.replaceWith(data.html);
					MensagemRodape('success', data.mensagem);
				}
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		});
}

function converterData(data) {
	if (!/^\d{2}\/\d{2}\/\d{4}$/.test(data)) {
		throw new Error("Formato de data inválido. Use DD/MM/YYYY.");
	}
	const [dia, mes, ano] = data.split("/");
	return `${ano}-${mes}-${dia}`;
}