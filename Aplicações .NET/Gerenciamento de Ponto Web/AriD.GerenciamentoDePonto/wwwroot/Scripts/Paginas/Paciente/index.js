function aplicarFiltrosPaciente() {
    var adicional = JSON.parse($('#Adicional').val());
    adicional.Ativo = $('#FiltroAtivo').val() === '' ? null : $('#FiltroAtivo').val() === 'true';
    $('#Adicional').val(JSON.stringify(adicional));
    
    pesquisarNoGrid('grid', 'Paciente/TabelaPaginada', 'TermoDeBusca', 'Adicional');
}

function abrirModal(pacienteId) {
    BloquearTela();
    $.get('/Paciente/Modal', { pacienteId: pacienteId }, function (res) {
        DesbloquearTela();
        if (res.sucesso) {
            $('#div-modal').html(res.html);
            var modal = new bootstrap.Modal(document.getElementById('modal-container'));
            modal.show();
            
            // Aplicar máscaras se necessário (Assumindo que existem helpers globais)
            if (window.aplicarMascaras) window.aplicarMascaras();
        }
    });
}

function salvarPaciente() {
    if (!$('#form-paciente')[0].checkValidity()) {
        $('#form-paciente')[0].reportValidity();
        return;
    }

    BloquearTela();
    var formData = $('#form-paciente').serialize();
    $.post('/Paciente/Salvar', formData, function (res) {
        DesbloquearTela();
        if (res.sucesso) {
            NotificarSucesso(res.mensagem);
            bootstrap.Modal.getInstance(document.getElementById('modal-container')).hide();
            aplicarFiltrosPaciente();
        } else {
            NotificarErro(res.mensagem);
        }
    });
}

function removerRegistro(pacienteId) {
    Confirmar("Deseja realmente remover este paciente?", function () {
        BloquearTela();
        $.post('/Paciente/Remova', { pacienteId: pacienteId }, function (res) {
            DesbloquearTela();
            if (res.sucesso) {
                NotificarSucesso(res.mensagem);
                aplicarFiltrosPaciente();
            } else {
                NotificarErro(res.mensagem);
            }
        });
    });
}

$('.filter-trigger').on('change', function() {
    aplicarFiltrosPaciente();
});
