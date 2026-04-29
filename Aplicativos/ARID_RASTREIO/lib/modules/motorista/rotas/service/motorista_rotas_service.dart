import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/encerrar_parada_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:arid_rastreio/core/http/http_client.dart';
import '../dto/parada_rota_dto.dart';

class MotoristaRotasService {
  final _client = locator<AppHttpClient>().dio;

  bool get _usarMock => dotenv.env['USE_FAKE_LOGIN']?.toLowerCase() == 'true';

  Future<RotaExecucaoDTO> iniciarRota({
    required int rotaId,
    required int veiculoId,
    int? checklistExecucaoId,
    double? latitudeInicio,
    double? longitudeInicio,
    bool gpsSimulado = false,
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
      '/api/rastreio-app/rotas/iniciar',
      data: {
        'rotaId': rotaId,
        'veiculoId': veiculoId,
        'checklistExecucaoId': checklistExecucaoId,
        if (latitudeInicio != null) 'latitudeInicio': latitudeInicio.toStringAsFixed(6),
        if (longitudeInicio != null) 'longitudeInicio': longitudeInicio.toStringAsFixed(6),
        'gpsSimulado': gpsSimulado,
      },
    );

    return RotaExecucaoDTO.fromJson(response.data);
  }

  Future<RotaExecucaoDTO?> obterRotaEmAndamento() async {
    if (_usarMock) {
      return null;
    }

    final response = await _client.get('/api/rastreio-app/rotas/em-andamento');

    if (response.data == null || response.data == '') return null;

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
      '/api/rastreio-app/rotas/encerrar',
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
    double? latitude,
    double? longitude,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 500));
      return;
    }

    await _client.post(
      '/api/rastreio-app/rotas/confirmar-parada',
      data: {
        'rotaExecucaoId': rotaExecucaoId,
        'paradaId': paradaId,
        'entregue': entregue,
        'observacao': observacao,
        if (latitude != null) 'latitude': latitude.toStringAsFixed(6),
        if (longitude != null) 'longitude': longitude.toStringAsFixed(6),
      },
    );
  }

  Future<void> iniciarPausa({
    required int rotaExecucaoId,
    required String motivo,
    required double? latitude,
    required double? longitude,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 500));
      return;
    }

    await _client.post(
      '/api/rastreio-app/rotas/fazer-pausa',
      data: {
        'rotaExecucaoId': rotaExecucaoId,
        'motivo': motivo,
        'latitude': latitude?.toStringAsFixed(6),
        'longitude': longitude?.toStringAsFixed(6),
        'dataHora': DateTime.now().toIso8601String(),
      },
    );
  }

  Future<void> finalizarPausa({
    required int rotaExecucaoId,
    required double? latitude,
    required double? longitude,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 500));
      return;
    }

    await _client.post(
      '/api/rastreio-app/rotas/finalizar-pausa',
      data: {
        'rotaExecucaoId': rotaExecucaoId,
        'latitude': latitude?.toStringAsFixed(6),
        'longitude': longitude?.toStringAsFixed(6),
        'dataHora': DateTime.now().toIso8601String(),
      },
    );
  }

  Future<void> salvarPontoDaRotaBackground({
    required int rotaExecucaoId,
    required double latitude,
    required double longitude,
    required DateTime dataHora,
    bool? gpsSimulado,
    double? precisaoEmMetros,
    double? velocidadeMetrosPorSegundo,
    double? direcaoGraus,
    double? altitudeMetros,
    int? fonteCaptura,
  }) async {
    if (_usarMock) {
      await Future.delayed(const Duration(milliseconds: 200));
      print(
        '[MOCK] Ponto salvo: $latitude, $longitude em $dataHora (rota $rotaExecucaoId)',
      );
      return;
    }

    await _client.post(
      '/api/rastreio-app/rotas/salvar-ponto',
      data: {
        'rotaExecucaoId': rotaExecucaoId,
        'latitude': latitude.toStringAsFixed(6),
        'longitude': longitude.toStringAsFixed(6),
        'dataHora': dataHora.toIso8601String(),
        'gpsSimulado': gpsSimulado ?? false,
        if (precisaoEmMetros != null) 'precisaoEmMetros': precisaoEmMetros,
        if (velocidadeMetrosPorSegundo != null) 'velocidadeMetrosPorSegundo': velocidadeMetrosPorSegundo,
        if (direcaoGraus != null) 'direcaoGraus': direcaoGraus,
        if (altitudeMetros != null) 'altitudeMetros': altitudeMetros,
        if (fonteCaptura != null) 'fonteCaptura': fonteCaptura,
      },
    );
  }
}

