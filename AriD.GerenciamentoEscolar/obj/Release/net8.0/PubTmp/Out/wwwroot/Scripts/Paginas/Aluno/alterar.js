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

var removerAluno = function (id) {
    Swal.fire({
        text: "Tem certeza que deseja remover esse aluno?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "SIM",
        cancelButtonText: 'NÃO'
    }).then((result) => {
        if (result.isConfirmed) {
            RequisicaoAjaxComCarregamento(
                '/Aluno/Remover',
                'POST',
                { id },
                function (data) {
                    if (data.sucesso) {
                        MensagemRodape('success', data.mensagem);
                        CarregarPagina('/Aluno/Index');
                    } else {
                        MensagemRodape('warning', data.mensagem);
                    }
                });
        }
    });
}