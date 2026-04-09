import 'package:arid_motorista/ioc/service_locator.dart';
import 'package:arid_motorista/modules/motorista/rotas/dto/encerrar_parada_dto.dart';
import 'package:arid_motorista/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:arid_motorista/core/http/http_client.dart';
import '../dto/parada_rota_dto.dart';

class MotoristaRotasService {
  final _client = locator<AppHttpClient>().dio;

  bool get _usarMock => dotenv.env['USE_FAKE_LOGIN']?.toLowerCase() == 'true';

  Future<RotaExecucaoDTO> iniciarRota({
    required int rotaId,
    required int veiculoId,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 600));

      return RotaExecucaoDTO(
        id: DateTime.now().millisecondsSinceEpoch, // simula execução
        rotaId: rotaId,
        descricao: 'Rota Manhã',
        emAndamento: true,
        paradas: [
          ParadaRotaDTO(
            id: 1,
            endereco: 'Rua A, 123',
            latitude: -16.7069,
            longitude: -49.2364,
            link: 'https://maps.google.com',
          ),
          ParadaRotaDTO(
            id: 2,
            endereco: 'Rua B, 456',
            latitude: -16.7080,
            longitude: -49.2382,
          ),
        ],
      );
    }

    final response = await _client.post(
      '/api/rota-app/rotas/iniciar',
      data: {'rotaId': rotaId, 'veiculoId': veiculoId},
    );

    return RotaExecucaoDTO.fromJson(response.data);
  }

  Future<void> encerrarRota({
    required int rotaExecucaoId,
    required List<EncerrarParadaDTO> paradas,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 500));
      return;
    }

    await _client.post(
      '/api/rota-app/rotas/encerrar',
      data: {
        'rotaExecucaoId': rotaExecucaoId,
        'paradas': paradas.map((p) => p.toJson()).toList(),
      },
    );
  }

  Future<void> confirmarParada({
    required int rotaExecucaoId,
    required int paradaId,
    required bool? entregue,
    required String? observacao,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 500));
      return;
    }

    await _client.post(
      '/api/rota-app/rotas/confirmar-parada',
      data: {
        'rotaExecucaoId': rotaExecucaoId,
        'paradaId': paradaId,
        'entregue': entregue,
        'observacao': observacao,
      },
    );
  }

  Future<void> salvarPontoDaRotaBackground({
    required int rotaExecucaoId,
    required double latitude,
    required double longitude,
    required DateTime dataHora,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 200));
      print(
        '[MOCK] Ponto salvo: $latitude, $longitude em $dataHora (rota $rotaExecucaoId)',
      );
      return;
    }

    await _client.post(
      '/api/rota-app/rotas/salvar-ponto',
      data: {
        'rotaExecucaoId': rotaExecucaoId,
        'latitude': latitude.toStringAsFixed(6),
        'longitude': longitude.toStringAsFixed(6),
        'dataHora': dataHora.toIso8601String(),
      },
    );
  }
}
