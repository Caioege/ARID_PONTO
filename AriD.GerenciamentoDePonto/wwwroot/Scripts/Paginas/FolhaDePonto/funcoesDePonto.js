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

function salvarPontoDia(removerRegistro, motivoAcao) {
	let form = $('#formulario-pontodia');
	let data = {
		vinculoDeTrabalhoId: form.find('#VinculoDeTrabalhoId').val(),
		data: $('#DataModal').val(),
		valorHora: $('[data-bs-target="#navs-pills-hora"]').hasClass('active') ? form.find('#ValorHora').val() : null,
		justificativaId: $('[data-bs-target="#navs-pills-justificativa"]').hasClass('active') ? form.find('#JustificativaId').val() : null,
		acao: form.find('#Acao').val(),
		folhaDePonto: $('#TelaFolhaDePonto').length > 0,
		removerRegistro: removerRegistro || false,
		motivoAcao: motivoAcao
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

function executeRemoverRegistroPontoDia() {
	Swal.fire({
		target: document.getElementById('_Modal'),
		html: 'Desconsiderar esse registro de ponto?<br />Ele não voltará a ser exibido na folha de ponto a menos que ela seja resetada.',
		input: 'textarea',
		inputLabel: 'Informe o motivo (máximo 300 caracteres)',
		inputPlaceholder: 'Digite o motivo aqui...',
		inputAttributes: {
			maxlength: '300'
		},
		showCancelButton: true,
		confirmButtonText: 'Confirmar',
		cancelButtonText: 'Cancelar',
		allowOutsideClick: false,
		reverseButtons: true,
		customClass: {
			container: 'swal-zindex-alto'
		},
		inputValidator: (value) => {
			if (!value) {
				return 'O motivo é obrigatório!';
			}
		}
	}).then((result) => {
		if (result.isConfirmed) {
			const motivo = result.value;
			salvarPontoDia(true, motivo);
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

			if (jTD.hasClass('registro-app')) {

				const registroAppTitulo = document.createElement("div");
				registroAppTitulo.style.fontStyle = 'italic';
				registroAppTitulo.style.marginBottom = "6px";
				registroAppTitulo.textContent = 'Registro via aplicativo';
				popup.appendChild(registroAppTitulo);

			} else if (jTD.hasClass('registromanualapp')) {

				const registroManualAppTitulo = document.createElement("div");
				registroManualAppTitulo.style.fontStyle = 'italic';
				registroManualAppTitulo.style.marginBottom = "6px";
				registroManualAppTitulo.textContent = 'Registro manual';
				popup.appendChild(registroManualAppTitulo);

			}

			let exibirMovementarRegistros = $('#PodeMovimentarRegistros').val() == 'true';
			if (exibirMovementarRegistros) {
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
	Swal.fire({
		html: "Tem certeza que deseja <b>movimentar</b> esse registro?",
		icon: "question",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "SIM",
		cancelButtonText: 'NÃO'
	}).then((result) => {
		if (result.isConfirmed) {
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

function aprovaRegistro(registroId) {
	Swal.fire({
		html: "Tem certeza que deseja <b>aprovar</b> esse item?<br /><br />Uma vez aprovado, só é possível estornar sua situação ao <b>resetar a folha de ponto</b>.",
		icon: "question",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "SIM",
		cancelButtonText: 'NÃO'
	}).then((result) => {
		if (result.isConfirmed) {
			RequisicaoAjaxComCarregamento(
				'/FolhaDePonto/AprovarRegistroAplicativo',
				'POST',
				{ id: registroId, mesDeReferencia: $('#MesDeReferencia').val(), unidadeId: $('#UnidadeOrganizacionalId').val() },
				function (data) {
					if (data.sucesso) {
						MensagemRodape('success', data.mensagem);
						$(`#linha-registro-app-${registroId}`).remove();
						$('#RecarregarFolhaDePontoAoFechar').val(true);
					} else {
						MensagemRodape('warning', data.mensagem);
					}
				});
		}
	});
}

function reprovaRegistro(registroId) {
	Swal.fire({
		html: "Tem certeza que deseja <b>reprovar</b> esse item?<br /><br />Uma vez reprovado, é impossível estornar sua situação.",
		icon: "question",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "SIM",
		cancelButtonText: 'NÃO'
	}).then((result) => {
		if (result.isConfirmed) {
			RequisicaoAjaxComCarregamento(
				'/FolhaDePonto/ReprovarRegistroAplicativo',
				'POST',
				{ id: registroId },
				function (data) {
					if (data.sucesso) {
						$(`#linha-registro-app-${registroId}`).remove();
						MensagemRodape('success', data.mensagem);
					} else {
						MensagemRodape('warning', data.mensagem);
					}
				});
		}
	});
}

function abrirModalAjuste(id, valorAtual) {
	$('#hdnPontoIdAjuste').val(id);

	$('#txtValorAjuste').val('');
	$('#rdCreditar').prop('checked', true);

	if (valorAtual) {
		if (valorAtual.startsWith('-')) {
			$('#rdDebitar').prop('checked', true);
			$('#txtValorAjuste').val(valorAtual.replace('-', ''));
		} else {
			$('#rdCreditar').prop('checked', true);
			$('#txtValorAjuste').val(valorAtual);
		}
	}

	$('#modalAjusteBH').modal('show');
}

function salvarAjusteBH() {
	var id = $('#hdnPontoIdAjuste').val();
	var valorDigitado = $('#txtValorAjuste').val();
	var tipo = $('input[name="rdTipoAjuste"]:checked').val();

	var valorFinal = null;

	if (valorDigitado && valorDigitado.trim() !== '') {
		valorDigitado = valorDigitado.replace('-', '').replace('+', '').trim();

		if (tipo === 'D') {
			valorFinal = '-' + valorDigitado;
		} else {
			valorFinal = valorDigitado;
		}
	}

	RequisicaoAjaxComCarregamento('/FolhaDePonto/SalvarAjusteBancoHoras',
		'POST',
		{ id: id, ajuste: valorFinal },
		function (data) {
			if (data.sucesso) {
				$('#modalAjusteBH').modal('hide');
				carregarFolhaDePonto('O registro foi atualizado... Recarregando folha.');
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		});
}

function modalHorasExtrasDoDia(pontoDoDiaId) {
	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/ModalHorasExtrasDoDia',
		'GET',
		{ pontoDoDiaId },
		function (data) {
			if (data.sucesso) {
				$('#div-modal').html(data.html);
				$('#_ModalHorasExtras').modal('show');
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		}
	);
}

function aprovarHoraExtra(horaExtraId, maxMinutos) {
	let minutosAprovados = parseInt($(`#min-aprov-${horaExtraId}`).val() || '0', 10);
	if (minutosAprovados < 0) minutosAprovados = 0;
	if (minutosAprovados > maxMinutos) minutosAprovados = maxMinutos;

	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/AprovarHoraExtra',
		'POST',
		{ horaExtraId, minutosAprovados },
		function (data) {
			if (data.sucesso) {
				MensagemRodape('success', data.mensagem);
				// Reabre modal para refletir status/valores
				$('#_ModalHorasExtras').modal('hide');
				// Se quiser atualizar a folha inteira:
				carregarFolhaDePonto('Atualizando folha...');
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		}
	);
}

function reprovarHoraExtra(horaExtraId) {
	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/ReprovarHoraExtra',
		'POST',
		{ horaExtraId },
		function (data) {
			if (data.sucesso) {
				MensagemRodape('success', data.mensagem);
				$('#_ModalHorasExtras').modal('hide');
				carregarFolhaDePonto('Atualizando folha...');
			} else {
				MensagemRodape('warning', data.mensagem);
			}
		}
	);
}

function modalAuditoriaFolha(pontoDoDiaId) {
	RequisicaoAjaxComCarregamento(
		'/FolhaDePonto/ModalAuditoriaFolha',
		'GET',
		{ vinculoDeTrabalhoId: $('#VinculoDeTrabalhoId').val(), mesAno: $('#MesDeReferencia').val(), pontoDoDiaId },
		function (data) {
			if (data.sucesso) {
				$('#div-modal').html(data.html);
				$('#_ModalAuditoria').modal('show');
			} else {
				MensagemRodape('warning', data.mensagem || 'Erro ao abrir auditoria.');
			}
		}
	);
}