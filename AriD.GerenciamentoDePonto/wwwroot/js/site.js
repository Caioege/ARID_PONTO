$(document).ready(function () {
    $.ajaxSetup({
        statusCode: {
            401: function () {
                window.location.href = '/Acesso/Sair';
            }
        }
    });
});

var AbrirCaixaDeCarregamento = function (mensagem) {
    Swal.fire({
        title: mensagem ?? 'Carregando...',
        allowEscapeKey: false,
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading()
        }
    });
}

var FecharCaixaDeCarregamento = function () {
    swal.close();
}

var CarregarPagina = function (url) {
    AbrirCaixaDeCarregamento('Carregando página...');
    setTimeout(function () {
        $.ajax({
            url: url,
            type: 'GET',
            data: {},
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(textStatus, errorThrown);
            }
        }).done(function (data) {
            FecharCaixaDeCarregamento();
            $('#content-body').html(data);
        });
    }, 500);
}

function ObtenhaFormularioSerializado(formId) {
    const $form = $(`#${formId}`);
    if ($form.length === 0) {
        console.error(`Formulário com o ID "${formId}" não foi encontrado.`);
        return {};
    }

    const formData = {};

    $form.find('input, select, textarea').each(function () {
        const $element = $(this);
        const name = $element.attr('name') || $element.attr('id'); // Usa "name" ou "id" como chave
        if (!name) return; // Ignora elementos sem "name" ou "id"

        if ($element.is(':checkbox')) {
            // Para checkbox, adiciona true/false
            formData[name] = $element.is(':checked');
        } else if ($element.is(':radio')) {
            // Para radio, apenas pega o valor do selecionado
            if ($element.is(':checked')) {
                formData[name] = $element.val();
            }
        } else if ($element.is('select[multiple]')) {
            // Para selects múltiplos, retorna array de valores
            formData[name] = $element.val() || [];
        } else {
            // Para outros tipos (texto, textarea, etc.)
            formData[name] = $element.val();
        }
    });

    return formData;
}