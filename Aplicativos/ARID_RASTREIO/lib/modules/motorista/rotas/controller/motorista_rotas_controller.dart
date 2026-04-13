import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/encerrar_parada_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/store/parada_store.dart';
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
  int? rotaFinalizadaId;

  @observable
  ObservableList<ParadaStore> paradas = ObservableList<ParadaStore>();

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

      paradas.clear();

      for (final p in execucao.paradas) {
        paradas.add(
          ParadaStore(
            id: p.id,
            endereco: p.endereco,
            latitude: p.latitude,
            longitude: p.longitude,
            link: p.link,
            entregue: p.entregue,
          ),
        );
      }

      rotaIniciada = true;

      return execucao;
    } finally {
      carregando = false;
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
  void limpar() {
    rotaAtual = null;
    rotaIniciada = false;
    rotaFinalizadaId = null;
    paradas.clear();
  }
}

