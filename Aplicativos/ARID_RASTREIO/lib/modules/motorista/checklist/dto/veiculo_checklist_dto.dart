import 'package:arid_rastreio/modules/motorista/checklist/store/checklist_item_store.dart';

class VeiculoChecklistDTO {
  final int id;
  final int rotaId;
  final String nome;
  final String placa;
  final String modelo;
  final String cor;
  final List<ChecklistItemStore> checklist;

  VeiculoChecklistDTO({
    required this.id,
    required this.rotaId,
    required this.nome,
    required this.placa,
    required this.modelo,
    required this.cor,
    required this.checklist,
  });

  factory VeiculoChecklistDTO.fromJson(Map<String, dynamic> json) {
    return VeiculoChecklistDTO(
      id: json['id'],
      rotaId: json['rotaId'],
      nome: json['nome'],
      placa: json['placa'],
      modelo: json['modelo'],
      cor: json['cor'],
      checklist: (json['checklist'] as List)
          .map(
            (item) => ChecklistItemStoreMapper.fromJson(
              Map<String, dynamic>.from(item),
            ),
          )
          .toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'rotaId': rotaId,
      'nome': nome,
      'placa': placa,
      'modelo': modelo,
      'cor': cor,
      'checklist': checklist.map((item) => item.toJson()).toList(),
    };
  }
}
