// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'checklist_controller.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic, no_leading_underscores_for_local_identifiers

mixin _$ChecklistController on ChecklistControllerBase, Store {
  Computed<bool>? _$temAlteracoesNaoSalvasComputed;

  @override
  bool get temAlteracoesNaoSalvas =>
      (_$temAlteracoesNaoSalvasComputed ??= Computed<bool>(
        () => super.temAlteracoesNaoSalvas,
        name: 'ChecklistControllerBase.temAlteracoesNaoSalvas',
      )).value;
  Computed<int>? _$totalItensComputed;

  @override
  int get totalItens => (_$totalItensComputed ??= Computed<int>(
    () => super.totalItens,
    name: 'ChecklistControllerBase.totalItens',
  )).value;
  Computed<int>? _$itensMarcadosComputed;

  @override
  int get itensMarcados => (_$itensMarcadosComputed ??= Computed<int>(
    () => super.itensMarcados,
    name: 'ChecklistControllerBase.itensMarcados',
  )).value;
  Computed<double>? _$progressoChecklistComputed;

  @override
  double get progressoChecklist =>
      (_$progressoChecklistComputed ??= Computed<double>(
        () => super.progressoChecklist,
        name: 'ChecklistControllerBase.progressoChecklist',
      )).value;
  Computed<bool>? _$checklistAlteradoDepoisDeSalvarComputed;

  @override
  bool get checklistAlteradoDepoisDeSalvar =>
      (_$checklistAlteradoDepoisDeSalvarComputed ??= Computed<bool>(
        () => super.checklistAlteradoDepoisDeSalvar,
        name: 'ChecklistControllerBase.checklistAlteradoDepoisDeSalvar',
      )).value;
  Computed<bool>? _$podeEditarChecklistComputed;

  @override
  bool get podeEditarChecklist =>
      (_$podeEditarChecklistComputed ??= Computed<bool>(
        () => super.podeEditarChecklist,
        name: 'ChecklistControllerBase.podeEditarChecklist',
      )).value;

  late final _$erroProcessamentoAtom = Atom(
    name: 'ChecklistControllerBase.erroProcessamento',
    context: context,
  );

  @override
  String? get erroProcessamento {
    _$erroProcessamentoAtom.reportRead();
    return super.erroProcessamento;
  }

  @override
  set erroProcessamento(String? value) {
    _$erroProcessamentoAtom.reportWrite(value, super.erroProcessamento, () {
      super.erroProcessamento = value;
    });
  }

  late final _$rotasFutureAtom = Atom(
    name: 'ChecklistControllerBase.rotasFuture',
    context: context,
  );

  @override
  ObservableFuture<List<RotaChecklistDTO>>? get rotasFuture {
    _$rotasFutureAtom.reportRead();
    return super.rotasFuture;
  }

  @override
  set rotasFuture(ObservableFuture<List<RotaChecklistDTO>>? value) {
    _$rotasFutureAtom.reportWrite(value, super.rotasFuture, () {
      super.rotasFuture = value;
    });
  }

  late final _$veiculosFutureAtom = Atom(
    name: 'ChecklistControllerBase.veiculosFuture',
    context: context,
  );

  @override
  ObservableFuture<List<VeiculoChecklistDTO>>? get veiculosFuture {
    _$veiculosFutureAtom.reportRead();
    return super.veiculosFuture;
  }

  @override
  set veiculosFuture(ObservableFuture<List<VeiculoChecklistDTO>>? value) {
    _$veiculosFutureAtom.reportWrite(value, super.veiculosFuture, () {
      super.veiculosFuture = value;
    });
  }

  late final _$rotaSelecionadaAtom = Atom(
    name: 'ChecklistControllerBase.rotaSelecionada',
    context: context,
  );

  @override
  RotaChecklistDTO? get rotaSelecionada {
    _$rotaSelecionadaAtom.reportRead();
    return super.rotaSelecionada;
  }

  @override
  set rotaSelecionada(RotaChecklistDTO? value) {
    _$rotaSelecionadaAtom.reportWrite(value, super.rotaSelecionada, () {
      super.rotaSelecionada = value;
    });
  }

  late final _$veiculoSelecionadoAtom = Atom(
    name: 'ChecklistControllerBase.veiculoSelecionado',
    context: context,
  );

  @override
  VeiculoChecklistDTO? get veiculoSelecionado {
    _$veiculoSelecionadoAtom.reportRead();
    return super.veiculoSelecionado;
  }

  @override
  set veiculoSelecionado(VeiculoChecklistDTO? value) {
    _$veiculoSelecionadoAtom.reportWrite(value, super.veiculoSelecionado, () {
      super.veiculoSelecionado = value;
    });
  }

  late final _$salvandoAtom = Atom(
    name: 'ChecklistControllerBase.salvando',
    context: context,
  );

  @override
  bool get salvando {
    _$salvandoAtom.reportRead();
    return super.salvando;
  }

  @override
  set salvando(bool value) {
    _$salvandoAtom.reportWrite(value, super.salvando, () {
      super.salvando = value;
    });
  }

  late final _$checklistSalvoAtom = Atom(
    name: 'ChecklistControllerBase.checklistSalvo',
    context: context,
  );

  @override
  bool get checklistSalvo {
    _$checklistSalvoAtom.reportRead();
    return super.checklistSalvo;
  }

  @override
  set checklistSalvo(bool value) {
    _$checklistSalvoAtom.reportWrite(value, super.checklistSalvo, () {
      super.checklistSalvo = value;
    });
  }

  late final _$ultimaExecucaoIdAtom = Atom(
    name: 'ChecklistControllerBase.ultimaExecucaoId',
    context: context,
  );

  @override
  int? get ultimaExecucaoId {
    _$ultimaExecucaoIdAtom.reportRead();
    return super.ultimaExecucaoId;
  }

  @override
  set ultimaExecucaoId(int? value) {
    _$ultimaExecucaoIdAtom.reportWrite(value, super.ultimaExecucaoId, () {
      super.ultimaExecucaoId = value;
    });
  }

  late final _$checklistSnapshotsAtom = Atom(
    name: 'ChecklistControllerBase.checklistSnapshots',
    context: context,
  );

  @override
  Map<String, List<int>> get checklistSnapshots {
    _$checklistSnapshotsAtom.reportRead();
    return super.checklistSnapshots;
  }

  @override
  set checklistSnapshots(Map<String, List<int>> value) {
    _$checklistSnapshotsAtom.reportWrite(value, super.checklistSnapshots, () {
      super.checklistSnapshots = value;
    });
  }

  late final _$houveInteracaoAtom = Atom(
    name: 'ChecklistControllerBase.houveInteracao',
    context: context,
  );

  @override
  bool get houveInteracao {
    _$houveInteracaoAtom.reportRead();
    return super.houveInteracao;
  }

  @override
  set houveInteracao(bool value) {
    _$houveInteracaoAtom.reportWrite(value, super.houveInteracao, () {
      super.houveInteracao = value;
    });
  }

  late final _$salvarChecklistAsyncAction = AsyncAction(
    'ChecklistControllerBase.salvarChecklist',
    context: context,
  );

  @override
  Future<void> salvarChecklist() {
    return _$salvarChecklistAsyncAction.run(() => super.salvarChecklist());
  }

  late final _$carregarRotasAsyncAction = AsyncAction(
    'ChecklistControllerBase.carregarRotas',
    context: context,
  );

  @override
  Future<void> carregarRotas() {
    return _$carregarRotasAsyncAction.run(() => super.carregarRotas());
  }

  late final _$selecionarRotaAsyncAction = AsyncAction(
    'ChecklistControllerBase.selecionarRota',
    context: context,
  );

  @override
  Future<void> selecionarRota(RotaChecklistDTO rota) {
    return _$selecionarRotaAsyncAction.run(() => super.selecionarRota(rota));
  }

  late final _$restaurarSelecaoSessaoAsyncAction = AsyncAction(
    'ChecklistControllerBase.restaurarSelecaoSessao',
    context: context,
  );

  @override
  Future<void> restaurarSelecaoSessao({
    required int rotaId,
    required int veiculoId,
    int? checklistExecucaoId,
  }) {
    return _$restaurarSelecaoSessaoAsyncAction.run(
      () => super.restaurarSelecaoSessao(
        rotaId: rotaId,
        veiculoId: veiculoId,
        checklistExecucaoId: checklistExecucaoId,
      ),
    );
  }

  late final _$ChecklistControllerBaseActionController = ActionController(
    name: 'ChecklistControllerBase',
    context: context,
  );

  @override
  void alternarCheck(ChecklistItemStore item) {
    final _$actionInfo = _$ChecklistControllerBaseActionController.startAction(
      name: 'ChecklistControllerBase.alternarCheck',
    );
    try {
      return super.alternarCheck(item);
    } finally {
      _$ChecklistControllerBaseActionController.endAction(_$actionInfo);
    }
  }

  @override
  void selecionarVeiculo(VeiculoChecklistDTO veiculo) {
    final _$actionInfo = _$ChecklistControllerBaseActionController.startAction(
      name: 'ChecklistControllerBase.selecionarVeiculo',
    );
    try {
      return super.selecionarVeiculo(veiculo);
    } finally {
      _$ChecklistControllerBaseActionController.endAction(_$actionInfo);
    }
  }

  @override
  void limpar() {
    final _$actionInfo = _$ChecklistControllerBaseActionController.startAction(
      name: 'ChecklistControllerBase.limpar',
    );
    try {
      return super.limpar();
    } finally {
      _$ChecklistControllerBaseActionController.endAction(_$actionInfo);
    }
  }

  @override
  String toString() {
    return '''
erroProcessamento: ${erroProcessamento},
rotasFuture: ${rotasFuture},
veiculosFuture: ${veiculosFuture},
rotaSelecionada: ${rotaSelecionada},
veiculoSelecionado: ${veiculoSelecionado},
salvando: ${salvando},
checklistSalvo: ${checklistSalvo},
ultimaExecucaoId: ${ultimaExecucaoId},
checklistSnapshots: ${checklistSnapshots},
houveInteracao: ${houveInteracao},
temAlteracoesNaoSalvas: ${temAlteracoesNaoSalvas},
totalItens: ${totalItens},
itensMarcados: ${itensMarcados},
progressoChecklist: ${progressoChecklist},
checklistAlteradoDepoisDeSalvar: ${checklistAlteradoDepoisDeSalvar},
podeEditarChecklist: ${podeEditarChecklist}
    ''';
  }
}
