// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'rota_monitoramento_controller.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic, no_leading_underscores_for_local_identifiers

mixin _$RotaMonitoramentoController on _RotaMonitoramentoControllerBase, Store {
  late final _$latitudeAtom = Atom(
    name: '_RotaMonitoramentoControllerBase.latitude',
    context: context,
  );

  @override
  double? get latitude {
    _$latitudeAtom.reportRead();
    return super.latitude;
  }

  @override
  set latitude(double? value) {
    _$latitudeAtom.reportWrite(value, super.latitude, () {
      super.latitude = value;
    });
  }

  late final _$longitudeAtom = Atom(
    name: '_RotaMonitoramentoControllerBase.longitude',
    context: context,
  );

  @override
  double? get longitude {
    _$longitudeAtom.reportRead();
    return super.longitude;
  }

  @override
  set longitude(double? value) {
    _$longitudeAtom.reportWrite(value, super.longitude, () {
      super.longitude = value;
    });
  }

  late final _$carregandoAtom = Atom(
    name: '_RotaMonitoramentoControllerBase.carregando',
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

  late final _$mensagemErroAtom = Atom(
    name: '_RotaMonitoramentoControllerBase.mensagemErro',
    context: context,
  );

  @override
  String? get mensagemErro {
    _$mensagemErroAtom.reportRead();
    return super.mensagemErro;
  }

  @override
  set mensagemErro(String? value) {
    _$mensagemErroAtom.reportWrite(value, super.mensagemErro, () {
      super.mensagemErro = value;
    });
  }

  late final _$iniciarMonitoramentoAsyncAction = AsyncAction(
    '_RotaMonitoramentoControllerBase.iniciarMonitoramento',
    context: context,
  );

  @override
  Future<void> iniciarMonitoramento(int rotaId) {
    return _$iniciarMonitoramentoAsyncAction.run(
      () => super.iniciarMonitoramento(rotaId),
    );
  }

  late final _$_atualizarPosicaoAsyncAction = AsyncAction(
    '_RotaMonitoramentoControllerBase._atualizarPosicao',
    context: context,
  );

  @override
  Future<void> _atualizarPosicao(int rotaId) {
    return _$_atualizarPosicaoAsyncAction.run(
      () => super._atualizarPosicao(rotaId),
    );
  }

  @override
  String toString() {
    return '''
latitude: ${latitude},
longitude: ${longitude},
carregando: ${carregando},
mensagemErro: ${mensagemErro}
    ''';
  }
}
