import 'parada_rota_dto.dart';

class RotaDTO {
  final int id;
  final String descricao;
  final String veiculo;
  final bool emAndamento;
  final List<ParadaRotaDTO> paradas;

  RotaDTO({
    required this.id,
    required this.descricao,
    required this.veiculo,
    required this.emAndamento,
    required this.paradas,
  });

  factory RotaDTO.fromJson(Map<String, dynamic> json) {
    return RotaDTO(
      id: json['id'],
      descricao: json['descricao'],
      veiculo: json['veiculo'],
      emAndamento: json['emAndamento'] ?? false,
      paradas: (json['paradas'] as List)
          .map((e) => ParadaRotaDTO.fromJson(e))
          .toList(),
    );
  }
}
