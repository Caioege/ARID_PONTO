import 'package:arid_motorista/ioc/service_locator.dart';
import 'package:arid_motorista/modules/motorista/checklist/store/checklist_item_store.dart';
import 'package:dio/dio.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:arid_motorista/core/http/http_client.dart';
import 'package:arid_motorista/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_motorista/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';

class ChecklistService {
  final _client = locator<AppHttpClient>().dio;

  bool get _usarMock => dotenv.env['USE_FAKE_LOGIN']?.toLowerCase() == 'true';

  Future<List<RotaChecklistDTO>> obterRotas() async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 600));

      return [
        RotaChecklistDTO(
          id: 1,
          codigo: 'RT-001',
          nome: 'Rota Centro',
          descricao: 'Entregas na região central',
        ),
        RotaChecklistDTO(
          id: 2,
          codigo: 'RT-002',
          nome: 'Rota Norte',
          descricao: 'Entregas zona norte',
        ),
      ];
    }

    try {
      final response = await _client.get('/api/rota-app/rotas');

      final retorno = <RotaChecklistDTO>[];
      final body = response.data;

      if (body is Map && body['data'] is List) {
        for (final item in body['data']) {
          retorno.add(
            RotaChecklistDTO.fromJson(Map<String, dynamic>.from(item)),
          );
        }
      }

      return retorno;
    } on DioException {
      rethrow;
    }
  }

  Future<List<VeiculoChecklistDTO>> obterVeiculosPorRota(int rotaId) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 600));

      final checklistCarro = [
        ChecklistItemStore(id: 1, descricao: 'Faróis funcionando'),
        ChecklistItemStore(id: 2, descricao: 'Lanternas traseiras'),
        ChecklistItemStore(id: 3, descricao: 'Setas'),
        ChecklistItemStore(id: 4, descricao: 'Freio de mão'),
        ChecklistItemStore(id: 5, descricao: 'Cinto de segurança'),
      ];

      final checklistMoto = [
        ChecklistItemStore(id: 6, descricao: 'Farol dianteiro'),
        ChecklistItemStore(id: 7, descricao: 'Luz de freio'),
        ChecklistItemStore(id: 8, descricao: 'Setas'),
        ChecklistItemStore(id: 9, descricao: 'Freios'),
      ];

      final todos = [
        VeiculoChecklistDTO(
          id: 1,
          rotaId: 1,
          nome: 'Carro 01',
          placa: 'ABC-1234',
          modelo: 'Fiat Uno',
          cor: 'Branco',
          checklist: checklistCarro,
        ),
        VeiculoChecklistDTO(
          id: 2,
          rotaId: 1,
          nome: 'Moto 01',
          placa: 'XYZ-9876',
          modelo: 'CG 160',
          cor: 'Preta',
          checklist: checklistMoto,
        ),
        VeiculoChecklistDTO(
          id: 3,
          rotaId: 2,
          nome: 'Carro 02',
          placa: 'DEF-5678',
          modelo: 'Onix',
          cor: 'Prata',
          checklist: checklistCarro,
        ),
      ];

      return todos.where((v) => v.rotaId == rotaId).toList();
    }

    try {
      final response = await _client.get(
        '/api/rota-app/veiculos',
        queryParameters: {'rotaId': rotaId},
      );

      final retorno = <VeiculoChecklistDTO>[];
      final body = response.data;

      if (body is Map && body['data'] is List) {
        for (final item in body['data']) {
          retorno.add(
            VeiculoChecklistDTO.fromJson(Map<String, dynamic>.from(item)),
          );
        }
      }

      return retorno;
    } on DioException {
      rethrow;
    }
  }

  Future<void> salvarChecklist({
    required int rotaId,
    required int veiculoId,
    required List<int> itensMarcados,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 600));
      return;
    }

    try {
      await _client.post(
        '/api/rota-app/checklist',
        data: {
          'rotaId': rotaId,
          'veiculoId': veiculoId,
          'itens': itensMarcados,
        },
      );
    } on DioException {
      rethrow;
    }
  }
}
