$(document).ready(() => {
    assineEventoBotaoSalvar();

    if ($('#Id').val() != '0') {
        CarregueTabelaDeUnidadesOrganizacionais();
        assineClickImagem();

        atualizarTabelaResumo();
        controleSeparador();

        // Evento para mostrar/ocultar separador no modal
        $('#ddlFormatoModal').change(controleSeparador);

        // Botão Adicionar Linha (No Modal)
        $('#btnAdicionarColunaModal').click(function () {
            var tbody = $('#tbodyExportacaoModal');
            var index = tbody.find('tr').length;
            var ordem = index + 1;
            var optionsEnum = $('#template-opcoes-enum').html();

            var tr = `
            <tr>
                <td class="text-center align-middle index-ordem">${ordem}</td>
                <td>
                    <select class="form-select form-select-sm select-campo-modal" name="ItensExportacao[${index}].Campo">
                        ${optionsEnum}
                    </select>
                </td>
                <td>
                    <input type="text" name="ItensExportacao[${index}].CodigoExterno" class="form-control form-control-sm input-codigo-modal" />
                </td>
                <td>
                    <input type="text" name="ItensExportacao[${index}].Formato" class="form-control form-control-sm input-formato-modal" />
                </td>
                <td class="text-center">
                    <button type="button" class="btn btn-outline-danger btn-sm btn-remover-modal">
                        <i class='bx bx-trash'></i>
                    </button>
                    <input type="hidden" name="ItensExportacao[${index}].Ordem" value="${ordem}" class="input-ordem" />
                </td>
            </tr>`;
            tbody.append(tr);
        });

        // Remover Linha (No Modal)
        $(document).on('click', '.btn-remover-modal', function () {
            $(this).closest('tr').remove();
            reordenarIndicesModal();
        });
    }
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-organizacao');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Organizacao/Salvar',
            'POST',
            dadosFormulario.dados,
            function (data) {
                MensagemRodape('success', data.mensagem);
                $('#Id').val(data.id);
            });
    });
}

function CarregueTabelaDeUnidadesOrganizacionais() {
    $('#Adicional').val(JSON.stringify({ OrganizacaoId: $('#Id').val() }));
    $.ajax({
        url: '/UnidadeOrganizacional/TabelaPaginada',
        type: 'GET',
        data: { Adicional: $('#Adicional').val() }
    }).done(function (data) {
        $('#grid').html(data);
    });
}

var assineClickImagem = function () {
    $('.img-foto').on('click', function () {
        $('#fotoInput').trigger('click');
    });

    $('#fotoInput').on('change', function () {
        let formData = new FormData();
        let fileInput = document.querySelector('#fotoInput');

        if (fileInput.files.length > 0) {
            formData.append('file', fileInput.files[0]);
            formData.append('id', $('#Id').val());

            $.ajax({
                url: '/Organizacao/PostBrasao',
                type: 'POST',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
            }).done(function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Organizacao/Alterar/' + $('#Id').val());
                } else {
                    $('#fotoInput').val('').trigger('change');
                    MensagemRodape('warning', data.mensagem);
                }
            });
        }
    });
}

function enviarMensagemWhatsAppTeste() {
    RequisicaoAjaxComCarregamento(
        '/WhatsApp/EnviarComprovanteDePonto',
        'POST',
        {  },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function abrirModalExportacao() {
    var modal = new bootstrap.Modal(document.getElementById('modalConfigExportacao'));
    modal.show();
}

// Controle visual do separador
function controleSeparador() {
    var formato = $('#ddlFormatoModal').val();
    if (formato == '0') { // Excel
        $('#divSeparadorModal').slideUp();
        $('#divSeparadorResumo').hide();
    } else {
        $('#divSeparadorModal').slideDown();
        $('#divSeparadorResumo').show();
    }
}

// Reordena os índices para o ASP.NET entender a lista
function reordenarIndicesModal() {
    $('#tbodyExportacaoModal tr').each(function (i) {
        var ordem = i + 1;
        $(this).find('.index-ordem').text(ordem);
        $(this).find('.input-ordem').val(ordem);

        $(this).find('select, input').each(function () {
            var name = $(this).attr('name');
            if (name) {
                var novoName = name.replace(/\[\d+\]/, '[' + i + ']');
                $(this).attr('name', novoName);
            }
        });
    });
}

// AÇÃO PRINCIPAL: Pega dados do Modal e joga na Tela Principal (Apenas visualização)
function confirmarConfiguracaoModal() {
    // 1. Atualiza campos simples
    var textoFormato = $('#ddlFormatoModal option:selected').text();
    $('#txtFormatoResumo').val(textoFormato);
    $('#txtSeparadorResumo').val($('#txtSeparadorModal').val());
    controleSeparador(); // Atualiza visibilidade na tela principal

    // 2. Atualiza Tabela de Resumo
    atualizarTabelaResumo();

    // 3. Fecha Modal
    var modalEl = document.getElementById('modalConfigExportacao');
    var modal = bootstrap.Modal.getInstance(modalEl);
    modal.hide();
}

function atualizarTabelaResumo() {
    var tbodyResumo = $('#tbodyResumoPrincipal');
    tbodyResumo.empty();

    var linhasModal = $('#tbodyExportacaoModal tr');

    if (linhasModal.length === 0) {
        tbodyResumo.html('<tr><td colspan="4" class="text-center text-muted"><small>Nenhuma configuração definida.</small></td></tr>');
        return;
    }

    // Itera sobre as linhas do modal para criar linhas de texto na tela principal
    linhasModal.each(function () {
        var ordem = $(this).find('.index-ordem').text();
        var textoCampo = $(this).find('.select-campo-modal option:selected').text();
        var codExterno = $(this).find('.input-codigo-modal').val();
        var formato = $(this).find('.input-formato-modal').val();

        var tr = `
            <tr>
                <td class="text-center">${ordem}</td>
                <td>${textoCampo}</td>
                <td>${codExterno || '-'}</td>
                <td>${formato || '-'}</td>
            </tr>
        `;
        tbodyResumo.append(tr);
    });

    // Atualiza também os inputs simples iniciais caso tenha carregado a página agora
    var textoFormato = $('#ddlFormatoModal option:selected').text();
    $('#txtFormatoResumo').val(textoFormato);
    $('#txtSeparadorResumo').val($('#txtSeparadorModal').val());
}