class ParadaRotaDTO {
  final int id;
  final String endereco;
  final double? latitude;
  final double? longitude;
  final String? link;
  final String? observacaoCadastro;
  bool? entregue;
  String? observacao;
  final String? concluidoEm;
  final double? latitudeConfirmacao;
  final double? longitudeConfirmacao;

  ParadaRotaDTO({
    required this.id,
    required this.endereco,
    this.latitude,
    this.longitude,
    this.link,
    this.observacaoCadastro,
    this.entregue,
    this.observacao,
    this.concluidoEm,
    this.latitudeConfirmacao,
    this.longitudeConfirmacao,
  });

  factory ParadaRotaDTO.fromJson(Map<String, dynamic> json) {
    return ParadaRotaDTO(
      id: json['id'],
      endereco: json['endereco'],
      latitude: json['latitude'] != null
          ? double.tryParse(json['latitude'].toString())
          : null,
      longitude: json['longitude'] != null
          ? double.tryParse(json['longitude'].toString())
          : null,

      link: json['link'],
      observacaoCadastro: json['observacaoCadastro'],
      entregue: json['entregue'],
      observacao: json['observacao'],
      concluidoEm: json['concluidoEm'],
      latitudeConfirmacao: json['latitudeConfirmacao'] != null
          ? double.tryParse(json['latitudeConfirmacao'].toString())
          : null,
      longitudeConfirmacao: json['longitudeConfirmacao'] != null
          ? double.tryParse(json['longitudeConfirmacao'].toString())
          : null,
    );
  }

  Map<String, dynamic> toJsonEncerramento() {
    return {'paradaId': id, 'entregue': entregue, 'observacao': observacao};
  }
}
