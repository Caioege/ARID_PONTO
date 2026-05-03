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
  final double? origemLatitudeRota;
  final double? origemLongitudeRota;
  final bool? origemEntregue;
  final String? origemObservacao;
  final String? origemConcluidaEm;
  final String? origemLatitude;
  final String? origemLongitude;
  final String? nomeUnidadeDestino;
  final double? destinoLatitudeRota;
  final double? destinoLongitudeRota;
  final bool? destinoEntregue;
  final String? destinoObservacao;
  final String? destinoConcluidoEm;
  final String? destinoLatitude;
  final String? destinoLongitude;
  final int? veiculoId;
  final int? checklistExecucaoId;
  final bool execucaoOffline;
  final String? localExecucaoId;
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
    this.origemLatitudeRota,
    this.origemLongitudeRota,
    this.origemEntregue,
    this.origemObservacao,
    this.origemConcluidaEm,
    this.origemLatitude,
    this.origemLongitude,
    this.nomeUnidadeDestino,
    this.destinoLatitudeRota,
    this.destinoLongitudeRota,
    this.destinoEntregue,
    this.destinoObservacao,
    this.destinoConcluidoEm,
    this.destinoLatitude,
    this.destinoLongitude,
    this.veiculoId,
    this.checklistExecucaoId,
    this.execucaoOffline = false,
    this.localExecucaoId,
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
      origemLatitudeRota: json['origemLatitudeRota'] != null
          ? double.tryParse(json['origemLatitudeRota'].toString())
          : null,
      origemLongitudeRota: json['origemLongitudeRota'] != null
          ? double.tryParse(json['origemLongitudeRota'].toString())
          : null,
      origemEntregue: json['origemEntregue'],
      origemObservacao: json['origemObservacao'],
      origemConcluidaEm: json['origemConcluidaEm'],
      origemLatitude: json['origemLatitude'],
      origemLongitude: json['origemLongitude'],
      nomeUnidadeDestino: json['nomeUnidadeDestino'],
      destinoLatitudeRota: json['destinoLatitudeRota'] != null
          ? double.tryParse(json['destinoLatitudeRota'].toString())
          : null,
      destinoLongitudeRota: json['destinoLongitudeRota'] != null
          ? double.tryParse(json['destinoLongitudeRota'].toString())
          : null,
      destinoEntregue: json['destinoEntregue'],
      destinoObservacao: json['destinoObservacao'],
      destinoConcluidoEm: json['destinoConcluidoEm'],
      destinoLatitude: json['destinoLatitude'],
      destinoLongitude: json['destinoLongitude'],
      veiculoId: json['veiculoId'],
      checklistExecucaoId: json['checklistExecucaoId'],
      execucaoOffline: json['execucaoOffline'] ?? false,
      localExecucaoId: json['localExecucaoId'],
      paradas: (json['paradas'] as List)
          .map((e) => ParadaRotaDTO.fromJson(e))
          .toList(),
    );
  }
}
