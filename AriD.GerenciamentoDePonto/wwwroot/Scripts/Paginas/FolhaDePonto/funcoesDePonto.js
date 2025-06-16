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

function assineEventoAvancarRecuarPonto() {
	const popup = document.getElementById("popup");
	let hideTimeout = null;

	document.querySelectorAll("td.executa-movimentacao").forEach(td => {
		td.addEventListener("mouseenter", () => {
			if (hideTimeout) clearTimeout(hideTimeout);

			let jTD = $(td);

			popup.innerHTML = "";

			const isEntrada1 = td.classList.contains("entrada1");
			const isSaida5 = td.classList.contains("saida5");

			const titulo = document.createElement("div");
			titulo.style.fontWeight = "bold";
			titulo.style.marginBottom = "6px";
			titulo.textContent = td.textContent;
			popup.appendChild(titulo);

			if (!isSaida5) {
				const btnAvancar = document.createElement("button");
				btnAvancar.classList = 'btn btn-primary';
				btnAvancar.innerHTML = "<i class='bx bx-right-arrow-alt'></i> Avançar";
				btnAvancar.onclick = () => executaMovimentacaoRegistro(jTD.data('id'), jTD.data('acao'), true);
				popup.appendChild(btnAvancar);
			}

			if (!isEntrada1) {
				const btnRecuar = document.createElement("button");
				btnRecuar.classList = 'btn btn-primary';
				btnRecuar.innerHTML = "<i class='bx bx-left-arrow-alt'></i> Recuar";
				btnRecuar.onclick = () => executaMovimentacaoRegistro(jTD.data('id'), jTD.data('acao'), false);
				popup.appendChild(btnRecuar);
			}

			popup.classList.remove("oculto");

			const rect = td.getBoundingClientRect();
			const popupHeight = popup.offsetHeight;
			const popupWidth = popup.offsetWidth;
			const verticalOffset = 0;

			popup.style.top = (rect.top + window.scrollY - popupHeight - verticalOffset) + "px";
			popup.style.left = (rect.left + window.scrollX + rect.width / 2 - popupWidth / 2) + "px";
		});

		td.addEventListener("mouseleave", () => {
			hideTimeout = setTimeout(() => {
				popup.classList.add("oculto");
			}, 200);
		});
	});

	popup.addEventListener("mouseenter", () => {
		if (hideTimeout) clearTimeout(hideTimeout);
	});

	popup.addEventListener("mouseleave", () => {
		popup.classList.add("oculto");
	});
}

function executaMovimentacaoRegistro(id, classe, avancar) {
	RequisicaoAjaxComCarregamento('/FolhaDePonto/MovimentarRegistro',
		'POST',
		{ id, classe, avancar },
		function (data) {
			if (data.sucesso) {
				carregarFolhaDePonto('O registro foi atualizado... Recarregando folha.');
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		});
}

function expandirImagem(caminho) {
	document.getElementById('imagemExpandidaSrc').src = caminho;
	document.getElementById('imagemExpandida').classList.remove('d-none');
}

function fecharImagemExpandida() {
	document.getElementById('imagemExpandida').classList.add('d-none');
	document.getElementById('imagemExpandidaSrc').src = '';
}

document.getElementById('imagemExpandida').addEventListener('click', function (e) {
	if (e.target.id === 'imagemExpandida') {
		fecharImagemExpandida();
	}
});