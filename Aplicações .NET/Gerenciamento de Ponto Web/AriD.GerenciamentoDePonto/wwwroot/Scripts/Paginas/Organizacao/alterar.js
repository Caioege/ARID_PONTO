$(document).ready(() => {
    assineEventoBotaoSalvar();

    if ($('#Id').val() != '0') {
        CarregueTabelaDeUnidadesOrganizacionais();
        assineClickImagem();
        assineEventosFiltroUnidades();
    }

    // Inicializa a primeira tab se necessário (Bootstrap faz isso automaticamente via data-attributes)
    // Mas garantimos o carregamento da tabela se a tab de unidades for clicada
    $('button[data-bs-target="#tab-unidades"]').on('shown.bs.tab', function () {
        CarregueTabelaDeUnidadesOrganizacionais();
    });
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

function assineEventosFiltroUnidades() {
    $('#filtro-nome-unidade').on('keyup', function() {
        // Debounce para não sobrecarregar o servidor
        clearTimeout(this.intervaloBusca);
        this.intervaloBusca = setTimeout(CarregueTabelaDeUnidadesOrganizacionais, 500);
    });

    $('#filtro-tipo-unidade').on('change', function() {
        CarregueTabelaDeUnidadesOrganizacionais();
    });
}

function CarregueTabelaDeUnidadesOrganizacionais() {
    const parametrosAdicionais = {
        OrganizacaoId: $('#Id').val(),
        Nome: $('#filtro-nome-unidade').val(),
        Tipo: $('#filtro-tipo-unidade').val()
    };

    $('#Adicional').val(JSON.stringify(parametrosAdicionais));
    
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