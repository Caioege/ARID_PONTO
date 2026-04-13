import 'dart:async';
import 'dart:math';
import 'package:arid_rastreio/core/service/rota_background_service.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';
import 'package:geolocator/geolocator.dart';
import 'package:timezone/data/latest.dart' as tz;
import 'package:timezone/timezone.dart' as tz;

@pragma('vm:entry-point')
void startCallback() {
  FlutterForegroundTask.setTaskHandler(LocationTaskHandler());
}

class LocationTaskHandler extends TaskHandler {
  StreamSubscription<Position>? _positionSub;

  @override
  Future<void> onStart(DateTime timestamp, TaskStarter starter) async {
    WidgetsFlutterBinding.ensureInitialized();

    tz.initializeTimeZones();
    tz.setLocalLocation(tz.getLocation('America/Sao_Paulo'));

    await dotenv.load(fileName: '.env');

    setupLocator();

    final rotaExecucaoIdStr = await FlutterForegroundTask.getData(
      key: 'rotaExecucaoId',
    );

    final rotaExecucaoId = int.tryParse(rotaExecucaoIdStr ?? '');

    if (rotaExecucaoId == null) return;

    final backgroundService = locator<RotaBackgroundService>();

    Position? ultimaPosicaoSalva;

    _positionSub =
        Geolocator.getPositionStream(
          locationSettings: const LocationSettings(
            accuracy: LocationAccuracy.bestForNavigation,
            distanceFilter: 3,
          ),
        ).listen((pos) async {
          if (!deveSalvarPosicao(
            posicaoAtual: pos,
            ultimaPosicao: ultimaPosicaoSalva,
          )) {
            return;
          }

          try {
            await backgroundService.enviarPonto(
              rotaExecucaoId: rotaExecucaoId,
              latitude: pos.latitude,
              longitude: pos.longitude,
              dataHora: tz.TZDateTime.now(tz.local),
            );
          } catch (_) {}

          ultimaPosicaoSalva = pos;
        });
  }

  @override
  Future<void> onRepeatEvent(DateTime timestamp) async {
    // agora não precisa mais chamar manualmente
    // porque o stream já emite quando há mudança
  }

  @override
  Future<void> onDestroy(DateTime timestamp, bool isTimeout) async {
    await _positionSub?.cancel();
  }

  @override
  void onNotificationPressed() {
    FlutterForegroundTask.launchApp('/');
  }

  double distanciaEmMetros(double lat1, double lon1, double lat2, double lon2) {
    const R = 6371000.0;
    double toRad(double deg) => deg * pi / 180.0;
    final dLat = toRad(lat2 - lat1);
    final dLon = toRad(lon2 - lon1);
    final a =
        sin(dLat / 2) * sin(dLat / 2) +
        cos(toRad(lat1)) * cos(toRad(lat2)) * sin(dLon / 2) * sin(dLon / 2);
    final c = 2 * atan2(sqrt(a), sqrt(1 - a));
    return R * c;
  }

  bool deveSalvarPosicao({
    required Position posicaoAtual,
    required Position? ultimaPosicao,
    double maximaPrecisaoEmMetros = 40.0,
    double velocidadeMaxima = 40.0,
    int maxOldMillis = 5000,
  }) {
    if (posicaoAtual.accuracy > maximaPrecisaoEmMetros) {
      return false;
    }

    final posTs = posicaoAtual.timestamp;

    if (DateTime.now().difference(posTs).inMilliseconds > maxOldMillis) {
      return false;
    }

    if (ultimaPosicao == null) {
      return true;
    }

    final dist = distanciaEmMetros(
      ultimaPosicao.latitude,
      ultimaPosicao.longitude,
      posicaoAtual.latitude,
      posicaoAtual.longitude,
    );
    final timeDiff =
        posicaoAtual.timestamp
            .difference(ultimaPosicao.timestamp)
            .inMilliseconds /
        1000.0;

    if (timeDiff <= 0) {
      return false;
    }

    final requiredSpeed = dist / timeDiff;

    if (requiredSpeed > velocidadeMaxima) {
      return false;
    }

    return true;
  }
}

