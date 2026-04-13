import 'parada_rota_dto.dart';

class RotaExecucaoDTO {
  final int id;
  final int rotaId;
  final String descricao;
  final bool emAndamento;
  final List<ParadaRotaDTO> paradas;

  RotaExecucaoDTO({
    required this.id,
    required this.rotaId,
    required this.descricao,
    required this.emAndamento,
    required this.paradas,
  });

  factory RotaExecucaoDTO.fromJson(Map<String, dynamic> json) {
    return RotaExecucaoDTO(
      id: json['id'],
      rotaId: json['rotaId'],
      descricao: json['descricao'],
      emAndamento: json['emAndamento'],
      paradas: (json['paradas'] as List)
          .map((e) => ParadaRotaDTO.fromJson(e))
          .toList(),
    );
  }
}
