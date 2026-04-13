// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'parada_store.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic, no_leading_underscores_for_local_identifiers

mixin _$ParadaStore on ParadaStoreBase, Store {
  late final _$entregueAtom = Atom(
    name: 'ParadaStoreBase.entregue',
    context: context,
  );

  @override
  bool? get entregue {
    _$entregueAtom.reportRead();
    return super.entregue;
  }

  @override
  set entregue(bool? value) {
    _$entregueAtom.reportWrite(value, super.entregue, () {
      super.entregue = value;
    });
  }

  late final _$observacaoAtom = Atom(
    name: 'ParadaStoreBase.observacao',
    context: context,
  );

  @override
  String get observacao {
    _$observacaoAtom.reportRead();
    return super.observacao;
  }

  @override
  set observacao(String value) {
    _$observacaoAtom.reportWrite(value, super.observacao, () {
      super.observacao = value;
    });
  }

  late final _$salvandoAtom = Atom(
    name: 'ParadaStoreBase.salvando',
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

  late final _$confirmadaAtom = Atom(
    name: 'ParadaStoreBase.confirmada',
    context: context,
  );

  @override
  bool get confirmada {
    _$confirmadaAtom.reportRead();
    return super.confirmada;
  }

  @override
  set confirmada(bool value) {
    _$confirmadaAtom.reportWrite(value, super.confirmada, () {
      super.confirmada = value;
    });
  }

  @override
  String toString() {
    return '''
entregue: ${entregue},
observacao: ${observacao},
salvando: ${salvando},
confirmada: ${confirmada}
    ''';
  }
}
