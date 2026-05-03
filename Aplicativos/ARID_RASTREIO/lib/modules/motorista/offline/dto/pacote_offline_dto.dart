import 'package:arid_rastreio/modules/motorista/checklist/store/checklist_item_store.dart';

class PacoteOfflineDTO {
  final DateTime dataHoraGeracao;
  final DateTime validoAte;
  final int validadeEmDias;
  final List<RotaOfflineDTO> rotas;

  PacoteOfflineDTO({
    required this.dataHoraGeracao,
    required this.validoAte,
    required this.validadeEmDias,
    required this.rotas,
  });

  factory PacoteOfflineDTO.fromJson(Map<String, dynamic> json) {
    return PacoteOfflineDTO(
      dataHoraGeracao: DateTime.parse(json['dataHoraGeracao']),
      validoAte: DateTime.parse(json['validoAte']),
      validadeEmDias: json['validadeEmDias'] ?? 3,
      rotas: ((json['rotas'] ?? []) as List)
          .map(
            (item) => RotaOfflineDTO.fromJson(Map<String, dynamic>.from(item)),
          )
          .toList(),
    );
  }
}

class RotaOfflineDTO {
  final int id;
  final String codigo;
  final String nome;
  final String descricao;
  final bool permitePausa;
  final int quantidadePausas;
  final int? unidadeOrigemId;
  final int? unidadeDestinoId;
  final String? nomeUnidadeOrigem;
  final String? nomeUnidadeDestino;
  final double? origemLatitudeRota;
  final double? origemLongitudeRota;
  final double? destinoLatitudeRota;
  final double? destinoLongitudeRota;
  final List<VeiculoOfflineDTO> veiculos;
  final List<ParadaOfflineDTO> paradas;

  RotaOfflineDTO({
    required this.id,
    required this.codigo,
    required this.nome,
    required this.descricao,
    required this.permitePausa,
    required this.quantidadePausas,
    this.unidadeOrigemId,
    this.unidadeDestinoId,
    this.nomeUnidadeOrigem,
    this.nomeUnidadeDestino,
    this.origemLatitudeRota,
    this.origemLongitudeRota,
    this.destinoLatitudeRota,
    this.destinoLongitudeRota,
    required this.veiculos,
    required this.paradas,
  });

  factory RotaOfflineDTO.fromJson(Map<String, dynamic> json) {
    return RotaOfflineDTO(
      id: json['id'],
      codigo: json['codigo'],
      nome: json['nome'],
      descricao: json['descricao'],
      permitePausa: json['permitePausa'] ?? false,
      quantidadePausas: json['quantidadePausas'] ?? 0,
      unidadeOrigemId: json['unidadeOrigemId'],
      unidadeDestinoId: json['unidadeDestinoId'],
      nomeUnidadeOrigem: json['nomeUnidadeOrigem'],
      nomeUnidadeDestino: json['nomeUnidadeDestino'],
      origemLatitudeRota: json['origemLatitudeRota'] != null
          ? double.tryParse(json['origemLatitudeRota'].toString())
          : null,
      origemLongitudeRota: json['origemLongitudeRota'] != null
          ? double.tryParse(json['origemLongitudeRota'].toString())
          : null,
      destinoLatitudeRota: json['destinoLatitudeRota'] != null
          ? double.tryParse(json['destinoLatitudeRota'].toString())
          : null,
      destinoLongitudeRota: json['destinoLongitudeRota'] != null
          ? double.tryParse(json['destinoLongitudeRota'].toString())
          : null,
      veiculos: ((json['veiculos'] ?? []) as List)
          .map(
            (item) =>
                VeiculoOfflineDTO.fromJson(Map<String, dynamic>.from(item)),
          )
          .toList(),
      paradas: ((json['paradas'] ?? []) as List)
          .map(
            (item) =>
                ParadaOfflineDTO.fromJson(Map<String, dynamic>.from(item)),
          )
          .toList(),
    );
  }
}

class VeiculoOfflineDTO {
  final int id;
  final int rotaId;
  final String nome;
  final String placa;
  final String modelo;
  final String cor;
  final List<ChecklistItemStore> checklist;

  VeiculoOfflineDTO({
    required this.id,
    required this.rotaId,
    required this.nome,
    required this.placa,
    required this.modelo,
    required this.cor,
    required this.checklist,
  });

  factory VeiculoOfflineDTO.fromJson(Map<String, dynamic> json) {
    return VeiculoOfflineDTO(
      id: json['id'],
      rotaId: json['rotaId'],
      nome: json['nome'],
      placa: json['placa'],
      modelo: json['modelo'],
      cor: json['cor'],
      checklist: ((json['checklist'] ?? []) as List)
          .map(
            (item) => ChecklistItemStoreMapper.fromJson(
              Map<String, dynamic>.from(item),
            ),
          )
          .toList(),
    );
  }
}

class ParadaOfflineDTO {
  final int id;
  final String endereco;
  final double? latitude;
  final double? longitude;
  final String? link;
  final String? observacaoCadastro;

  ParadaOfflineDTO({
    required this.id,
    required this.endereco,
    this.latitude,
    this.longitude,
    this.link,
    this.observacaoCadastro,
  });

  factory ParadaOfflineDTO.fromJson(Map<String, dynamic> json) {
    return ParadaOfflineDTO(
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
    );
  }
}
