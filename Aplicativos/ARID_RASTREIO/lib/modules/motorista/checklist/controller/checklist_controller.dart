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
  int? ultimaExecucaoId;

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

      final idExec = await _service.salvarChecklist(
        rotaId: rotaSelecionada!.id,
        veiculoId: veiculoSelecionado!.id,
        itensMarcados: itensMarcados,
      );

      ultimaExecucaoId = idExec;

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

  void restaurarSelecaoLocal({
    required RotaChecklistDTO rota,
    required VeiculoChecklistDTO veiculo,
    int? checklistExecucaoId,
    List<int> itensMarcados = const [],
  }) {
    rotaSelecionada = rota;
    veiculoSelecionado = veiculo;
    ultimaExecucaoId = checklistExecucaoId;
    checklistSalvo = checklistExecucaoId != null;

    final chave = '${rota.id}_${veiculo.id}';
    checklistSnapshots[chave] = List.from(itensMarcados);

    for (final item in veiculo.checklist) {
      item.checked = itensMarcados.isNotEmpty
          ? itensMarcados.contains(item.id)
          : checklistSalvo;
    }

    veiculosFuture = ObservableFuture.value([veiculo]);
    houveInteracao = false;
    erroProcessamento = null;
  }

  @action
  Future<void> restaurarSelecaoSessao({
    required int rotaId,
    required int veiculoId,
    int? checklistExecucaoId,
  }) async {
    try {
      // 1. Carrega as rotas se não estiverem carregadas
      if (rotasFuture == null) {
        await carregarRotas();
      }

      // 2. Localiza a rota na lista
      final rota = rotasFuture?.value?.where((r) => r.id == rotaId).firstOrNull;
      if (rota != null) {
        rotaSelecionada = rota;

        // 3. Carrega os veículos daquela rota
        veiculosFuture = ObservableFuture(
          _service.obterVeiculosPorRota(rotaId),
        );
        final veiculos = await veiculosFuture;

        // 4. Localiza o veículo
        final veiculo = veiculos?.where((v) => v.id == veiculoId).firstOrNull;
        if (veiculo != null) {
          veiculoSelecionado = veiculo;

          // Se já temos um checklist salvo nessa sessão
          checklistSalvo = checklistExecucaoId != null;

          // Marcar todos como checked para efeito visual de "concluído"
          // (Poderíamos buscar os itens específicos se necessário, mas por agora
          //  se checklistExecucaoId existe, consideramos pronto)
          if (checklistSalvo) {
            for (final item in veiculo.checklist) {
              item.checked = true;
            }
          }
        }
      }
    } catch (_) {}
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
