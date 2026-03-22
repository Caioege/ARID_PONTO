// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'motorista_splash_controller.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic, no_leading_underscores_for_local_identifiers

mixin _$MotoristaSplashController on MotoristaSplashControllerBase, Store {
  Computed<StatusRequest>? _$statusSplashComputed;

  @override
  StatusRequest get statusSplash =>
      (_$statusSplashComputed ??= Computed<StatusRequest>(
        () => super.statusSplash,
        name: 'MotoristaSplashControllerBase.statusSplash',
      )).value;

  late final _$splashObserverAtom = Atom(
    name: 'MotoristaSplashControllerBase.splashObserver',
    context: context,
  );

  @override
  ObservableFuture<void>? get splashObserver {
    _$splashObserverAtom.reportRead();
    return super.splashObserver;
  }

  @override
  set splashObserver(ObservableFuture<void>? value) {
    _$splashObserverAtom.reportWrite(value, super.splashObserver, () {
      super.splashObserver = value;
    });
  }

  late final _$erroProcessamentoAtom = Atom(
    name: 'MotoristaSplashControllerBase.erroProcessamento',
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

  late final _$initializeAsyncAction = AsyncAction(
    'MotoristaSplashControllerBase.initialize',
    context: context,
  );

  @override
  Future<void> initialize() {
    return _$initializeAsyncAction.run(() => super.initialize());
  }

  @override
  String toString() {
    return '''
splashObserver: ${splashObserver},
erroProcessamento: ${erroProcessamento},
statusSplash: ${statusSplash}
    ''';
  }
}
