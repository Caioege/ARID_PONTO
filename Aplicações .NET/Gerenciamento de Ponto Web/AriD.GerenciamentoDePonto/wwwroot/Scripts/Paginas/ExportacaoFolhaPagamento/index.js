function exportarFolhaPagamento() {
    let dadosFormulario = ObtenhaFormularioSerializado('form-export-folha');
    if (!dadosFormulario.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
        return;
    }

    dadosFormulario.dados.formatoArquivo = $('#FormatoArquivo').val();
    dadosFormulario.dados.agruparPorMatricula = $('#AgruparPorMatricula').val();
    dadosFormulario.dados.somenteServidoresHabilitados = $('#SomenteHabilitados').val();

    RequisicaoAjaxComCarregamento(
        '/ExportacaoFolhaPagamento/Exportar',
        'POST',
        dadosFormulario.dados,
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.resumo || 'Exportação gerada.');
                downloadBase64File(data.base64, data.fileName, data.mimeType);
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        }
    );
}

function abrirModalLayout(id) {
    RequisicaoAjaxComCarregamento(
        '/ExportacaoFolhaPagamento/ModalLayout',
        'GET',
        { id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                $('#_ModalLayoutFolha').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem || 'Erro ao abrir layout.');
            }
        }
    );
}

function adicionarCampoLayout() {
    const tbody = $('#tbl-campos tbody');
    const ordem = tbody.find('tr').length + 1;

    // clona o primeiro select existente (tem as opções)
    let selectHtml = $('#tbl-campos tbody select.campo:first').prop('outerHTML');
    if (!selectHtml) {
        // fallback (se o layout está vazio, vai existir no modal a lista de opções mesmo assim)
        selectHtml = '<select class="form-select campo"></select>';
    }

    tbody.append(`
        <tr>
            <td class="text-center"><input class="form-control ordem" value="${ordem}" readonly /></td>
            <td>${selectHtml}</td>
            <td><input class="form-control nomeColuna" /></td>
            <td><input class="form-control valorFixo" /></td>
            <td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger" onclick="removerLinhaCampo(this)">X</button></td>
        </tr>
    `);
}

function removerLinhaCampo(btn) {
    const tbody = $('#tbl-campos tbody');
    $(btn).closest('tr').remove();

    tbody.find('tr').each(function (i) {
        $(this).find('input.ordem').val(i + 1);
    });
}

function salvarLayout() {
    // monta JSON dos campos
    let campos = [];
    $('#tbl-campos tbody tr').each(function (i) {
        campos.push({
            Ordem: i + 1,
            Campo: parseInt($(this).find('select.campo').val() || '0', 10),
            NomeColuna: $(this).find('input.nomeColuna').val(),
            ValorFixo: $(this).find('input.valorFixo').val(),
            Ativo: true
        });
    });

    $('#CamposJson').val(JSON.stringify(campos));

    let dadosFormulario = ObtenhaFormularioSerializado('form-layout');
    if (!dadosFormulario.formularioEstaValido) {
        MensagemRodape('warning', 'Preencha os campos obrigatórios.');
        return;
    }

    RequisicaoAjaxComCarregamento(
        '/ExportacaoFolhaPagamento/SalvarLayout',
        'POST',
        dadosFormulario.dados,
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                $('#_ModalLayoutFolha').modal('hide');
                // recarrega a página para atualizar o dropdown de layouts (simples e funciona)
                CarregarPagina('/ExportacaoFolhaPagamento/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        }
    );
}

function abrirModalCodigos() {
    RequisicaoAjaxComCarregamento(
        '/ExportacaoFolhaPagamento/ModalCodigos',
        'GET',
        {},
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                $('#_ModalCodigosFolha').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem || 'Erro ao abrir códigos.');
            }
        }
    );
}

function adicionarLinhaCodigo() {
    const tbody = $('#tbl-codigos tbody');
    let selectHtml = $('#tbl-codigos tbody select.tipo:first').prop('outerHTML');
    if (!selectHtml) selectHtml = '<select class="form-select tipo"></select>';

    tbody.append(`
        <tr>
            <td>${selectHtml}</td>
            <td><input class="form-control percentual" placeholder="(opcional)" /></td>
            <td><input class="form-control codigo" /></td>
            <td><input class="form-control descricao" /></td>
            <td class="text-center"><button type="button" class="btn btn-sm btn-outline-danger" onclick="removerLinhaCodigo(this)">X</button></td>
        </tr>
    `);
}

function removerLinhaCodigo(btn) {
    $(btn).closest('tr').remove();
}

function salvarCodigos() {
    let mapas = [];
    $('#tbl-codigos tbody tr').each(function () {
        const tipo = parseInt($(this).find('select.tipo').val() || '0', 10);
        const percentualStr = ($(this).find('input.percentual').val() || '').trim();
        const codigo = ($(this).find('input.codigo').val() || '').trim();
        const descricao = ($(this).find('input.descricao').val() || '').trim();

        let percentual = null;
        if (percentualStr) {
            // aceita 50,00 ou 50.00
            const n = parseFloat(percentualStr.replace(',', '.'));
            if (!isNaN(n)) percentual = n;
        }

        mapas.push({
            TipoEvento: tipo,
            Percentual: percentual,
            Codigo: codigo,
            Descricao: descricao,
            Ativo: true
        });
    });

    RequisicaoAjaxComCarregamento(
        '/ExportacaoFolhaPagamento/SalvarCodigos',
        'POST',
        { mapeamentosJson: JSON.stringify(mapas) },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                $('#_ModalCodigosFolha').modal('hide');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        }
    );
}

function editarLayoutSelecionado() {
    const id = parseInt($('#LayoutId').val() || '0', 10);
    if (!id) {
        MensagemRodape('warning', 'Selecione um layout para editar.');
        return;
    }
    abrirModalLayout(id);
}