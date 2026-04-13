import 'package:mobx/mobx.dart';
import 'package:flutter/material.dart';

part 'parada_store.g.dart';

class ParadaStore = ParadaStoreBase with _$ParadaStore;

abstract class ParadaStoreBase with Store {
  final int id;
  final String endereco;
  final double? latitude;
  final double? longitude;
  final String? link;

  @observable
  bool? entregue;

  @observable
  String observacao = '';

  @observable
  bool salvando = false;

  @observable
  bool confirmada = false;

  late final TextEditingController controllerObservacao;

  ParadaStoreBase({
    required this.id,
    required this.endereco,
    this.latitude,
    this.longitude,
    this.link,
    this.entregue,
  }) {
    controllerObservacao = TextEditingController(text: observacao);

    controllerObservacao.addListener(() {
      observacao = controllerObservacao.text;
    });
  }
}
