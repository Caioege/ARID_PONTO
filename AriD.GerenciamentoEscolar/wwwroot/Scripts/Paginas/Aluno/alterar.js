$(document).ready(() => {
    assineEventoBotaoSalvar();
    assinechangeArquivoImagem();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-aluno');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Aluno/Salvar',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Aluno/Alterar/' + $('#Id').val());
                }
                else {
                    MensagemRodape('warning', data.mensagem);
                }
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
                    CarregarPagina('/Aluno/Alterar/' + $('#Id').val());
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
            CarregarPagina('/Aluno/Alterar/' + $('#Id').val());
        }
    });
}

function abrirModalVinculoDeTrabalho(id) {
    RequisicaoAjaxComCarregamento(
        '/Aluno/ModalVinculoDeTrabalho',
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
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-vinculo');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        let vinculoDeTrabalho = dadosFormulario.dados;
        vinculoDeTrabalho.ServidorId = $('#formulario-servidor').find('#Id').val();

        RequisicaoAjaxComCarregamento(
            '/Aluno/SalvarVinculoDeTrabalho',
            'POST',
            { vinculoDeTrabalho },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    $('#_Modal').modal('hide');
                    CarregarPagina('/Aluno/Alterar/' + vinculoDeTrabalho.ServidorId);
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function salvarCadastroDeLotacao() {
    $('#_Modal').find('#btn-salvar-lotacao').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-lotacao');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        let lotacao = dadosFormulario.dados;
        lotacao.VinculoDeTrabalhoId = $('#formulario-vinculo').find('#Id').val();

        RequisicaoAjaxComCarregamento(
            '/Aluno/SalvarLotacao',
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
            '/Aluno/RemoverLotacao',
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
        '/Aluno/DadosEdicaoLotacao',
        'GET',
        { id },
        function (data) {
            if (data.sucesso) {
                $('#EscolaId').empty().trigger("change");

                adicioneItemNoCampoSelecionavel($('#EscolaId'), '', '');
                $.each(data.escolas, function (i, item) {
                    adicioneItemNoCampoSelecionavel($('#EscolaId'), item.codigo, item.descricao);
                });

                $('#div-modal-lotacao').find('#Id').val(data.id);

                if (data.escolaId) {
                    $('#EscolaId').val(data.escolaId).trigger('change');
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
        url: '/Aluno/PartialLotacoes',
        type: 'GET',
        data: { vinculoId: $('#formulario-vinculo').find('#Id').val() }
    }).done(function (data) {
        if (data.sucesso) {
            $('#div-partial-lotacoes').html(data.html);
        }
    });
}

function abrirModalAfastamento(id) {
    RequisicaoAjaxComCarregamento(
        '/Aluno/ModalAfastamento',
        'GET',
        { id, servidorId: $('#formulario-servidor').find('#Id').val() },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_Modal'));
                assineSalvarAfastamento();
                assineRemoverAfastamento();
                $('#_Modal').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function assineSalvarAfastamento() {
    $('#_Modal').find('#btn-salvar-modal').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-afastamento');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        let afastamento = dadosFormulario.dados;

        RequisicaoAjaxComCarregamento(
            '/Aluno/SalvarAfastamento',
            'POST',
            { afastamento },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    $('#_Modal').modal('hide');
                    recarregarPartialDeAfastamento();
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function recarregarPartialDeAfastamento() {
    $.ajax({
        url: '/Aluno/PartialAfastamentos',
        type: 'GET',
        data: { servidorId: $('#formulario-servidor').find('#Id').val() }
    }).done(function (data) {
        if (data.sucesso) {
            $('#s3').html(data.html);
        } else {
            MensagemRodape('warning', data.mensagem);
        }
    });
}

function assineRemoverAfastamento() {
    $('#_Modal').find('#btn-remover-modal').on('click', function () {
        Swal.fire({
            title: 'Remover Afastamento?',
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#7b7b7b",
            confirmButtonText: "Sim",
            cancelButtonText: 'N�o'
        }).then((result) => {
            if (result.isConfirmed) {
                RequisicaoAjaxComCarregamento(
                    '/Aluno/RemoverAfastamento',
                    'DELETE',
                    { afastamentoId: $('#form-afastamento').find('#Id').val() },
                    function (data) {
                        if (data.sucesso) {
                            MensagemRodape('success', data.mensagem);
                            $('#_Modal').modal('hide');
                            recarregarPartialDeAfastamento();
                        } else {
                            MensagemRodape('warning', data.mensagem);
                        }
                    });
            }
        });
    });
}

function matricularNaUnidade() {
    RequisicaoAjaxComCarregamento(
        '/Aluno/ModalMatricularNaEscola',
        'GET',
        {},
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_ModalAlocarNaEscola'));
                $('#_ModalAlocarNaEscola').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function matricularNaEscolaModal() {
    let dadosFormulario = ObtenhaFormularioSerializado('_ModalAlocarNaEscola');
    if (!dadosFormulario.formularioEstaValido) {
        MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
        return;
    }

    let alunoId = $('#formulario-aluno #Id').val();
    RequisicaoAjaxComCarregamento(
        '/Aluno/MatricularNaEscola',
        'POST',
        { alunoId, escolaId: $('#_ModalAlocarNaEscola #EscolaId').val(), idEquipamento: $('#_ModalAlocarNaEscola #IdEquipamento').val() },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                $('#_ModalAlocarNaEscola').modal('hide');
                CarregarPagina('/Aluno/Alterar/' + alunoId);
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function desalocarAlunoDaEscola() {
    let alunoId = $('#formulario-aluno #Id').val();
    Swal.fire({
        text: "Tem certeza que deseja desalocar esse aluno da escola?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "SIM",
        cancelButtonText: 'NÃO'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/Aluno/Desalocar',
                'POST',
                { alunoId },
                function (data) {
                    if (data.sucesso) {
                        MensagemRodape('success', data.mensagem);
                        CarregarPagina('/Aluno/Alterar/' + alunoId);
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
        }
    });
}