class RotaChecklistDTO {
  final int id;
  final String codigo;
  final String nome;
  final String descricao;
  final bool rotaFinalizada;

  RotaChecklistDTO({
    required this.id,
    required this.codigo,
    required this.nome,
    required this.descricao,
    this.rotaFinalizada = false,
  });

  factory RotaChecklistDTO.fromJson(Map<String, dynamic> json) {
    return RotaChecklistDTO(
      id: json['id'],
      codigo: json['codigo'],
      nome: json['nome'],
      descricao: json['descricao'],
      rotaFinalizada: json['rotaFinalizada'] ?? false,
    );
  }
}
