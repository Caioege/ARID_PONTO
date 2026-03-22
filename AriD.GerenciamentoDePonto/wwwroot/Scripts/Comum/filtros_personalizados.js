/**
 * ARI-D - Gerenciamento de Filtros Personalizados
 * Componente para salvar e carregar configurações de filtros em JSON.
 */

var FiltrosPersonalizados = {
    urlRelatorio: window.location.pathname,

    init: function (containerSelector, dropdownSelector) {
        this.containerSelector = containerSelector;
        this.dropdownSelector = dropdownSelector;
        
        let container = $(this.dropdownSelector).parent();
        if (container.find('.btn-limpar-filtro').length === 0) {
            container.append(`
                <button class="btn btn-outline-danger btn-sm ms-1 btn-excluir-filtro" onclick="FiltrosPersonalizados.excluir()" title="Excluir Filtro" style="display:none;">
                    <i class='bx bx-trash'></i>
                </button>
                <button class="btn btn-outline-warning btn-sm ms-1 btn-limpar-filtro" onclick="FiltrosPersonalizados.limpar()" title="Limpar Filtro Selecionado" style="display:none;">
                    <i class='bx bx-eraser'></i>
                </button>
            `);
        }

        $(this.dropdownSelector).on('change', function() {
            if($(this).val()) {
                $('.btn-excluir-filtro').show();
            } else {
                $('.btn-excluir-filtro').hide();
            }
        });

        this.carregarTemplates();
    },

    salvar: function () {
        Swal.fire({
            title: 'Salvar Template de Filtro',
            html: `
                <input id="swal-input-nome" class="swal2-input" placeholder="Nome do filtro">
                <div class="form-check mt-3" style="text-align: left; padding-left: 3rem;">
                    <input class="form-check-input" type="checkbox" id="swal-input-compartilhado">
                    <label class="form-check-label" for="swal-input-compartilhado">
                        Compartilhar com outros operadores
                    </label>
                </div>
            `,
            focusConfirm: false,
            showCancelButton: true,
            confirmButtonText: 'Salvar',
            cancelButtonText: 'Cancelar',
            preConfirm: () => {
                const nome = document.getElementById('swal-input-nome').value;
                if (!nome) {
                    Swal.showValidationMessage('O nome do filtro é obrigatório.');
                    return false;
                }
                return {
                    nome: nome,
                    compartilhado: document.getElementById('swal-input-compartilhado').checked
                }
            }
        }).then((result) => {
            if (result.isConfirmed) {
                let filtros = {};
                $(this.containerSelector).find('input, select').not(this.dropdownSelector).each(function () {
                    let id = $(this).attr('id');
                    if (id) {
                        filtros[id] = $(this).val();
                    }
                });

                let model = {
                    nome: result.value.nome,
                    urlRelatorio: this.urlRelatorio,
                    jsonFiltros: JSON.stringify(filtros),
                    compartilhado: result.value.compartilhado
                };

                RequisicaoAjaxComCarregamento('/FiltroRelatorio/Salvar', 'POST', model, function (data) {
                    if (data.sucesso) {
                        MensagemRodape('success', data.mensagem);
                        FiltrosPersonalizados.carregarTemplates();
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
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
                dropdown.trigger('change');
            }
        });
    },

    aplicar: function () {
        let option = $(this.dropdownSelector).find('option:selected');
        let jsonStr = option.data('json');
        
        if (!jsonStr) {
            this.limpar();
            return;
        }

        let filtros = JSON.parse(jsonStr);
        for (let id in filtros) {
            let elemento = $('#' + id);
            if (elemento.length && id !== $(this.dropdownSelector).attr('id')) {
                elemento.val(filtros[id]).trigger('change');
                elemento.prop('disabled', true);
            }
        }
        $('.btn-limpar-filtro').show();
        $('.btn-excluir-filtro').show();
        MensagemRodape('info', 'Filtro aplicado. Pressione limpar para editar os campos.');
    },

    limpar: function () {
         $(this.containerSelector).find('input, select').not(this.dropdownSelector).each(function () {
             $(this).prop('disabled', false);
         });
         $(this.dropdownSelector).val('');
         $('.btn-limpar-filtro').hide();
         $('.btn-excluir-filtro').hide();
    },

    excluir: function () {
        let idFiltro = $(this.dropdownSelector).val();
        if (!idFiltro) return;

        Swal.fire({
            title: 'Excluir Filtro?',
            text: "Você tem certeza que deseja excluir permanentemente este filtro?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Sim, excluir!',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                RequisicaoAjaxComCarregamento('/FiltroRelatorio/Excluir', 'POST', { id: idFiltro }, function (data) {
                    if (data.sucesso) {
                        MensagemRodape('success', data.mensagem);
                        FiltrosPersonalizados.limpar();
                        FiltrosPersonalizados.carregarTemplates();
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
            }
        });
    }
};
