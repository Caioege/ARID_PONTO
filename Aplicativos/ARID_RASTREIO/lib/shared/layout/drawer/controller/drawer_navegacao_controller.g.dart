// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'drawer_navegacao_controller.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic, no_leading_underscores_for_local_identifiers

mixin _$DrawerNavegacaoController on DrawerNavegacaoControllerBase, Store {
  late final _$selectedIndexAtom = Atom(
    name: 'DrawerNavegacaoControllerBase.selectedIndex',
    context: context,
  );

  @override
  int get selectedIndex {
    _$selectedIndexAtom.reportRead();
    return super.selectedIndex;
  }

  @override
  set selectedIndex(int value) {
    _$selectedIndexAtom.reportWrite(value, super.selectedIndex, () {
      super.selectedIndex = value;
    });
  }

  late final _$menusSuperiorDisponiveisAtom = Atom(
    name: 'DrawerNavegacaoControllerBase.menusSuperiorDisponiveis',
    context: context,
  );

  @override
  List<DrawerMenuItem> get menusSuperiorDisponiveis {
    _$menusSuperiorDisponiveisAtom.reportRead();
    return super.menusSuperiorDisponiveis;
  }

  @override
  set menusSuperiorDisponiveis(List<DrawerMenuItem> value) {
    _$menusSuperiorDisponiveisAtom.reportWrite(
      value,
      super.menusSuperiorDisponiveis,
      () {
        super.menusSuperiorDisponiveis = value;
      },
    );
  }

  late final _$DrawerNavegacaoControllerBaseActionController = ActionController(
    name: 'DrawerNavegacaoControllerBase',
    context: context,
  );

  @override
  void mudarIndex(int novoIndex) {
    final _$actionInfo = _$DrawerNavegacaoControllerBaseActionController
        .startAction(name: 'DrawerNavegacaoControllerBase.mudarIndex');
    try {
      return super.mudarIndex(novoIndex);
    } finally {
      _$DrawerNavegacaoControllerBaseActionController.endAction(_$actionInfo);
    }
  }

  @override
  void limpar() {
    final _$actionInfo = _$DrawerNavegacaoControllerBaseActionController
        .startAction(name: 'DrawerNavegacaoControllerBase.limpar');
    try {
      return super.limpar();
    } finally {
      _$DrawerNavegacaoControllerBaseActionController.endAction(_$actionInfo);
    }
  }

  @override
  String toString() {
    return '''
selectedIndex: ${selectedIndex},
menusSuperiorDisponiveis: ${menusSuperiorDisponiveis}
    ''';
  }
}
