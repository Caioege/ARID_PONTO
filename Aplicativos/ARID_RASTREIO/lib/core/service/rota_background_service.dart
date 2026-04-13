import 'package:arid_rastreio/modules/motorista/rotas/service/motorista_rotas_service.dart';

class RotaBackgroundService {
  final MotoristaRotasService _service;

  RotaBackgroundService(this._service);

  Future<void> enviarPonto({
    required int rotaExecucaoId,
    required double latitude,
    required double longitude,
    required DateTime dataHora,
  }) async {
    await _service.salvarPontoDaRotaBackground(
      rotaExecucaoId: rotaExecucaoId,
      latitude: latitude,
      longitude: longitude,
      dataHora: dataHora,
    );
  }
}

