// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'motorista_rotas_controller.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic, no_leading_underscores_for_local_identifiers

mixin _$MotoristaRotasController on MotoristaRotasControllerBase, Store {
  late final _$rotaAtualAtom = Atom(
    name: 'MotoristaRotasControllerBase.rotaAtual',
    context: context,
  );

  @override
  RotaExecucaoDTO? get rotaAtual {
    _$rotaAtualAtom.reportRead();
    return super.rotaAtual;
  }

  @override
  set rotaAtual(RotaExecucaoDTO? value) {
    _$rotaAtualAtom.reportWrite(value, super.rotaAtual, () {
      super.rotaAtual = value;
    });
  }

  late final _$carregandoAtom = Atom(
    name: 'MotoristaRotasControllerBase.carregando',
    context: context,
  );

  @override
  bool get carregando {
    _$carregandoAtom.reportRead();
    return super.carregando;
  }

  @override
  set carregando(bool value) {
    _$carregandoAtom.reportWrite(value, super.carregando, () {
      super.carregando = value;
    });
  }

  late final _$rotaIniciadaAtom = Atom(
    name: 'MotoristaRotasControllerBase.rotaIniciada',
    context: context,
  );

  @override
  bool get rotaIniciada {
    _$rotaIniciadaAtom.reportRead();
    return super.rotaIniciada;
  }

  @override
  set rotaIniciada(bool value) {
    _$rotaIniciadaAtom.reportWrite(value, super.rotaIniciada, () {
      super.rotaIniciada = value;
    });
  }

  late final _$rotaFinalizadaIdAtom = Atom(
    name: 'MotoristaRotasControllerBase.rotaFinalizadaId',
    context: context,
  );

  @override
  int? get rotaFinalizadaId {
    _$rotaFinalizadaIdAtom.reportRead();
    return super.rotaFinalizadaId;
  }

  @override
  set rotaFinalizadaId(int? value) {
    _$rotaFinalizadaIdAtom.reportWrite(value, super.rotaFinalizadaId, () {
      super.rotaFinalizadaId = value;
    });
  }

  late final _$paradasAtom = Atom(
    name: 'MotoristaRotasControllerBase.paradas',
    context: context,
  );

  @override
  ObservableList<ParadaStore> get paradas {
    _$paradasAtom.reportRead();
    return super.paradas;
  }

  @override
  set paradas(ObservableList<ParadaStore> value) {
    _$paradasAtom.reportWrite(value, super.paradas, () {
      super.paradas = value;
    });
  }

  late final _$iniciarRotaAsyncAction = AsyncAction(
    'MotoristaRotasControllerBase.iniciarRota',
    context: context,
  );

  @override
  Future<RotaExecucaoDTO> iniciarRota({
    required int rotaId,
    required int veiculoId,
  }) {
    return _$iniciarRotaAsyncAction.run(
      () => super.iniciarRota(rotaId: rotaId, veiculoId: veiculoId),
    );
  }

  late final _$confirmarParadaAsyncAction = AsyncAction(
    'MotoristaRotasControllerBase.confirmarParada',
    context: context,
  );

  @override
  Future<void> confirmarParada(ParadaStore parada) {
    return _$confirmarParadaAsyncAction.run(
      () => super.confirmarParada(parada),
    );
  }

  late final _$encerrarRotaAsyncAction = AsyncAction(
    'MotoristaRotasControllerBase.encerrarRota',
    context: context,
  );

  @override
  Future<void> encerrarRota() {
    return _$encerrarRotaAsyncAction.run(() => super.encerrarRota());
  }

  late final _$MotoristaRotasControllerBaseActionController = ActionController(
    name: 'MotoristaRotasControllerBase',
    context: context,
  );

  @override
  void atualizarEntrega(ParadaStore parada, bool? valor) {
    final _$actionInfo = _$MotoristaRotasControllerBaseActionController
        .startAction(name: 'MotoristaRotasControllerBase.atualizarEntrega');
    try {
      return super.atualizarEntrega(parada, valor);
    } finally {
      _$MotoristaRotasControllerBaseActionController.endAction(_$actionInfo);
    }
  }

  @override
  void atualizarObservacao(ParadaStore parada, String valor) {
    final _$actionInfo = _$MotoristaRotasControllerBaseActionController
        .startAction(name: 'MotoristaRotasControllerBase.atualizarObservacao');
    try {
      return super.atualizarObservacao(parada, valor);
    } finally {
      _$MotoristaRotasControllerBaseActionController.endAction(_$actionInfo);
    }
  }

  @override
  void limpar() {
    final _$actionInfo = _$MotoristaRotasControllerBaseActionController
        .startAction(name: 'MotoristaRotasControllerBase.limpar');
    try {
      return super.limpar();
    } finally {
      _$MotoristaRotasControllerBaseActionController.endAction(_$actionInfo);
    }
  }

  @override
  String toString() {
    return '''
rotaAtual: ${rotaAtual},
carregando: ${carregando},
rotaIniciada: ${rotaIniciada},
rotaFinalizadaId: ${rotaFinalizadaId},
paradas: ${paradas}
    ''';
  }
}
