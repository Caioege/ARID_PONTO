class UsuarioDTO {
  final int id;
  final String nome;
  final String login;
  final String? foto;
  final String? cpf;
  final DateTime? dataNascimento;
  final String? email;

  UsuarioDTO({
    required this.id,
    required this.nome,
    required this.login,
    this.foto,
    this.cpf,
    this.dataNascimento,
    this.email,
  });

  /// Constrói o usuário a partir de um JSON (API ou mock)
  factory UsuarioDTO.fromJson(Map<String, dynamic> json) {
    return UsuarioDTO(
      id: json['id'],
      nome: json['nome'],
      login: json['login'],
      foto: json['foto'],
      cpf: json['cpf'],
      email: json['email'],
      dataNascimento: json['dataNascimento'] != null
          ? DateTime.tryParse(json['dataNascimento'])
          : null,
    );
  }

  /// Converte o usuário para JSON
  /// Útil para salvar em cache/local storage
  Map<String, dynamic> toJson() => {
    'id': id,
    'nome': nome,
    'login': login,
    'foto': foto,
    'cpf': cpf,
    'email': email,
    'dataNascimento': dataNascimento?.toIso8601String(),
  };
}
