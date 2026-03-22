class ParadaRotaDTO {
  final int id;
  final String endereco;
  final double? latitude;
  final double? longitude;
  final String? link;
  bool? entregue;
  String? observacao;

  ParadaRotaDTO({
    required this.id,
    required this.endereco,
    this.latitude,
    this.longitude,
    this.link,
    this.entregue,
    this.observacao,
  });

  factory ParadaRotaDTO.fromJson(Map<String, dynamic> json) {
    return ParadaRotaDTO(
      id: json['id'],
      endereco: json['endereco'],
      latitude: json['latitude'] != null
          ? (json['latitude'] as num).toDouble()
          : null,
      longitude: json['longitude'] != null
          ? (json['longitude'] as num).toDouble()
          : null,

      link: json['link'],
      entregue: json['entregue'],
      observacao: json['observacao'],
    );
  }

  Map<String, dynamic> toJsonEncerramento() {
    return {'paradaId': id, 'entregue': entregue, 'observacao': observacao};
  }
}
