import 'dart:convert';

import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:shared_preferences/shared_preferences.dart';

class RotaSessaoCacheDTO {
  final RotaExecucaoDTO execucao;
  final RotaChecklistDTO? rotaChecklist;
  final VeiculoChecklistDTO? veiculoChecklist;
  final int? checklistExecucaoId;
  final List<int> itensMarcados;

  RotaSessaoCacheDTO({
    required this.execucao,
    this.rotaChecklist,
    this.veiculoChecklist,
    this.checklistExecucaoId,
    this.itensMarcados = const [],
  });
}

class RotaSessaoCacheService {
  static const _key = 'rota_sessao_cache_v1';

  Future<void> salvar({
    required RotaExecucaoDTO execucao,
    RotaChecklistDTO? rotaChecklist,
    VeiculoChecklistDTO? veiculoChecklist,
    int? checklistExecucaoId,
  }) async {
    final prefs = await SharedPreferences.getInstance();
    final itensMarcados =
        veiculoChecklist?.checklist
            .where((item) => item.checked)
            .map((item) => item.id)
            .toList() ??
        <int>[];

    await prefs.setString(
      _key,
      jsonEncode({
        'execucao': execucao.toJson(),
        'rotaChecklist': rotaChecklist?.toJson(),
        'veiculoChecklist': veiculoChecklist?.toJson(),
        'checklistExecucaoId': checklistExecucaoId,
        'itensMarcados': itensMarcados,
        'salvoEm': DateTime.now().toIso8601String(),
      }),
    );
  }

  Future<RotaSessaoCacheDTO?> obter() async {
    final prefs = await SharedPreferences.getInstance();
    final raw = prefs.getString(_key);
    if (raw == null || raw.isEmpty) return null;

    try {
      final json = Map<String, dynamic>.from(jsonDecode(raw));
      final execucao = RotaExecucaoDTO.fromJson(
        Map<String, dynamic>.from(json['execucao']),
      );
      final rotaJson = json['rotaChecklist'];
      final veiculoJson = json['veiculoChecklist'];

      return RotaSessaoCacheDTO(
        execucao: execucao,
        rotaChecklist: rotaJson == null
            ? null
            : RotaChecklistDTO.fromJson(Map<String, dynamic>.from(rotaJson)),
        veiculoChecklist: veiculoJson == null
            ? null
            : VeiculoChecklistDTO.fromJson(
                Map<String, dynamic>.from(veiculoJson),
              ),
        checklistExecucaoId: json['checklistExecucaoId'],
        itensMarcados: ((json['itensMarcados'] ?? []) as List)
            .map((item) => int.tryParse(item.toString()))
            .whereType<int>()
            .toList(),
      );
    } catch (_) {
      await limpar();
      return null;
    }
  }

  Future<void> limpar() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_key);
  }
}
