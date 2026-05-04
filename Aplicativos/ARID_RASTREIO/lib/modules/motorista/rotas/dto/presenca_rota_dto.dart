class PresencaPacienteRotaDTO {
  final int? pacienteId;
  final String? nome;
  final String? cpf;
  final String? telefone;
  final bool presente;
  final bool possuiAcompanhante;
  final bool acompanhantePresente;
  final String? acompanhanteNome;
  final String? acompanhanteCPF;
  final bool novoPaciente;
  final bool incluirNaRota;

  const PresencaPacienteRotaDTO({
    this.pacienteId,
    this.nome,
    this.cpf,
    this.telefone,
    required this.presente,
    this.possuiAcompanhante = false,
    this.acompanhantePresente = false,
    this.acompanhanteNome,
    this.acompanhanteCPF,
    this.novoPaciente = false,
    this.incluirNaRota = true,
  });

  Map<String, dynamic> toJson() => {
    'pacienteId': pacienteId,
    'nome': nome,
    'cpf': cpf,
    'telefone': telefone,
    'presente': presente,
    'possuiAcompanhante': possuiAcompanhante,
    'acompanhantePresente': acompanhantePresente,
    'acompanhanteNome': acompanhanteNome,
    'acompanhanteCPF': acompanhanteCPF,
    'novoPaciente': novoPaciente,
    'incluirNaRota': incluirNaRota,
  };
}

class PresencaProfissionalRotaDTO {
  final int servidorId;
  final String? nome;
  final bool presente;

  const PresencaProfissionalRotaDTO({
    required this.servidorId,
    this.nome,
    required this.presente,
  });

  Map<String, dynamic> toJson() => {
    'servidorId': servidorId,
    'nome': nome,
    'presente': presente,
  };
}
