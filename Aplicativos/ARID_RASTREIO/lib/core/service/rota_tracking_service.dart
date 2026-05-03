import 'package:flutter/material.dart';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';

import 'rota_location_task.dart';

// ignore_for_file: avoid_classes_with_only_static_members
class RotaTrackingService {
  static const String abrirRotaButtonId = 'abrir_rota';

  static Future<void> start({
    String? descricaoRota,
    bool execucaoOffline = false,
  }) async {
    final descricao = _normalizarDescricao(descricaoRota);

    await FlutterForegroundTask.saveData(
      key: 'rotaDescricao',
      value: descricao,
    );
    await FlutterForegroundTask.saveData(
      key: 'execucaoOffline',
      value: execucaoOffline.toString(),
    );

    final notificationTitle = 'Rota em execução';
    final notificationText = notificationTextFor(
      descricaoRota: descricao,
      execucaoOffline: execucaoOffline,
      frame: 0,
    );
    final notificationButtons = [
      const NotificationButton(
        id: abrirRotaButtonId,
        text: 'Abrir rota',
        textColor: Colors.green,
      ),
    ];

    if (await FlutterForegroundTask.isRunningService) {
      await FlutterForegroundTask.updateService(
        notificationTitle: notificationTitle,
        notificationText: notificationText,
        notificationButtons: notificationButtons,
        notificationInitialRoute: '/',
      );
      return;
    }

    await FlutterForegroundTask.startService(
      serviceTypes: const [ForegroundServiceTypes.location],
      notificationTitle: notificationTitle,
      notificationText: notificationText,
      notificationButtons: notificationButtons,
      notificationInitialRoute: '/',
      callback: startCallback,
    );
  }

  static Future<void> stop() async {
    await FlutterForegroundTask.stopService();
    await FlutterForegroundTask.removeData(key: 'rotaDescricao');
  }

  static String notificationTextFor({
    required String descricaoRota,
    required bool execucaoOffline,
    required int frame,
  }) {
    const frames = ['.', '..', '...'];
    final progresso = frames[frame % frames.length];
    final status = execucaoOffline
        ? 'salvando localmente'
        : 'enviando localização';

    return '$descricaoRota em segundo plano, $status$progresso';
  }

  static String _normalizarDescricao(String? descricaoRota) {
    final descricao = descricaoRota?.trim();
    if (descricao == null || descricao.isEmpty) return 'Rota atual';
    return descricao;
  }
}
