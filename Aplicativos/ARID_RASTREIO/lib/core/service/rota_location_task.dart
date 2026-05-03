import 'dart:async';
import 'dart:math';
import 'package:arid_rastreio/core/service/rota_background_service.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/core/auth/session_manager.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';
import 'package:geolocator/geolocator.dart';
import 'package:timezone/data/latest.dart' as tz;
import 'package:timezone/timezone.dart' as tz;

const Duration _idadeMaximaPosicaoConhecida = Duration(seconds: 60);

@pragma('vm:entry-point')
void startCallback() {
  FlutterForegroundTask.setTaskHandler(LocationTaskHandler());
}

class LocationTaskHandler extends TaskHandler {
  StreamSubscription<Position>? _positionSub;
  int _notificationFrame = 0;

  @override
  Future<void> onStart(DateTime timestamp, TaskStarter starter) async {
    WidgetsFlutterBinding.ensureInitialized();

    tz.initializeTimeZones();
    tz.setLocalLocation(tz.getLocation('America/Sao_Paulo'));

    await dotenv.load(fileName: '.env');

    setupLocator();

    // O Isolate de background não compartilha a memória do Isolate principal.
    // É necessário carregar a sessão (token) do storage para autenticar as requisições.
    final session = locator<SessionManager>();
    await session.carregarSessao();

    final rotaExecucaoIdStr = await FlutterForegroundTask.getData(
      key: 'rotaExecucaoId',
    );
    final localExecucaoId = await FlutterForegroundTask.getData(
      key: 'localExecucaoId',
    );
    final execucaoOfflineStr = await FlutterForegroundTask.getData(
      key: 'execucaoOffline',
    );
    final rotaDescricao = await FlutterForegroundTask.getData(
      key: 'rotaDescricao',
    );

    final rotaExecucaoId = int.tryParse(rotaExecucaoIdStr ?? '');
    final execucaoOffline = execucaoOfflineStr == 'true';
    await _atualizarNotificacao(
      descricaoRota: rotaDescricao,
      execucaoOffline: execucaoOffline,
    );

    if (!execucaoOffline && rotaExecucaoId == null) return;
    if (execucaoOffline &&
        (localExecucaoId == null || localExecucaoId.isEmpty)) {
      return;
    }

    final backgroundService = locator<RotaBackgroundService>();

    Position? ultimaPosicaoSalva;

    _positionSub =
        Geolocator.getPositionStream(
          locationSettings: const LocationSettings(
            accuracy: LocationAccuracy.bestForNavigation,
            distanceFilter: 2,
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
              localExecucaoId: localExecucaoId,
              execucaoOffline: execucaoOffline,
              latitude: pos.latitude,
              longitude: pos.longitude,
              dataHora: tz.TZDateTime.now(tz.local),
              gpsSimulado: pos.isMocked,
              precisaoEmMetros: pos.accuracy,
              velocidadeMetrosPorSegundo: pos.speed,
              direcaoGraus: pos.heading,
              altitudeMetros: pos.altitude,
              fonteCaptura: 1,
            );
          } catch (e, stack) {
            debugPrint('[DEBUG] Background Task: falha ao salvar ponto: $e');
            debugPrintStack(stackTrace: stack);
          }

          ultimaPosicaoSalva = pos;
        });
  }

  @override
  Future<void> onRepeatEvent(DateTime timestamp) async {
    final rotaExecucaoIdStr = await FlutterForegroundTask.getData(
      key: 'rotaExecucaoId',
    );
    final localExecucaoId = await FlutterForegroundTask.getData(
      key: 'localExecucaoId',
    );
    final execucaoOfflineStr = await FlutterForegroundTask.getData(
      key: 'execucaoOffline',
    );
    final rotaDescricao = await FlutterForegroundTask.getData(
      key: 'rotaDescricao',
    );

    final rotaExecucaoId = int.tryParse(rotaExecucaoIdStr ?? '');
    final execucaoOffline = execucaoOfflineStr == 'true';
    if (!execucaoOffline && rotaExecucaoId == null) return;
    if (execucaoOffline &&
        (localExecucaoId == null || localExecucaoId.isEmpty)) {
      return;
    }
    await _atualizarNotificacao(
      descricaoRota: rotaDescricao,
      execucaoOffline: execucaoOffline,
    );

    try {
      Position? ultimaPosicao;
      try {
        ultimaPosicao = await Geolocator.getLastKnownPosition();
      } catch (_) {}

      Position? pos;
      if (ultimaPosicao != null &&
          DateTime.now().difference(ultimaPosicao.timestamp) <=
              _idadeMaximaPosicaoConhecida) {
        pos = ultimaPosicao;
      } else {
        try {
          pos = await Geolocator.getCurrentPosition(
            locationSettings: const LocationSettings(
              accuracy: LocationAccuracy.bestForNavigation,
              timeLimit: Duration(seconds: 15),
            ),
          );
        } catch (e) {
          if (!kDebugMode && ultimaPosicao != null) {
            pos = ultimaPosicao;
          }
        }
      }

      if (pos == null) return;

      final backgroundService = locator<RotaBackgroundService>();

      await backgroundService.enviarPonto(
        rotaExecucaoId: rotaExecucaoId,
        localExecucaoId: localExecucaoId,
        execucaoOffline: execucaoOffline,
        latitude: pos.latitude,
        longitude: pos.longitude,
        dataHora: DateTime.now(),
        gpsSimulado: pos.isMocked,
        precisaoEmMetros: pos.accuracy,
        velocidadeMetrosPorSegundo: pos.speed,
        direcaoGraus: pos.heading,
        altitudeMetros: pos.altitude,
        fonteCaptura: 1,
      );

      debugPrint('[DEBUG] Background Task: Ponto salvo com sucesso.');
    } catch (e, stack) {
      debugPrint('[DEBUG] Background Task: falha no evento periodico: $e');
      debugPrintStack(stackTrace: stack);
    }
  }

  @override
  Future<void> onDestroy(DateTime timestamp, bool isTimeout) async {
    await _positionSub?.cancel();
  }

  @override
  void onNotificationPressed() {
    FlutterForegroundTask.launchApp('/');
  }

  @override
  void onNotificationButtonPressed(String id) {
    if (id == 'abrir_rota') {
      FlutterForegroundTask.launchApp('/');
    }
  }

  @override
  void onNotificationDismissed() {
    unawaited(_restaurarNotificacaoSeRotaAtiva());
  }

  Future<void> _restaurarNotificacaoSeRotaAtiva() async {
    final rotaExecucaoIdStr = await FlutterForegroundTask.getData(
      key: 'rotaExecucaoId',
    );
    final localExecucaoId = await FlutterForegroundTask.getData(
      key: 'localExecucaoId',
    );
    final execucaoOfflineStr = await FlutterForegroundTask.getData(
      key: 'execucaoOffline',
    );
    final rotaDescricao = await FlutterForegroundTask.getData(
      key: 'rotaDescricao',
    );

    final rotaExecucaoId = int.tryParse(rotaExecucaoIdStr ?? '');
    final execucaoOffline = execucaoOfflineStr == 'true';
    final rotaOnlineAtiva = !execucaoOffline && rotaExecucaoId != null;
    final rotaOfflineAtiva =
        execucaoOffline &&
        localExecucaoId != null &&
        localExecucaoId.isNotEmpty;

    if (!rotaOnlineAtiva && !rotaOfflineAtiva) return;

    await _atualizarNotificacao(
      descricaoRota: rotaDescricao,
      execucaoOffline: execucaoOffline,
    );
  }

  Future<void> _atualizarNotificacao({
    required String? descricaoRota,
    required bool execucaoOffline,
  }) async {
    const frames = ['.', '..', '...'];
    final descricao = descricaoRota == null || descricaoRota.trim().isEmpty
        ? 'Rota atual'
        : descricaoRota.trim();
    final status = execucaoOffline
        ? 'salvando localmente'
        : 'enviando localização';
    final progresso = frames[_notificationFrame % frames.length];
    _notificationFrame++;

    await FlutterForegroundTask.updateService(
      notificationTitle: 'Rota em execução',
      notificationText: '$descricao em segundo plano, $status$progresso',
      notificationButtons: const [
        NotificationButton(
          id: 'abrir_rota',
          text: 'Abrir rota',
          textColor: Color(0xFF2E7D32),
        ),
      ],
      notificationInitialRoute: '/',
    );
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
    if (kDebugMode) return true;

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
