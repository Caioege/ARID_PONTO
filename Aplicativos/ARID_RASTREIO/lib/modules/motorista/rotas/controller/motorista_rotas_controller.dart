import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/encerrar_parada_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/store/parada_store.dart';
import 'package:arid_rastreio/core/service/rota_tracking_service.dart';
import 'package:geolocator/geolocator.dart';
import 'package:mobx/mobx.dart';
import 'package:flutter/foundation.dart';
import '../service/motorista_rotas_service.dart';

part 'motorista_rotas_controller.g.dart';

class MotoristaRotasController = MotoristaRotasControllerBase
    with _$MotoristaRotasController;

abstract class MotoristaRotasControllerBase with Store {
  static const Duration _idadeMaximaPosicaoConhecida = Duration(seconds: 60);

  final MotoristaRotasService _service = MotoristaRotasService();

  LocationSettings _getLocationSettings(
    LocationAccuracy accuracy, {
    int seconds = 25,
  }) {
    if (kIsWeb || defaultTargetPlatform != TargetPlatform.android) {
      return LocationSettings(
        accuracy: accuracy,
        timeLimit: Duration(seconds: seconds),
      );
    }
    return AndroidSettings(
      accuracy: accuracy,
      timeLimit: Duration(seconds: seconds),
      forceLocationManager: true,
    );
  }

  // ignore: unused_element
  Future<Position?> _obterPosicaoRobusta({bool lancaErro = true}) async {
    print('[DEBUG] Controller: Tentando getLastKnownPosition...');
    Position? pos;
    try {
      pos = await Geolocator.getLastKnownPosition();
    } catch (_) {}

    if (pos != null) {
      final age = DateTime.now().difference(pos.timestamp);
      print(
        '[DEBUG] Controller: Última posição tem ${age.inSeconds} segundos de idade.',
      );

      // No emulador (debug), pontos estáticos ficam "velhos" rápido e o getCurrentPosition
      // trava esperando sinal real. Por isso, ignoramos a idade se for debug.
      final bool aceitarPosicaoAntiga = kDebugMode;

      // Se a posição for antiga, tentamos uma nova, guardando a antiga como fallback
      if (age.inSeconds > 120 && !aceitarPosicaoAntiga) {
        print('[DEBUG] Controller: Posição antiga. Tentando atualizar...');
        try {
          pos = await Geolocator.getCurrentPosition(
            locationSettings: _getLocationSettings(
              LocationAccuracy.high,
              seconds: 15,
            ),
          );
        } catch (e) {
          print(
            '[DEBUG] Controller: Falha ao obter nova posição ($e). Usando a antiga como fallback.',
          );
          pos = await Geolocator.getLastKnownPosition();
        }
      } else if (age.inSeconds > 120 && aceitarPosicaoAntiga) {
        print(
          '[DEBUG] Controller: Posição antiga aceita por estar em modo Debug (Emulador).',
        );
      }
    } else {
      print(
        '[DEBUG] Controller: Nenhuma posição anterior. Capturando posição atual...',
      );
      try {
        pos = await Geolocator.getCurrentPosition(
          locationSettings: _getLocationSettings(LocationAccuracy.high),
        );
      } catch (e) {
        print('[DEBUG] Controller: Falha total ao capturar posição ($e).');
      }
    }

    if (pos == null && lancaErro) {
      throw Exception('Não foi possível obter a localização. Verifique o GPS.');
    }

    if (pos != null) {
      print(
        '[DEBUG] Controller: Posição obtida: ${pos.latitude}, ${pos.longitude}. isMocked: ${pos.isMocked}',
      );
    }
    return pos;
  }

  Future<Position?> _obterPosicaoAtualizada({bool lancaErro = true}) async {
    debugPrint('[DEBUG] Controller: Tentando getLastKnownPosition...');
    Position? ultimaPosicao;
    try {
      ultimaPosicao = await Geolocator.getLastKnownPosition();
    } catch (_) {}

    if (ultimaPosicao != null) {
      final age = DateTime.now().difference(ultimaPosicao.timestamp);
      debugPrint(
        '[DEBUG] Controller: Ultima posicao tem ${age.inSeconds} segundos de idade.',
      );

      if (age <= _idadeMaximaPosicaoConhecida) {
        debugPrint(
          '[DEBUG] Controller: Usando ultima posicao conhecida recente.',
        );
        return ultimaPosicao;
      }

      debugPrint(
        '[DEBUG] Controller: Posicao conhecida antiga. Tentando atualizar...',
      );
    } else {
      debugPrint(
        '[DEBUG] Controller: Nenhuma posicao anterior. Capturando posicao atual...',
      );
    }

    Position? pos;
    try {
      pos = await Geolocator.getCurrentPosition(
        locationSettings: _getLocationSettings(LocationAccuracy.high),
      );
    } catch (e) {
      debugPrint('[DEBUG] Controller: Falha ao capturar posicao atual ($e).');

      if (!kDebugMode && ultimaPosicao != null) {
        debugPrint(
          '[DEBUG] Controller: Usando ultima posicao como fallback em producao.',
        );
        pos = ultimaPosicao;
      }
    }

    if (pos == null && lancaErro) {
      throw Exception('Nao foi possivel obter a localizacao. Verifique o GPS.');
    }

    if (pos != null) {
      debugPrint(
        '[DEBUG] Controller: Posicao obtida: ${pos.latitude}, ${pos.longitude}. isMocked: ${pos.isMocked}',
      );
    }
    return pos;
  }

