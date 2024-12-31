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

var RequisicaoAjaxComCarregamento = function (url, tipo, data, callbackSucesso, mensagem) {
    AbrirCaixaDeCarregamento('Carregando...');

    setTimeout(function () {
        $.ajax({
            url: url,
            type: tipo,
            data: data,
            error: function (jqXHR, textStatus, errorThrown) {
                FecharCaixaDeCarregamento();
                MensagemRodape('warning', 'Ocorreu um erro inesperado ao fazer a requisição. Tente novamente mais tarde.');
            }
        }).done(function (data) {
            FecharCaixaDeCarregamento();

            if (callbackSucesso) {
                callbackSucesso(data);
            }
        });
    }, 750);
}

var CarregarPagina = function (url) {
    $(".content-wrapper").loading({
        message: 'Carregando Página...',
        onStart: function (loading) {
            loading.overlay.slideDown(250);
        },
        onStop: function (loading) {
            loading.overlay.slideUp(250);
        }
    });

    setTimeout(function () {
        $.ajax({
            url: url,
            type: 'GET',
            data: {},
            error: function (jqXHR, textStatus, errorThrown) {
                $(".content-wrapper").loading('stop');
                setTimeout(() => { $('#content-body').html('<h5>Ocorreu um erro inesperado ao fazer a requisição. Tente novamente mais tarde.</h5>'); }, 200);
            }
        }).done(function (data) {
            $(".content-wrapper").loading('stop');
            setTimeout(() => {
                $('#content-body').html(data);
                assineMascarasDoComponente($('#content-body'));
            }, 200);
        });
    }, 750);
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

function MensagemRodape(icone, mensagem) {
    const Toast = Swal.mixin({
        toast: true,
        position: "bottom-end",
        showConfirmButton: false,
        timer: 10000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        }
    });
    Toast.fire({
        icon: icone,
        html: mensagem
    });
}

function assineMascarasDoComponente(componente) {
    componente.find('.hora').mask('00:00');
    //componente.find('.hora').clockpicker({
    //    placement: 'top',
    //    align: 'left',
    //    donetext: 'Pronto'
    //});

    componente.find('.data').mask('00/00/0000');
    componente.find('.cpf').mask('000.000.000-00');
    componente.find('.cpf').on('change', function () {
        if ($(this).val().length > 0 && $(this).val().length != 14) {
            $(this).val('');
        }
    });
    componente.find('.cnpj').mask('00.000.000/0000-00');
}