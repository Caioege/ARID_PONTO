$(document).ready(() => {
    $('.cep-endereco-partial').on('change', function () {
        let cep = $(this).val();
        if (cep) {
            $.ajax({
                url: '/Cep/ConsulteCEP',
                type: 'GET',
                data: { cep }
            }).done(function (data) {
                const dados = data.dados;
                $('.logradouro-endereco-partial').val(dados.logradouro);
                $('.numero-endereco-partial').val(dados.numero);
                $('.complemento-endereco-partial').val(dados.complemento);
                $('.bairro-endereco-partial').val(dados.bairro);
                $('.cidade-endereco-partial').val(dados.localidade);
                $('.uf-endereco-partial').val(dados.ufEnum);
            });
        }
    });
});