  @observable
  RotaExecucaoDTO? rotaAtual;

  @observable
  bool carregando = false;

  @observable
  bool rotaIniciada = false;

  @observable
  bool estaPausada = false;

  @observable
  bool recuperandoSessao = false;

  @observable
  int? checklistExecucaoId;

  @observable
  int? rotaFinalizadaId;

  @observable
  ObservableList<ParadaStore> paradas = ObservableList<ParadaStore>();

  @action
  Future<RotaExecucaoDTO?> obterRotaEmAndamento() async {
    recuperandoSessao = true;
    try {
      final execucao = await _service.obterRotaEmAndamento();
      if (execucao != null) {
        rotaAtual = execucao;
        checklistExecucaoId = execucao.checklistExecucaoId;

        _carregarParadasEOrigens(execucao);
        rotaIniciada = true;
        estaPausada = execucao.estaPausada;

        // Recuperar estado no ChecklistController
        if (execucao.veiculoId != null) {
          await locator<ChecklistController>().restaurarSelecaoSessao(
            rotaId: execucao.rotaId,
            veiculoId: execucao.veiculoId!,
            checklistExecucaoId: execucao.checklistExecucaoId,
          );
        }

        _enviarLocalizacaoAtual();
      }
      return execucao;
    } finally {
      recuperandoSessao = false;
    }
  }

  Future<void> _enviarLocalizacaoAtual() async {
    if (rotaAtual == null) return;
    try {
      final pos = await _obterPosicaoAtualizada(lancaErro: false);
      if (pos == null) return;

      await _service.salvarPontoDaRotaBackground(
        rotaExecucaoId: rotaAtual!.id,
        latitude: pos.latitude,
        longitude: pos.longitude,
        dataHora: DateTime.now(),
        gpsSimulado: pos.isMocked,
        precisaoEmMetros: pos.accuracy,
        velocidadeMetrosPorSegundo: pos.speed,
        direcaoGraus: pos.heading,
        altitudeMetros: pos.altitude,
        fonteCaptura: 0,
      );
    } catch (e, stack) {
      print('[DEBUG] Falha ao salvar ponto background: $e');
      print(stack);
    }
  }

  @action
  Future<RotaExecucaoDTO> iniciarRota({
    required int rotaId,
    required int veiculoId,
    int? checklistId,
  }) async {
    print('[DEBUG] Controller: iniciarRota iniciado');
    carregando = true;

    try {
      print(
        '[DEBUG] Controller: Verificando se serviço de localização está ativo...',
      );
      bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
      print('[DEBUG] Controller: Serviço ativo? $serviceEnabled');
      if (!serviceEnabled) {
        throw Exception(
          'O serviço de localização (GPS) está desativado. Por favor, ative-o nas configurações do dispositivo.',
        );
      }

      print('[DEBUG] Controller: Capturando posição para iniciar rota...');
      final pos = await _obterPosicaoAtualizada(lancaErro: true);

      if (pos!.isMocked && !kDebugMode) {
        throw Exception(
          'Simulador de localização detectado. Por favor, desabilite-o para iniciar a rota.',
        );
      }

      print(
        '[DEBUG] Controller: Chamando serviço para iniciar rota no servidor...',
      );
      final execucao = await _service.iniciarRota(
        rotaId: rotaId,
        veiculoId: veiculoId,
        checklistExecucaoId: checklistId,
        latitudeInicio: pos.latitude,
        longitudeInicio: pos.longitude,
        gpsSimulado: pos.isMocked,
      );
      print('[DEBUG] Controller: Rota iniciada no servidor com sucesso.');

      rotaAtual = execucao;
      checklistExecucaoId = checklistId;

      _carregarParadasEOrigens(execucao);
      rotaIniciada = true;
      estaPausada = execucao.estaPausada;

      print(
        '[DEBUG] Controller: Enviando localização inicial em background...',
      );
      _enviarLocalizacaoAtual();

      return execucao;
    } finally {
      print('[DEBUG] Controller: Finalizando estado de carregamento.');
      carregando = false;
    }
  }

