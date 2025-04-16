$(document).ready(() => {
    assineEventoBotaoSalvar();
    assineChangeMesAnoDiario();
    assineChangeSituacaoDaTurma();
});

function assineEventoBotaoSalvar() {
    $('#btn-salvar').on('click', function () {
        let dadosFormulario = ObtenhaFormularioSerializado('formulario-turma');
        if (!dadosFormulario.formularioEstaValido) {
            MensagemRodape('warning', 'Existem campos obrigatórios que não foram preenchidos!');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Turma/Salvar',
            'POST',
            dadosFormulario.dados,
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    CarregarPagina('/Turma/Alterar/' + data.id);
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

var abrirModalAlocarAlunos = function () {
    RequisicaoAjaxComCarregamento(
        '/Turma/ModalAlocarAlunos',
        'GET',
        { turmaId: $('#formulario-turma #Id').val() },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_ModalAlocarAlunos'));
                assineAlocarAlunos();
                $('#_ModalAlocarAlunos').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

var assineAlocarAlunos = function () {
    $('#_ModalAlocarAlunos #btn-salvar-modal').on('click', function () {
        let entrada = $('#_ModalAlocarAlunos #EntradaNaTurma').val();
        if (!entrada) {
            MensagemRodape('warning', 'Informe a data de entrada para prosseguir.');
            return;
        }

        let alunos = [];
        $.each($('#_ModalAlocarAlunos #tabela-alocar-alunos .checkbox-aluno:checked'), function (i, item) {
            alunos.push($(item).data('alunoid'));
        });

        if (alunos.length == 0) {
            MensagemRodape('warning', 'Selecione pelo menos um aluno para prosseguir.');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Turma/AlocarAlunos',
            'POST',
            { turmaId: $('#formulario-turma #Id').val(), entrada, alunos },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    $('#_ModalAlocarAlunos').modal('hide');
                    CarregarPagina('/Turma/Alterar/' + $('#formulario-turma #Id').val());
                } else {
                    MensagemRodape('warfning', data.mensagem);
                }
            });
    });
}

var removerVinculoDeAlunoTurma = function (alunoTurmaId) {
    Swal.fire({
        text: "Tem certeza que deseja remover o vínculo do aluno na turma?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "SIM",
        cancelButtonText: 'NÃO'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/Turma/RemoverVinculoDeAluno',
                'POST',
                { alunoTurmaId },
                function (data) {
                    if (data.sucesso) {
                        MensagemRodape('success', data.mensagem);
                        $('#_ModalAlunoTurma').modal('hide');
                        CarregarPagina('/Turma/Alterar/' + $('#formulario-turma #Id').val());
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
        }
    });
}

function assineChangeSituacaoDaTurma() {
    $('#Situacao').on('change', function () {
        let situacao = $(this).val();
        let situacaoAtiva = $('#SituacaoAtiva').val();
        let situacaoAnterior = $('#SituacaoAnterior').val();

        if (situacaoAnterior == situacaoAtiva && situacaoAnterior != situacao) {
            Swal.fire({
                html: 'Ao alterar a situação da turma para outra que não seja ativa, todos os alunos que estiverem cursando terão sua situação alterada para "Concluído".<br><br>Deseja realmente alterar a situação da turma?',
                icon: "question",
                showCancelButton: true,
                confirmButtonColor: "#3085d6",
                cancelButtonColor: "#d33",
                confirmButtonText: "SIM",
                cancelButtonText: 'NÃO'
            }).then((result) => {
                if (!result.isConfirmed) {
                    $('#Situacao').val(situacaoAnterior).trigger('change');
                }
            });
        }
    });
}

function abrirModalDadosAlunoTurma(alunoTurmaId) {
    RequisicaoAjaxComCarregamento(
        '/Turma/ModalAlunoTurma',
        'GET',
        { alunoTurmaId },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_ModalAlunoTurma'));
                assineSalvarDadosDoAluno();
                $('#_ModalAlunoTurma').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function assineSalvarDadosDoAluno() {
    $('#_ModalAlunoTurma #btn-salvar-modal').on('click', function () {
        let modal = $('#_ModalAlunoTurma');
        let alunoTurma = {
            Id: modal.find('#AlunoTurmaId').val(),
            EntradaNaTurma: modal.find('#EntradaNaTurma').val(),
            SaidaDaTurma: modal.find('#SaidaDaTurma').val(),
            Situacao: modal.find('#Situacao').val()
        };

        if (!alunoTurma.EntradaNaTurma) {
            MensagemRodape('Informe a data de entrada para prosseguir.');
            return;
        }

        if (!alunoTurma.Situacao) {
            MensagemRodape('Informe a situação para prosseguir.');
            return;
        }

        RequisicaoAjaxComCarregamento(
            '/Turma/SalvarRegistroAlunoTurma',
            'POST',
            { alunoTurma },
            function (data) {
                if (data.sucesso) {
                    MensagemRodape('success', data.mensagem);
                    $('#_ModalAlunoTurma').modal('hide');
                    CarregarPagina('/Turma/Alterar/' + $('#formulario-turma #Id').val());
                } else {
                    MensagemRodape('warning', data.mensagem);
                }
            });
    });
}

function abrirModalHorarioDeAula(diaDaSemana) {
    RequisicaoAjaxComCarregamento(
        '/Turma/ModalHorarioDeAula',
        'GET',
        { turmaId: $('#formulario-turma #Id').val() , diaDaSemana },
        function (data) {
            if (data.sucesso) {
                $('#div-modal').html(data.html);
                assineMascarasDoComponente($('#_ModalHorarioDeAula'));
                $('#_ModalHorarioDeAula').modal('show');
            } else {
                MensagemRodape('warning', data.mensagem);
            }
        });
}

function assineChangeMesAnoDiario() {
    $('#MesAnoDiario').on('change', function () {
        $('#div-diario-classe').html('');
        if ($(this).val()) {
            RequisicaoAjaxComCarregamento(
                '/Turma/CarregarDiarioDeClasse',
                'GET',
                { turmaId: $('#formulario-turma #Id').val(), anoMes: $(this).val() },
                function (data) {
                    if (data.sucesso) {
                        $('#div-diario-classe').html(data.html);
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
        }
    });
}