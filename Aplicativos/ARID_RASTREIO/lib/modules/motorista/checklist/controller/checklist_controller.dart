import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/store/checklist_item_store.dart';
import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/shared/functions/functions.dart';
import 'package:mobx/mobx.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import '../service/checklist_service.dart';

part 'checklist_controller.g.dart';

class ChecklistController = ChecklistControllerBase with _$ChecklistController;

abstract class ChecklistControllerBase with Store {
  @observable
  String? erroProcessamento;

  final _service = locator<ChecklistService>();

  @observable
  ObservableFuture<List<RotaChecklistDTO>>? rotasFuture;

  @observable
  ObservableFuture<List<VeiculoChecklistDTO>>? veiculosFuture;

  @observable
  RotaChecklistDTO? rotaSelecionada;

  @observable
  VeiculoChecklistDTO? veiculoSelecionado;

  @observable
  bool salvando = false;

  @observable
  bool checklistSalvo = false;

  @observable
  Map<String, List<int>> checklistSnapshots = {};

  @computed
  bool get temAlteracoesNaoSalvas {
    if (rotaSelecionada == null || veiculoSelecionado == null) {
      return false;
    }

    final chave = '${rotaSelecionada!.id}_${veiculoSelecionado!.id}';

    final snapshot = checklistSnapshots[chave];

    final atual = veiculoSelecionado!.checklist
        .where((i) => i.checked)
        .map((i) => i.id)
        .toList();

    if (snapshot == null) {
      return houveInteracao;
    }

    if (atual.length != snapshot.length) return true;

    for (final id in atual) {
      if (!snapshot.contains(id)) return true;
    }

    return false;
  }

  @computed
  int get totalItens => veiculoSelecionado?.checklist.length ?? 0;

  @computed
  int get itensMarcados =>
      veiculoSelecionado?.checklist.where((i) => i.checked).length ?? 0;

  @computed
  double get progressoChecklist {
    if (totalItens == 0) return 0;
    return itensMarcados / totalItens;
  }

  @computed
  bool get checklistAlteradoDepoisDeSalvar {
    if (rotaSelecionada == null || veiculoSelecionado == null) {
      return false;
    }

    final chave = '${rotaSelecionada!.id}_${veiculoSelecionado!.id}';

    final snapshot = checklistSnapshots[chave];

    if (snapshot == null) return false;

    final atual = veiculoSelecionado!.checklist
        .where((i) => i.checked)
        .map((i) => i.id)
        .toList();

    if (atual.length != snapshot.length) return true;

    for (final id in atual) {
      if (!snapshot.contains(id)) {
        return true;
      }
    }

    return false;
  }

  @computed
  bool get podeEditarChecklist {
    final rotasController = locator<MotoristaRotasController>();

    if (rotasController.rotaIniciada) return false;

    if (rotasController.rotaFinalizadaId == rotaSelecionada?.id) {
      return false;
    }

    return true;
  }

  @observable
  bool houveInteracao = false;

  @action
  void alternarCheck(ChecklistItemStore item) {
    if (!podeEditarChecklist) return;
    item.checked = !item.checked;
    houveInteracao = true;
  }

  @action
  Future<void> salvarChecklist() async {
    if (rotaSelecionada == null || veiculoSelecionado == null) {
      return;
    }

    erroProcessamento = null;

    try {
      salvando = true;

      final itensMarcados = veiculoSelecionado!.checklist
          .where((i) => i.checked)
          .map((i) => i.id)
          .toList();

      await _service.salvarChecklist(
        rotaId: rotaSelecionada!.id,
        veiculoId: veiculoSelecionado!.id,
        itensMarcados: itensMarcados,
      );

      final chave = '${rotaSelecionada!.id}_${veiculoSelecionado!.id}';
      checklistSnapshots[chave] = List.from(itensMarcados);
      checklistSalvo = true;
    } catch (e) {
      erroProcessamento = extrairMensagemErro(e);
    } finally {
      salvando = false;
    }
  }

  @action
  Future<void> carregarRotas() async {
    erroProcessamento = null;

    try {
      rotasFuture = ObservableFuture(_service.obterRotas());
      await rotasFuture;
    } catch (e) {
      erroProcessamento = extrairMensagemErro(e);
    }
  }

  @action
  Future<void> selecionarRota(RotaChecklistDTO rota) async {
    erroProcessamento = null;

    rotaSelecionada = rota;
    veiculoSelecionado = null;
    checklistSalvo = false;
    houveInteracao = false;

    try {
      veiculosFuture = ObservableFuture(_service.obterVeiculosPorRota(rota.id));
      await veiculosFuture;
    } catch (e) {
      erroProcessamento = extrairMensagemErro(e);
    }
  }

  @action
  void selecionarVeiculo(VeiculoChecklistDTO veiculo) {
    veiculoSelecionado = veiculo;

    final chave = '${rotaSelecionada?.id}_${veiculo.id}';

    if (checklistSnapshots.containsKey(chave)) {
      final snapshot = checklistSnapshots[chave]!;

      for (final item in veiculo.checklist) {
        item.checked = snapshot.contains(item.id);
      }

      checklistSalvo = true;
    } else {
      for (final item in veiculo.checklist) {
        item.checked = false;
      }

      checklistSalvo = false;
    }

    houveInteracao = false;
  }

  @action
  void limpar() {
    rotasFuture = null;
    veiculosFuture = null;
    rotaSelecionada = null;
    veiculoSelecionado = null;
    checklistSalvo = false;
    salvando = false;
    erroProcessamento = null;
  }
}

