import 'package:flutter_foreground_task/flutter_foreground_task.dart';
import 'rota_location_task.dart';

// ignore_for_file: avoid_classes_with_only_static_members
class RotaTrackingService {
  static Future<void> start() async {
    await FlutterForegroundTask.startService(
      notificationTitle: 'Rota em andamento',
      notificationText: 'Enviando localização…',
      callback: startCallback,
    );
  }

  static Future<void> stop() async {
    await FlutterForegroundTask.stopService();
  }
}
