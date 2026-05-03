import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'app/app_widget.dart';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';

final navigatorKey = GlobalKey<NavigatorState>();

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  await dotenv.load(fileName: '.env');

  FlutterForegroundTask.init(
    androidNotificationOptions: AndroidNotificationOptions(
      channelId: 'rota_execucao_channel_id',
      channelName: 'Rota em execução',
      channelDescription:
          'Notificação fixa exibida enquanto uma rota está em execução.',
      channelImportance: NotificationChannelImportance.LOW,
      priority: NotificationPriority.LOW,
      showWhen: true,
      showBadge: true,
      onlyAlertOnce: true,
    ),
    iosNotificationOptions: const IOSNotificationOptions(
      showNotification: true,
      playSound: false,
    ),
    foregroundTaskOptions: ForegroundTaskOptions(
      eventAction: ForegroundTaskEventAction.repeat(10000),
      autoRunOnBoot: false,
      allowWakeLock: true,
      allowWifiLock: true,
    ),
  );

  setupLocator();

  runApp(const AppWidget());
}
