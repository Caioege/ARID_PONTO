import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/core/network/connectivity_service.dart';
import 'package:arid_rastreio/modules/motorista/checklist/store/checklist_item_store.dart';
import 'package:arid_rastreio/modules/motorista/offline/service/offline_rastreio_service.dart';
import 'package:dio/dio.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:arid_rastreio/core/http/http_client.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';

class ChecklistService {
  final _client = locator<AppHttpClient>().dio;
  final _offlineService = locator<OfflineRastreioService>();

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

    if (!await ConnectivityService.isConnected()) {
      return _offlineService.listarRotasCache();
    }

    try {
      final response = await _client.get('/api/rastreio-app/rotas');

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
      final cache = await _offlineService.listarRotasCache();
      if (cache.isNotEmpty) return cache;
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

    if (!await ConnectivityService.isConnected()) {
      return _offlineService.listarVeiculosCache(rotaId);
    }

    try {
      final response = await _client.get(
        '/api/rastreio-app/veiculos',
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
      final cache = await _offlineService.listarVeiculosCache(rotaId);
      if (cache.isNotEmpty) return cache;
      rethrow;
    }
  }

  Future<int> salvarChecklist({
    required int rotaId,
    required int veiculoId,
    required List<int> itensMarcados,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 600));
      return 1;
    }

    if (!await ConnectivityService.isConnected()) {
      return _offlineService.salvarChecklistLocal(
        rotaId: rotaId,
        veiculoId: veiculoId,
        itensMarcados: itensMarcados,
      );
    }

    try {
      final response = await _client.post(
        '/api/rastreio-app/checklist',
        data: {
          'rotaId': rotaId,
          'veiculoId': veiculoId,
          'itens': itensMarcados,
        },
      );

      return response.data['data'] as int;
    } on DioException {
      if (await _offlineService.podeIniciarRotaOffline()) {
        return _offlineService.salvarChecklistLocal(
          rotaId: rotaId,
          veiculoId: veiculoId,
          itensMarcados: itensMarcados,
        );
      }
      rethrow;
    }
  }
}
