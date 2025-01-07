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
    componente.find('.data').on('change', function () {
        const valor = $(this).val();

        if (!dataValida(valor)) {
            MensagemRodape('warning', 'Insira uma data válida!')
            $(this).val('');
            $(this).focus();
        }
    });

    componente.find('.cpf').mask('000.000.000-00');
    componente.find('.cpf').on('change', function () {
        const valor = $(this).val();

        if (!cpfValido(valor)) {
            MensagemRodape('warning', 'Insira um cpf válido!')
            $(this).val('');
            $(this).focus();
        }
    });

    componente.find('.cnpj').mask('00.000.000/0000-00');
    componente.find('.cnpj').on('change', function () {
        const valor = $(this).val();

        if (!cnpjValido(valor)) {
            MensagemRodape('warning', 'Insira um CNPJ válido!')
            $(this).val('');
            $(this).focus();
        }
    });

    componente.find('.select2').select2({
        dropdownParent: componente,
        templateResult: function (data) {
            if (!data.id) {
                return null;
            }
            return data.text;
        },
        language: {
            noResults: function () {
                return "Nenhum resultado encontrado";
            },
            inputTooShort: function (args) {
                return `Digite ${args.minimum - args.input.length} caractere(s) para pesquisar.`;
            },
            errorLoading: function () {
                return "Não foi possível carregar os resultados.";
            },
            loadingMore: function () {
                return "Carregando mais resultados...";
            },
            searching: function () {
                return "Pesquisando...";
            },
            removeAllItems: function () {
                return "Remover todos os itens";
            }
        }
    });
}

function dataValida(dateString) {
    if (!dateString) {
        return true;
    }

    const regex = /^(\d{2})\/(\d{2})\/(\d{4})$/;
    const match = dateString.match(regex);

    if (!match) {
        return false;
    }

    const day = parseInt(match[1], 10);
    const month = parseInt(match[2], 10) - 1;
    const year = parseInt(match[3], 10);

    const date = new Date(year, month, day);

    return (
        date.getFullYear() === year &&
        date.getMonth() === month &&
        date.getDate() === day
    );
}

function cpfValido(cpf) {
    if (!cpf) {
        return true;
    }

    cpf = cpf.replace(/\D/g, '');

    if (cpf.length !== 11 || /^(\d)\1+$/.test(cpf)) {
        return false;
    }

    let soma = 0;
    let resto;

    for (let i = 1; i <= 9; i++) {
        soma += parseInt(cpf.charAt(i - 1)) * (11 - i);
    }
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.charAt(9))) return false;

    soma = 0;
    for (let i = 1; i <= 10; i++) {
        soma += parseInt(cpf.charAt(i - 1)) * (12 - i);
    }
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;

    return resto === parseInt(cpf.charAt(10));
}

function cnpjValido(cnpj) {
    if (!cnpj) {
        return true;
    }

    cnpj = cnpj.replace(/\D/g, '');

    if (cnpj.length !== 14) {
        return false;
    }

    if (/^(\d)\1+$/.test(cnpj)) {
        return false;
    }

    let tamanho = cnpj.length - 2;
    let numeros = cnpj.substring(0, tamanho);
    let digitos = cnpj.substring(tamanho);
    let soma = 0;
    let pos = tamanho - 7;

    for (let i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }

    let resultado = soma % 11 < 2 ? 0 : 11 - (soma % 11);
    if (resultado !== parseInt(digitos.charAt(0))) {
        return false;
    }

    tamanho++;
    numeros = cnpj.substring(0, tamanho);
    soma = 0;
    pos = tamanho - 7;

    for (let i = tamanho; i >= 1; i--) {
        soma += numeros.charAt(tamanho - i) * pos--;
        if (pos < 2) pos = 9;
    }

    resultado = soma % 11 < 2 ? 0 : 11 - (soma % 11);
    return resultado === parseInt(digitos.charAt(1));
}

function adicioneItemNoCampoSelecionavel(campoSelecionavel, valor, texto) {
    campoSelecionavel.append(`<option value="${valor}">${texto}</option>`)
}