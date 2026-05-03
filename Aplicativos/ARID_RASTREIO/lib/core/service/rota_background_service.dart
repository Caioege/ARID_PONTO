import 'package:arid_rastreio/core/network/connectivity_service.dart';
import 'package:arid_rastreio/modules/motorista/offline/service/offline_rastreio_service.dart';
import 'package:arid_rastreio/modules/motorista/rotas/service/motorista_rotas_service.dart';

class RotaBackgroundService {
  final MotoristaRotasService _service;
  final OfflineRastreioService _offlineService;

  RotaBackgroundService(this._service, this._offlineService);

  Future<void> enviarPonto({
    required int? rotaExecucaoId,
    String? localExecucaoId,
    bool execucaoOffline = false,
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
    if (execucaoOffline) {
      if (localExecucaoId == null || localExecucaoId.isEmpty) {
        throw Exception('Execucao offline sem localExecucaoId.');
      }

      await _offlineService.registrarLocalizacaoLocal(
        localExecucaoId: localExecucaoId,
        latitude: latitude,
        longitude: longitude,
        dataHora: dataHora,
        gpsSimulado: gpsSimulado,
        precisaoEmMetros: precisaoEmMetros,
        velocidadeMetrosPorSegundo: velocidadeMetrosPorSegundo,
        direcaoGraus: direcaoGraus,
        altitudeMetros: altitudeMetros,
        fonteCaptura: fonteCaptura,
      );
      return;
    }

    if (rotaExecucaoId == null) {
      throw Exception('Execucao online sem RotaExecucaoId.');
    }

    if (!await ConnectivityService.isConnected()) {
      throw Exception('Sem conexao para enviar ponto online.');
    }

    await _service.salvarPontoDaRotaBackground(
      rotaExecucaoId: rotaExecucaoId,
      latitude: latitude,
      longitude: longitude,
      dataHora: dataHora,
      gpsSimulado: gpsSimulado,
      precisaoEmMetros: precisaoEmMetros,
      velocidadeMetrosPorSegundo: velocidadeMetrosPorSegundo,
      direcaoGraus: direcaoGraus,
      altitudeMetros: altitudeMetros,
      fonteCaptura: fonteCaptura,
    );
  }
}
