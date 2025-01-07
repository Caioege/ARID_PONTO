$(document).ready(() => {
    assineEventoBotaoSalvar();
    assinechangeArquivoImagem();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        RequisicaoAjaxComCarregamento(
            '/Servidor/Salvar',
            'POST',
            ObtenhaFormularioSerializado('formulario-servidor'),
            function (data) {
                MensagemRodape('success', data.mensagem);
            });
    });
}

function abrirSelecionarArquivoImagem() {
    $('#input-file').trigger('click');
}

function assinechangeArquivoImagem() {
    $('#input-file').on('change', function () {
        let formData = new FormData();
        let fileInput = document.querySelector('#input-file');

        if (fileInput.files.length > 0) {
            formData.append('file', fileInput.files[0]);
            formData.append('id', $('#Id').val());

            $.ajax({
                url: '/Foto/SalvarFotoServidor',
                type: 'POST',
                data: formData,
                cache: false,
                contentType: false,
                processData: false,
            }).done(function (data) {
                $('#fechar-offcanvas').trigger('click');
                if (!data.sucesso) {
                    $('#fotoInput').val('').trigger('change');
                    MensagemRodape('warning', 'Ocorreu um erro ao tentar salvar a foto.');
                } else {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Servidor/Alterar/' + $('#Id').val());
                }
            });
        }
    });
}

function removerFotoServidor() {
    $.ajax({
        url: '/Foto/RemoverFotoServidor',
        type: 'DELETE',
        data: { id: $('#Id').val() }
    }).done(function (data) {
        $('#fechar-offcanvas').trigger('click');
        if (!data.sucesso) {
            $('#fotoInput').val('').trigger('change');
            MensagemRodape('warning', 'Ocorreu um erro ao tentar remover a foto.');
        } else {
            MensagemRodape('success', data.mensagem);
            CarregarPagina('/Servidor/Alterar/' + $('#Id').val());
        }
    });
}

function abrirModalVinculoDeTrabalho(id) {
    RequisicaoAjaxComCarregamento(
        '/Servidor/ModalVinculoDeTrabalho',
        'GET',
        { id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                salvarCadastroDeVinculo();
                salvarCadastroDeLotacao();
                removerCadastroDeLotacao();
                assineMascarasDoComponente($('#_Modal'));
                $('#_Modal').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function salvarCadastroDeVinculo() {
    $('#_Modal').find('#btn-salvar-modal').on('click', function () {
        let vinculoDeTrabalho = ObtenhaFormularioSerializado('formulario-vinculo');
        vinculoDeTrabalho.ServidorId = $('#formulario-servidor').find('#Id').val();

        RequisicaoAjaxComCarregamento(
            '/Servidor/SalvarVinculoDeTrabalho',
            'POST',
            { vinculoDeTrabalho },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    $('#_Modal').modal('hide');
                    CarregarPagina('/Servidor/Alterar/' + vinculoDeTrabalho.ServidorId);
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function salvarCadastroDeLotacao() {
    $('#_Modal').find('#btn-salvar-lotacao').on('click', function () {
        let lotacao = ObtenhaFormularioSerializado('formulario-lotacao');
        lotacao.VinculoDeTrabalhoId = $('#formulario-vinculo').find('#Id').val();

        RequisicaoAjaxComCarregamento(
            '/Servidor/SalvarLotacao',
            'POST',
            { lotacao },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    recarregarListaDeLotacoes();
                    cancelarEdicaoLotacao();
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function removerCadastroDeLotacao() {
    $('#_Modal').find('#btn-remover-lotacao').on('click', function () {
        let id = $('#formulario-lotacao').find('#Id').val();

        RequisicaoAjaxComCarregamento(
            '/Servidor/RemoverLotacao',
            'DELETE',
            { id },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    recarregarListaDeLotacoes();
                    cancelarEdicaoLotacao();
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function carregarEdicaoDeLotacao(id) {
    RequisicaoAjaxComCarregamento(
        '/Servidor/DadosEdicaoLotacao',
        'GET',
        { id },
        function (data) {
            if (data.sucesso) {
                $('#UnidadeOrganizacionalId').empty().trigger("change");

                adicioneItemNoCampoSelecionavel($('#UnidadeOrganizacionalId'), '', '');
                $.each(data.unidades, function (i, item) {
                    adicioneItemNoCampoSelecionavel($('#UnidadeOrganizacionalId'), item.codigo, item.descricao);
                });

                $('#div-modal-lotacao').find('#Id').val(data.id);

                if (data.unidadeId) {
                    $('#UnidadeOrganizacionalId').val(data.unidadeId).trigger('change');
                }

                if (data.entrada) {
                    $('#Entrada').val(data.entrada).trigger('change');
                } else {
                    $('#Entrada').val('').trigger('change');
                }

                if (data.saida) {
                    $('#Saida').val(data.saida).trigger('change');
                } else {
                    $('#Saida').val('').trigger('change');
                }

                if (data.matriculaEquipamento) {
                    $('#MatriculaEquipamento').val(data.matriculaEquipamento).trigger('change');
                } else {
                    $('#MatriculaEquipamento').val('').trigger('change');
                }

                if (id == 0) {
                    $('#modal-footer-lotacao #btn-remover-lotacao').attr('style', 'display: none !important');
                } else {
                    $('#modal-footer-lotacao #btn-remover-lotacao').attr('style', 'display: inline-flex !important');
                }

                assineMascarasDoComponente($('#div-modal-lotacao'));


                $('#div-dados-modal').fadeOut('fast', function () {
                    $('#div-modal-lotacao').fadeIn('fast');
                });

                $('#modal-footer-principal').fadeOut('fast', function () {
                    $('#modal-footer-lotacao').fadeIn('fast');
                });
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function cancelarEdicaoLotacao() {
    $('#div-modal-lotacao').fadeOut('fast', function () {
        $('#div-dados-modal').fadeIn('fast');
    });

    $('#modal-footer-lotacao').fadeOut('fast', function () {
        $('#modal-footer-principal').fadeIn('fast');
    });
}

function recarregarListaDeLotacoes() {
    $.ajax({
        url: '/Servidor/PartialLotacoes',
        type: 'GET',
        data: { vinculoId: $('#formulario-vinculo').find('#Id').val() }
    }).done(function (data) {
        if (data.sucesso) {
            $('#div-partial-lotacoes').html(data.html);
        }
    });
}