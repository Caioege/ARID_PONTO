import 'parada_rota_dto.dart';

class RotaExecucaoDTO {
  final int id;
  final int rotaId;
  final String descricao;
  final bool emAndamento;
  final bool permitePausa;
  final int quantidadePausas;
  final int quantidadePausasRealizadas;
  final bool estaPausada;
  final String? nomeUnidadeOrigem;
  final bool? origemEntregue;
  final String? origemObservacao;
  final String? nomeUnidadeDestino;
  final bool? destinoEntregue;
  final String? destinoObservacao;
  final List<ParadaRotaDTO> paradas;

  RotaExecucaoDTO({
    required this.id,
    required this.rotaId,
    required this.descricao,
    required this.emAndamento,
    this.permitePausa = false,
    this.quantidadePausas = 0,
    this.quantidadePausasRealizadas = 0,
    this.estaPausada = false,
    this.nomeUnidadeOrigem,
    this.origemEntregue,
    this.origemObservacao,
    this.nomeUnidadeDestino,
    this.destinoEntregue,
    this.destinoObservacao,
    required this.paradas,
  });

  factory RotaExecucaoDTO.fromJson(Map<String, dynamic> json) {
    return RotaExecucaoDTO(
      id: json['id'],
      rotaId: json['rotaId'],
      descricao: json['descricao'],
      emAndamento: json['emAndamento'],
      permitePausa: json['permitePausa'] ?? false,
      quantidadePausas: json['quantidadePausas'] ?? 0,
      quantidadePausasRealizadas: json['quantidadePausasRealizadas'] ?? 0,
      estaPausada: json['estaPausada'] ?? false,
      nomeUnidadeOrigem: json['nomeUnidadeOrigem'],
      origemEntregue: json['origemEntregue'],
      origemObservacao: json['origemObservacao'],
      nomeUnidadeDestino: json['nomeUnidadeDestino'],
      destinoEntregue: json['destinoEntregue'],
      destinoObservacao: json['destinoObservacao'],
      paradas: (json['paradas'] as List)
          .map((e) => ParadaRotaDTO.fromJson(e))
          .toList(),
    );
  }
}
