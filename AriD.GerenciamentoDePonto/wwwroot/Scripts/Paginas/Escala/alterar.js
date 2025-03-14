$(document).ready(() => {
    assineSalvarFormulario();
});

function assineSalvarFormulario() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-escala');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Escala/Salvar',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Escala/Alterar/' + data.id)
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function assineCarregarCargaHoraria() {
    $('.linha').find('.hora').on('change', function () {
        if ($(this).val().length != 5) {
            $(this).val('');
            return;
        }

        let linha = $(this).parents('tr');

        $.ajax({
            url: '/HorarioDeTrabalho/CalculaCargaHorariaDoDia',
            type: 'GET',
            data: obtenhaHorarioDia(linha)
        }).done(function (data) {
            if (data.sucesso) {
                linha.find('.carga-horaria').html(data.cargaHoraria);
            }
        });
    });
}

function obtenhaHorarioDia(linha) {
    return {
        Entrada1: linha.find('.entrada1').val(),
        Saida1: linha.find('.saida1').val(),
        Entrada2: linha.find('.entrada2').val(),
        Saida2: linha.find('.saida2').val(),
        Entrada3: linha.find('.entrada3').val(),
        Saida3: linha.find('.saida3').val(),
        Entrada4: linha.find('.entrada4').val(),
        Saida4: linha.find('.saida4').val(),
        Entrada5: linha.find('.entrada5').val(),
        Saida5: linha.find('.saida5').val(),
    };
}

function abrirModalAdicionarCiclo(id, escalaId) {
    RequisicaoAjaxComCarregamento(
        '/Escala/ModalCiclo',
        'GET',
        { id, escalaId },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_ModalCiclo'));
                assineCarregarCargaHoraria();
                assineSalvarModal();
                $('#_ModalCiclo').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function assineSalvarModal() {
    $('#btn-salvar-modal').on('click', function () {
        let modal = $('#_ModalCiclo');
        let cicloDaEscala = {
            Id: modal.find('#Id').val(),
            EscalaId: $('#formulario-escala').find('#Id').val(),
            Ciclo: modal.find('#Ciclo').val(),
            Entrada1: modal.find('.entrada1').val(),
            Entrada2: modal.find('.entrada2').val(),
            Entrada3: modal.find('.entrada3').val(),
            Entrada4: modal.find('.entrada4').val(),
            Entrada5: modal.find('.entrada5').val(),
            Saida1: modal.find('.saida1').val(),
            Saida2: modal.find('.saida2').val(),
            Saida3: modal.find('.saida3').val(),
            Saida4: modal.find('.saida4').val(),
            Saida5: modal.find('.saida5').val(),
        };

        RequisicaoAjaxComCarregamento(
            '/Escala/SalvarCiclo',
            'POST',
            { cicloDaEscala },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    $('#_ModalCiclo').modal('hide');
                    CarregarPagina('/Escala/Alterar/' + $('#formulario-escala').find('#Id').val())
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function removerCiclo(id) {
    RequisicaoAjaxComCarregamento(
        '/Escala/RemoverCiclo',
        'POST',
        { id },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                $('#_ModalCiclo').modal('hide');
                CarregarPagina('/Escala/Alterar/' + $('#formulario-escala').find('#Id').val())
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function abrirModalServidorCiclo(id) {
    RequisicaoAjaxComCarregamento(
        '/Escala/ModalServidorCiclo',
        'GET',
        { id },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_ModalEscalaServidorCiclo'));
                assineSalvarServidorEscalaCiclo();
                $('#_ModalEscalaServidorCiclo').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function assineSalvarServidorEscalaCiclo() {
    $('#btn-salvar-modal').on('click', function () {
        let modal = $('#_ModalEscalaServidorCiclo');
        let escalaDoServidor = {
            Id: modal.find('#Id').val(),
            EscalaId: $('#formulario-escala').find('#Id').val(),
            VinculoDeTrabalhoId: modal.find('#VinculoDeTrabalhoId').val(),
            Data: modal.find('#Data').val(),
            DataFim: modal.find('#DataFim').val()
        };

        RequisicaoAjaxComCarregamento(
            '/Escala/SalvarServidorCiclo',
            'POST',
            { escalaDoServidor },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    $('#_ModalEscalaServidorCiclo').modal('hide');
                    CarregarPagina('/Escala/Alterar/' + $('#formulario-escala').find('#Id').val())
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function abrirModalServidorMensal(id, escalaId) {
    RequisicaoAjaxComCarregamento(
        '/Escala/ModalServidorMensal',
        'GET',
        { id, escalaId },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_ModalServidorMensal'));
                assineSalvarEscalaServidorMensal();
                assineCarregarCargaHoraria();
                $('#_ModalServidorMensal').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function removerServidorEscalaCiclo(id) {
    RequisicaoAjaxComCarregamento(
        '/Escala/RemoverServidorCiclo',
        'POST',
        { id },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                $('#_ModalEscalaServidorCiclo').modal('hide');
                $('#_ModalServidorMensal').modal('hide');
                CarregarPagina('/Escala/Alterar/' + $('#formulario-escala').find('#Id').val());
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function assineSalvarEscalaServidorMensal() {
    $('#btn-salvar-modal').on('click', function () {
        let modal = $('#_ModalServidorMensal');
        let escalaDoServidor = {
            Id: modal.find('#Id').val(),
            EscalaId: $('#formulario-escala').find('#Id').val(),
            VinculoDeTrabalhoId: modal.find('#VinculoDeTrabalhoId').val(),
            Data: modal.find('#Data').val(),
            CicloDaEscala: {
                Entrada1: modal.find('.entrada1').val(),
                Entrada2: modal.find('.entrada2').val(),
                Entrada3: modal.find('.entrada3').val(),
                Entrada4: modal.find('.entrada4').val(),
                Entrada5: modal.find('.entrada5').val(),
                Saida1: modal.find('.saida1').val(),
                Saida2: modal.find('.saida2').val(),
                Saida3: modal.find('.saida3').val(),
                Saida4: modal.find('.saida4').val(),
                Saida5: modal.find('.saida5').val()
            }
        };

        RequisicaoAjaxComCarregamento(
            '/Escala/SalvarEscalaServidorMensal',
            'POST',
            { escalaDoServidor },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    $('#_ModalServidorMensal').modal('hide');
                    CarregarPagina('/Escala/Alterar/' + $('#formulario-escala').find('#Id').val())
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function removerEscala() {
    RequisicaoAjaxComCarregamento(
        '/Escala/RemoverEscala',
        'POST',
        { id },
        function (data) {
            if (data.sucesso) {
                MensagemRodape('success', data.mensagem);
                CarregarPagina('/Escala/Index');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}