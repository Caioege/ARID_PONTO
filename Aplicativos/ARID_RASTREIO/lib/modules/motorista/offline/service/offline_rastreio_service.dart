import 'package:arid_rastreio/core/http/http_client.dart';
import 'package:arid_rastreio/core/network/connectivity_service.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/offline/dto/pacote_offline_dto.dart';
import 'package:arid_rastreio/modules/motorista/offline/repository/offline_rastreio_repository.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/presenca_rota_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';

class OfflineRastreioService {
  final _client = locator<AppHttpClient>().dio;
  final OfflineRastreioRepository _repository;

  OfflineRastreioService(this._repository);

  Future<PacoteOfflineDTO> baixarESalvarPacote() async {
    await ConnectivityService.ensureConnected();

    final response = await _client.get('/api/rastreio-app/offline/pacote');
    final pacote = PacoteOfflineDTO.fromJson(
      Map<String, dynamic>.from(response.data),
    );

    await _repository.salvarPacote(pacote);
    return pacote;
  }

  Future<void> habilitarModoOffline(bool habilitado) async {
    if (habilitado) {
      await baixarESalvarPacote();
      return;
    }

    await _repository.habilitarModoOffline(false);
  }

  Future<bool> podeIniciarRotaOffline() {
    return _repository.pacoteValido();
  }

  Future<ResumoPacoteOffline> obterResumoPacote() {
    return _repository.obterResumoPacote();
  }

  Future<int> contarPendencias() {
    return _repository.contarPendencias();
  }

  Future<int> sincronizarPendencias() async {
    await ConnectivityService.ensureConnected();

    final lotes = await _repository.montarLotesSincronizacao();
    var sincronizados = 0;

    for (final lote in lotes) {
      final localExecucaoId = lote['localExecucaoId'] as String;
      try {
        final response = await _client.post(
          '/api/rastreio-app/offline/sincronizar',
          data: lote,
        );

        final data = Map<String, dynamic>.from(response.data);
        final sucesso = data['sucesso'] == true || data['Sucesso'] == true;
        final rotaExecucaoId = data['rotaExecucaoId'] ?? data['RotaExecucaoId'];

        if (!sucesso || rotaExecucaoId == null) {
          throw Exception(
            data['mensagem'] ??
                data['Mensagem'] ??
                'Sincronização offline recusada pelo servidor.',
          );
        }

        await _repository.marcarExecucaoSincronizada(
          localExecucaoId: localExecucaoId,
          rotaExecucaoId: rotaExecucaoId as int,
          sincronizadoEm: DateTime.now(),
          checklistLocalId: lote['checklistLocalId'] as int?,
        );
        sincronizados++;
      } catch (e) {
        await _repository.marcarErroSincronizacao(
          localExecucaoId: localExecucaoId,
          erro: e.toString(),
        );
        rethrow;
      }
    }

    return sincronizados;
  }

  Future<List<RotaChecklistDTO>> listarRotasCache() {
    return _repository.listarRotasCache();
  }

  Future<List<VeiculoChecklistDTO>> listarVeiculosCache(int rotaId) {
    return _repository.listarVeiculosCache(rotaId);
  }

  Future<int> salvarChecklistLocal({
    required int rotaId,
    required int veiculoId,
    required List<int> itensMarcados,
  }) {
    return _repository.salvarChecklistLocal(
      rotaId: rotaId,
      veiculoId: veiculoId,
      itensMarcados: itensMarcados,
    );
  }

  Future<RotaExecucaoDTO> iniciarExecucaoLocal({
    required int rotaId,
    required int veiculoId,
    int? checklistExecucaoId,
    double? latitudeInicio,
    double? longitudeInicio,
    bool gpsSimulado = false,
    List<PresencaPacienteRotaDTO> pacientesPresenca = const [],
    List<PresencaProfissionalRotaDTO> profissionaisPresenca = const [],
  }) {
    return _repository.iniciarExecucaoLocal(
      rotaId: rotaId,
      veiculoId: veiculoId,
      checklistExecucaoId: checklistExecucaoId,
      latitudeInicio: latitudeInicio,
      longitudeInicio: longitudeInicio,
      gpsSimulado: gpsSimulado,
      pacientesPresenca: pacientesPresenca,
      profissionaisPresenca: profissionaisPresenca,
    );
  }

  Future<void> registrarEventoParadaLocal({
    required String localExecucaoId,
    required int paradaId,
    required bool? entregue,
    required String? observacao,
    double? latitude,
    double? longitude,
  }) {
    return _repository.registrarEventoParadaLocal(
      localExecucaoId: localExecucaoId,
      paradaId: paradaId,
      entregue: entregue,
      observacao: observacao,
      latitude: latitude,
      longitude: longitude,
    );
  }

  Future<void> encerrarExecucaoLocal({
    required String localExecucaoId,
    required String? observacao,
  }) {
    return _repository.encerrarExecucaoLocal(
      localExecucaoId: localExecucaoId,
      observacao: observacao,
    );
  }

  Future<void> iniciarPausaLocal({
    required String localExecucaoId,
    required String motivo,
    double? latitude,
    double? longitude,
  }) {
    return _repository.iniciarPausaLocal(
      localExecucaoId: localExecucaoId,
      motivo: motivo,
      latitude: latitude,
      longitude: longitude,
    );
  }

  Future<void> finalizarPausaLocal({
    required String localExecucaoId,
    double? latitude,
    double? longitude,
  }) {
    return _repository.finalizarPausaLocal(
      localExecucaoId: localExecucaoId,
      latitude: latitude,
      longitude: longitude,
    );
  }

  Future<void> registrarLocalizacaoLocal({
    required String localExecucaoId,
    required double latitude,
    required double longitude,
    required DateTime dataHora,
    bool? gpsSimulado,
    double? precisaoEmMetros,
    double? velocidadeMetrosPorSegundo,
    double? direcaoGraus,
    double? altitudeMetros,
    int? fonteCaptura,
  }) {
    return _repository.registrarLocalizacaoLocal(
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
  }
}
