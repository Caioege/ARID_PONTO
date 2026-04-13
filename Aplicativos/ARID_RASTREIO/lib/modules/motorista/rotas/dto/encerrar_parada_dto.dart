class EncerrarParadaDTO {
  final int id;
  final bool? entregue;
  final String observacao;

  EncerrarParadaDTO({
    required this.id,
    required this.entregue,
    required this.observacao,
  });

  Map<String, dynamic> toJson() {
    return {'id': id, 'entregue': entregue, 'observacao': observacao};
  }
}
