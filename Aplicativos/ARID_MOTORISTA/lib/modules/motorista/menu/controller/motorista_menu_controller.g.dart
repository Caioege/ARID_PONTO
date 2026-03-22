// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'motorista_menu_controller.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic, no_leading_underscores_for_local_identifiers

mixin _$MotoristaMenuController on MotoristaMenuBaseController, Store {
  late final _$erroProcessamentoAtom = Atom(
    name: 'MotoristaMenuBaseController.erroProcessamento',
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

  late final _$selectedindexAtom = Atom(
    name: 'MotoristaMenuBaseController.selectedindex',
    context: context,
  );

  @override
  int get selectedindex {
    _$selectedindexAtom.reportRead();
    return super.selectedindex;
  }

  @override
  set selectedindex(int value) {
    _$selectedindexAtom.reportWrite(value, super.selectedindex, () {
      super.selectedindex = value;
    });
  }

  late final _$MotoristaMenuBaseControllerActionController = ActionController(
    name: 'MotoristaMenuBaseController',
    context: context,
  );

  @override
  void mudarIndex(int index) {
    final _$actionInfo = _$MotoristaMenuBaseControllerActionController
        .startAction(name: 'MotoristaMenuBaseController.mudarIndex');
    try {
      return super.mudarIndex(index);
    } finally {
      _$MotoristaMenuBaseControllerActionController.endAction(_$actionInfo);
    }
  }

  @override
  void retornarTela(int index) {
    final _$actionInfo = _$MotoristaMenuBaseControllerActionController
        .startAction(name: 'MotoristaMenuBaseController.retornarTela');
    try {
      return super.retornarTela(index);
    } finally {
      _$MotoristaMenuBaseControllerActionController.endAction(_$actionInfo);
    }
  }

  @override
  void limpar() {
    final _$actionInfo = _$MotoristaMenuBaseControllerActionController
        .startAction(name: 'MotoristaMenuBaseController.limpar');
    try {
      return super.limpar();
    } finally {
      _$MotoristaMenuBaseControllerActionController.endAction(_$actionInfo);
    }
  }

  @override
  String toString() {
    return '''
erroProcessamento: ${erroProcessamento},
selectedindex: ${selectedindex}
    ''';
  }
}