  void _carregarParadasEOrigens(RotaExecucaoDTO execucao) {
    paradas.clear();

    if (execucao.nomeUnidadeOrigem != null) {
      paradas.add(
        ParadaStore(
          id: -1,
          endereco: execucao.nomeUnidadeOrigem!,
          entregue: execucao.origemEntregue,
          observacao: execucao.origemObservacao ?? '',
        ),
      );
    }

    for (final p in execucao.paradas) {
      paradas.add(
        ParadaStore(
          id: p.id,
          endereco: p.endereco,
          latitude: p.latitude,
          longitude: p.longitude,
          link: p.link,
          entregue: p.entregue,
          observacao: p.observacao ?? '',
        ),
      );
    }

    if (execucao.nomeUnidadeDestino != null) {
      paradas.add(
        ParadaStore(
          id: -2,
          endereco: execucao.nomeUnidadeDestino!,
          entregue: execucao.destinoEntregue,
          observacao: execucao.destinoObservacao ?? '',
        ),
      );
    }
  }

  @action
  Future<void> confirmarParada(ParadaStore parada) async {
    if (rotaAtual == null) return;

    parada.salvando = true;

    try {
      // Captura a posição exata no momento da confirmação para salvar junto com a parada
      final pos = await _obterPosicaoAtualizada(lancaErro: false);

      await _service.confirmarParada(
        rotaExecucaoId: rotaAtual!.id,
        paradaId: parada.id,
        entregue: parada.entregue,
        observacao: parada.observacao,
        latitude: pos?.latitude,
        longitude: pos?.longitude,
      );

      parada.confirmada = true;
      _enviarLocalizacaoAtual();
    } finally {
      parada.salvando = false;
    }
  }

  @action
  void atualizarEntrega(ParadaStore parada, bool? valor) {
    parada.entregue = valor;
  }

  @action
  void atualizarObservacao(ParadaStore parada, String valor) {
    parada.observacao = valor;
  }

  @action
  Future<void> encerrarRota() async {
    if (rotaAtual == null) return;

    carregando = true;

    try {
      await _service.encerrarRota(
        rotaExecucaoId: rotaAtual!.id,
        paradas: paradas
            .map(
              (p) => EncerrarParadaDTO(
                id: p.id,
                entregue: p.entregue,
                observacao: p.observacao,
              ),
            )
            .toList(),
      );

      final checklistController = locator<ChecklistController>();

      if (checklistController.rotaSelecionada != null) {
        checklistController.rotaSelecionada = RotaChecklistDTO(
          id: checklistController.rotaSelecionada!.id,
          codigo: checklistController.rotaSelecionada!.codigo,
          nome: checklistController.rotaSelecionada!.nome,
          descricao: checklistController.rotaSelecionada!.descricao,
          rotaFinalizada: true,
        );
      }

      rotaIniciada = false;
      rotaFinalizadaId = rotaAtual?.rotaId;
      rotaAtual = null;
      paradas.clear();
    } finally {
      carregando = false;
    }
  }

  @action
  Future<void> iniciarPausa(String motivo) async {
    if (rotaAtual == null) return;
    carregando = true;

    try {
      final loc = await _obterPosicaoAtualizada(lancaErro: false);
      await _service.iniciarPausa(
        rotaExecucaoId: rotaAtual!.id,
        motivo: motivo,
        latitude: loc?.latitude,
        longitude: loc?.longitude,
      );

      await RotaTrackingService.stop();
      estaPausada = true;
    } finally {
      carregando = false;
    }
  }

  @action
  Future<void> finalizarPausa() async {
    if (rotaAtual == null) return;
    carregando = true;

    try {
      final loc = await _obterPosicaoAtualizada(lancaErro: false);
      await _service.finalizarPausa(
        rotaExecucaoId: rotaAtual!.id,
        latitude: loc?.latitude,
        longitude: loc?.longitude,
      );

      await RotaTrackingService.start();
      estaPausada = false;
    } finally {
      carregando = false;
    }
  }

  @action
  void limpar() {
    rotaAtual = null;
    rotaIniciada = false;
    rotaFinalizadaId = null;
    paradas.clear();
  }
}
