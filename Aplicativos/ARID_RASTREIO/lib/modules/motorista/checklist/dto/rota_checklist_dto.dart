class RotaChecklistDTO {
  final int id;
  final String codigo;
  final String nome;
  final String descricao;
  final bool rotaFinalizada;
  final int? rotaExecucaoFinalizadaId;
  final bool permiteIniciarSemPacienteAcompanhante;
  final bool permiteIniciarSemProfissional;
  final List<RotaPacienteDTO> pacientes;
  final List<RotaProfissionalDTO> profissionais;
  final List<PacienteDisponivelDTO> pacientesDisponiveis;

  RotaChecklistDTO({
    required this.id,
    required this.codigo,
    required this.nome,
    required this.descricao,
    this.rotaFinalizada = false,
    this.rotaExecucaoFinalizadaId,
    this.permiteIniciarSemPacienteAcompanhante = true,
    this.permiteIniciarSemProfissional = true,
    this.pacientes = const [],
    this.profissionais = const [],
    this.pacientesDisponiveis = const [],
  });

  factory RotaChecklistDTO.fromJson(Map<String, dynamic> json) {
    return RotaChecklistDTO(
      id: json['id'],
      codigo: json['codigo'],
      nome: json['nome'],
      descricao: json['descricao'],
      rotaFinalizada: json['rotaFinalizada'] ?? false,
      rotaExecucaoFinalizadaId: json['rotaExecucaoFinalizadaId'],
      permiteIniciarSemPacienteAcompanhante:
          json['permiteIniciarSemPacienteAcompanhante'] ?? true,
      permiteIniciarSemProfissional:
          json['permiteIniciarSemProfissional'] ?? true,
      pacientes: ((json['pacientes'] ?? []) as List)
          .map((e) => RotaPacienteDTO.fromJson(Map<String, dynamic>.from(e)))
          .toList(),
      profissionais: ((json['profissionais'] ?? []) as List)
          .map(
            (e) => RotaProfissionalDTO.fromJson(Map<String, dynamic>.from(e)),
          )
          .toList(),
      pacientesDisponiveis: ((json['pacientesDisponiveis'] ?? []) as List)
          .map(
            (e) => PacienteDisponivelDTO.fromJson(Map<String, dynamic>.from(e)),
          )
          .toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'codigo': codigo,
      'nome': nome,
      'descricao': descricao,
      'rotaFinalizada': rotaFinalizada,
      'rotaExecucaoFinalizadaId': rotaExecucaoFinalizadaId,
      'permiteIniciarSemPacienteAcompanhante':
          permiteIniciarSemPacienteAcompanhante,
      'permiteIniciarSemProfissional': permiteIniciarSemProfissional,
      'pacientes': pacientes.map((p) => p.toJson()).toList(),
      'profissionais': profissionais.map((p) => p.toJson()).toList(),
      'pacientesDisponiveis': pacientesDisponiveis
          .map((p) => p.toJson())
          .toList(),
    };
  }
}

class RotaPacienteDTO {
  final int pacienteId;
  final String nome;
  final String? cpf;
  final String? telefone;
  final bool possuiAcompanhante;
  final String? acompanhanteNome;
  final String? acompanhanteCPF;

  const RotaPacienteDTO({
    required this.pacienteId,
    required this.nome,
    this.cpf,
    this.telefone,
    this.possuiAcompanhante = false,
    this.acompanhanteNome,
    this.acompanhanteCPF,
  });

  factory RotaPacienteDTO.fromJson(Map<String, dynamic> json) {
    return RotaPacienteDTO(
      pacienteId: json['pacienteId'],
      nome: json['nome'] ?? '',
      cpf: json['cpf'] ?? json['CPF'],
      telefone: json['telefone'],
      possuiAcompanhante: json['possuiAcompanhante'] ?? false,
      acompanhanteNome: json['acompanhanteNome'],
      acompanhanteCPF: json['acompanhanteCPF'] ?? json['acompanhanteCpf'],
    );
  }

  Map<String, dynamic> toJson() => {
    'pacienteId': pacienteId,
    'nome': nome,
    'cpf': cpf,
    'telefone': telefone,
    'possuiAcompanhante': possuiAcompanhante,
    'acompanhanteNome': acompanhanteNome,
    'acompanhanteCPF': acompanhanteCPF,
  };
}

class PacienteDisponivelDTO {
  final int pacienteId;
  final String nome;
  final String? cpf;
  final String? telefone;
  final String? acompanhanteNome;
  final String? acompanhanteCPF;

  const PacienteDisponivelDTO({
    required this.pacienteId,
    required this.nome,
    this.cpf,
    this.telefone,
    this.acompanhanteNome,
    this.acompanhanteCPF,
  });

  factory PacienteDisponivelDTO.fromJson(Map<String, dynamic> json) {
    return PacienteDisponivelDTO(
      pacienteId: json['pacienteId'],
      nome: json['nome'] ?? '',
      cpf: json['cpf'] ?? json['CPF'],
      telefone: json['telefone'],
      acompanhanteNome: json['acompanhanteNome'],
      acompanhanteCPF: json['acompanhanteCPF'] ?? json['acompanhanteCpf'],
    );
  }

  Map<String, dynamic> toJson() => {
    'pacienteId': pacienteId,
    'nome': nome,
    'cpf': cpf,
    'telefone': telefone,
    'acompanhanteNome': acompanhanteNome,
    'acompanhanteCPF': acompanhanteCPF,
  };
}

class RotaProfissionalDTO {
  final int servidorId;
  final String nome;
  final String? funcao;
  final String? observacao;

  const RotaProfissionalDTO({
    required this.servidorId,
    required this.nome,
    this.funcao,
    this.observacao,
  });

  factory RotaProfissionalDTO.fromJson(Map<String, dynamic> json) {
    return RotaProfissionalDTO(
      servidorId: json['servidorId'],
      nome: json['nome'] ?? '',
      funcao: json['funcao'],
      observacao: json['observacao'],
    );
  }

  Map<String, dynamic> toJson() => {
    'servidorId': servidorId,
    'nome': nome,
    'funcao': funcao,
    'observacao': observacao,
  };
}
