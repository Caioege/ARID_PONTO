import 'dart:async';
import 'package:arid_rastreio/modules/acompanhante/service/acompanhante_service.dart';
import 'package:flutter/material.dart';
import 'package:mobx/mobx.dart';

part 'rota_monitoramento_controller.g.dart';

class RotaMonitoramentoController = _RotaMonitoramentoControllerBase with _$RotaMonitoramentoController;

abstract class _RotaMonitoramentoControllerBase with Store {
  final _service = AcompanhanteService();

  @observable
  double? latitude;

  @observable
  double? longitude;

  @observable
  bool carregando = false;

  @observable
  String? mensagemErro;

  Timer? _timer;

  @action
  Future<void> iniciarMonitoramento(int rotaId) async {
    carregando = true;
    mensagemErro = null;

    // Busca imediata
    await _atualizarPosicao(rotaId);
    
    carregando = false;

    // Polling a cada 30 segundos
    _timer = Timer.periodic(const Duration(seconds: 30), (timer) async {
      await _atualizarPosicao(rotaId);
    });
  }

  @action
  Future<void> _atualizarPosicao(int rotaId) async {
    try {
      final pos = await _service.obterUltimaLocalizacao(rotaId);
      if (pos != null) {
        latitude = double.tryParse(pos['latitude']?.toString() ?? '');
        longitude = double.tryParse(pos['longitude']?.toString() ?? '');
      } else {
        mensagemErro = 'Nenhuma localização disponível no momento.';
      }
    } catch (e) {
      mensagemErro = 'Erro ao atualizar posição: $e';
    }
  }

  void pararMonitoramento() {
    _timer?.cancel();
  }
}
