$(document).ready(() => {
    assineEventoBotaoSalvar();

    $('input[name="TipoSolicitacao"]').change(function () {
        if ($(this).val() === 'Hora') {
            $('#container-hora').slideDown();
            $('#container-periodo').slideUp();
        } else {
            $('#container-hora').slideUp();
            $('#container-periodo').slideDown();
        }
    });
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-alterar-senha');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/PortalServidor/SalvarAlterarSenha',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/PortalServidor/AlterarSenha');
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function abrirModalExportarFolha() {
    $('#_ModalExportarFolha').modal('show');
}

function carregarListagemDeLotacoes() {
	let campoLotacao = $('#_ModalExportarFolha #UnidadeId');
	campoLotacao.empty().trigger("change");

	if ($('#_ModalExportarFolha #VinculoDeTrabalhoId').val()) {
		$.ajax({
			url: '/PortalServidor/CarregarLotacoes',
			type: 'GET',
			data: { vinculoId: $('#_ModalExportarFolha #VinculoDeTrabalhoId').val() }
		}).done(function (data) {
			if (data.sucesso) {
                if (data.lotacoes.length > 0) {
					adicioneItemNoCampoSelecionavel(campoLotacao, '', '');

					$.each(data.lotacoes, function (i, item) {
						adicioneItemNoCampoSelecionavel(campoLotacao, item.codigo, item.descricao);
					});

					campoLotacao.val('').trigger('change');
				}
				else {
					MensagemRodape('Não existe nenhuma lotação para o vínculo selecionado.');
				}
			}
		});
	}
}

function exportarFolhaDePonto() {
    let dadosFormulario = ObtenhaFormularioSerializado('formulario-exportar-folha');
    if (!dadosFormulario.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
        return;
    }

    RequisicaoAjaxComCarregamento(
        '/FolhaDePonto/ImprimirFolha',
        'POST',
        dadosFormulario.dados,
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', 'O arquivo será baixado...');
                downloadBase64File(data.base64, data.fileName, data.mimeType);
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function abrirModalRegistroManual() {
    $('#_ModalRegistroManual').modal('show');
}

function salvarRegistroManual() {
    let dadosFormulario = ObtenhaFormularioSerializado('formulario-registro-manual');
    if (!dadosFormulario.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
        return;
    }

    var formData = new FormData();
    formData.append('VinculoDeTrabalhoId', $('#_ModalRegistroManual #VinculoDeTrabalhoId').val());
    formData.append('Observacao', $('#_ModalRegistroManual #Observacao').val());

    var dataBase = $('#txtDataBase').val();
    var tipoSolicitacao = $('input[name="TipoSolicitacao"]:checked').val();

    if (tipoSolicitacao === 'Hora') {
        var hora = $('input[name="Horario"]').val();

        if (!dataBase || !hora) {
            MensagemRodape('warning', 'Informe a data e o horário.');
            return;
        }

        formData.append('DataHora', dataBase + 'T' + hora);
    }
    else {
        var justId = $('select[name="JustificativaId"]').val();

        formData.append('JustificativaDeAusenciaId', justId);
        formData.append('DataInicialAtestado', dataBase);
    }

    var fileInput = $('input[name="Anexo"]')[0];
    if (fileInput.files.length > 0) {
        formData.append('Imagem', fileInput.files[0]);
    }

    AbrirCaixaDeCarregamento('Carregando...');

    setTimeout(function () {
        $.ajax({
            url: '/PortalServidor/SalvarPontoManual',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            error: function (jqXHR, textStatus, errorThrown) {
                FecharCaixaDeCarregamento();
                MensagemRodape('warning', 'Ocorreu um erro inesperado ao fazer a requisição. Tente novamente mais tarde.');
            }
        }).done(function (data) {
            FecharCaixaDeCarregamento();

            if (data.sucesso) {
                $('#_ModalRegistroManual').modal('hide');
                CarregarPagina('/PortalServidor/RegistrosDePonto');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
    }, 750);
}

function baixarComprovante(registroId) {
    RequisicaoAjaxComCarregamento(
        '/PortalServidor/BaixarComprovante',
        'GET',
        { registroId },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', 'O arquivo será baixado...');
                downloadBase64File(data.base64, data.fileName, data.mimeType);
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}