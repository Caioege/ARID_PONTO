/**
 * ARI-D - Gerenciamento de Filtros Personalizados
 * Componente para salvar e carregar configurações de filtros em JSON.
 */

var FiltrosPersonalizados = {
    urlRelatorio: window.location.pathname,

    init: function (containerSelector, dropdownSelector) {
        this.containerSelector = containerSelector;
        this.dropdownSelector = dropdownSelector;
        this.carregarTemplates();
    },

    salvar: function () {
        let nome = prompt("Digite um nome para este template de filtro:");
        if (!nome) return;

        let compartilhado = confirm("Deseja compartilhar este filtro com outros operadores?");
        let filtros = {};

        $(this.containerSelector).find('input, select').each(function () {
            let id = $(this).attr('id');
            if (id) {
                filtros[id] = $(this).val();
            }
        });

        let model = {
            nome: nome,
            urlRelatorio: this.urlRelatorio,
            jsonFiltros: JSON.stringify(filtros),
            compartilhado: compartilhado
        };

        RequisicaoAjaxComCarregamento('/FiltroRelatorio/Salvar', 'POST', model, function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                FiltrosPersonalizados.carregarTemplates();
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
    },

    carregarTemplates: function () {
        let self = this;
        $.get('/FiltroRelatorio/ObtenhaFiltros', { urlRelatorio: this.urlRelatorio }, function (data) {
            if (data.sucesso) {
                let dropdown = $(self.dropdownSelector);
                dropdown.empty();
                dropdown.append('<option value="">-- Templates Salvos --</option>');
                
                data.dados.forEach(function (filtro) {
                    dropdown.append(`<option value='${filtro.id}' data-json='${filtro.jsonParametros}'>${filtro.nome}</option>`);
                });
            }
        });
    },

    aplicar: function (id) {
        let option = $(this.dropdownSelector).find('option:selected');
        let jsonStr = option.data('json');
        if (!jsonStr) return;

        let filtros = JSON.parse(jsonStr);
        for (let id in filtros) {
            let elemento = $('#' + id);
            if (elemento.length) {
                elemento.val(filtros[id]).trigger('change');
            }
        }
        MensagemRodape('info', 'Filtro aplicado com sucesso.');
    }
};
