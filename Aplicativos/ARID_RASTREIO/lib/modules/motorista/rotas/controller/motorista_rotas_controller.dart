import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/encerrar_parada_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/store/parada_store.dart';
import 'package:arid_rastreio/core/service/rota_tracking_service.dart';
import 'package:geolocator/geolocator.dart';
import 'package:mobx/mobx.dart';
import '../service/motorista_rotas_service.dart';

part 'motorista_rotas_controller.g.dart';

class MotoristaRotasController = MotoristaRotasControllerBase
    with _$MotoristaRotasController;

abstract class MotoristaRotasControllerBase with Store {
  final MotoristaRotasService _service = MotoristaRotasService();

  @observable
  RotaExecucaoDTO? rotaAtual;

  @observable
  bool carregando = false;

  @observable
  bool rotaIniciada = false;

  @observable
  bool estaPausada = false;

  @observable
  int? rotaFinalizadaId;

  @observable
  ObservableList<ParadaStore> paradas = ObservableList<ParadaStore>();

  @action
  Future<RotaExecucaoDTO?> obterRotaEmAndamento() async {
    carregando = true;
    try {
      final execucao = await _service.obterRotaEmAndamento();
      if (execucao != null) {
        rotaAtual = execucao;
        _carregarParadasEOrigens(execucao);
        rotaIniciada = true;
        estaPausada = execucao.estaPausada;
        
        _enviarLocalizacaoAtual();
      }
      return execucao;
    } finally {
      carregando = false;
    }
  }

  Future<void> _enviarLocalizacaoAtual() async {
    if (rotaAtual == null) return;
    try {
      final pos = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(
          accuracy: LocationAccuracy.bestForNavigation,
        ),
      );

      await _service.salvarPontoDaRotaBackground(
        rotaExecucaoId: rotaAtual!.id,
        latitude: pos.latitude,
        longitude: pos.longitude,
        dataHora: DateTime.now(),
      );
    } catch (_) {
      // Falha silenciosa para não travar o fluxo principal
    }
  }

  @action
  Future<RotaExecucaoDTO> iniciarRota({
    required int rotaId,
    required int veiculoId,
  }) async {
    carregando = true;

    try {
      final execucao = await _service.iniciarRota(
        rotaId: rotaId,
        veiculoId: veiculoId,
      );

      rotaAtual = execucao;
      _carregarParadasEOrigens(execucao);
      rotaIniciada = true;
      estaPausada = execucao.estaPausada;

      _enviarLocalizacaoAtual();

      return execucao;
    } finally {
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
        )
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
        )
      );
    }
  }

  @action
  Future<void> confirmarParada(ParadaStore parada) async {
    if (rotaAtual == null) return;

    parada.salvando = true;

    try {
      await _service.confirmarParada(
        rotaExecucaoId: rotaAtual!.id,
        paradaId: parada.id,
        entregue: parada.entregue,
        observacao: parada.observacao,
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
      final loc = await Geolocator.getCurrentPosition();
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
      final loc = await Geolocator.getCurrentPosition();
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

