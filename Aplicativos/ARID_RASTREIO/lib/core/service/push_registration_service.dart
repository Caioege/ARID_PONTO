import 'dart:io';

import 'package:arid_rastreio/core/http/http_client.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';

class PushRegistrationService {
  final _client = locator<AppHttpClient>().dio;
  bool _escutandoRefreshToken = false;

  Future<void> registrarTokenSeDisponivel() async {
    if (kIsWeb) return;
    if (!(Platform.isAndroid || Platform.isIOS)) return;

    try {
      if (Firebase.apps.isEmpty) {
        await Firebase.initializeApp();
      }

      final messaging = FirebaseMessaging.instance;
      await messaging.requestPermission();

      final token = await messaging.getToken();
      if (token == null || token.isEmpty) return;

      await _registrarToken(token);
      _escutarRefreshToken();
    } catch (e) {
      debugPrint('[PUSH] Nao foi possivel registrar o token: $e');
    }
  }

  void _escutarRefreshToken() {
    if (_escutandoRefreshToken) return;

    _escutandoRefreshToken = true;
    FirebaseMessaging.instance.onTokenRefresh.listen((token) async {
      try {
        await _registrarToken(token);
      } catch (e) {
        debugPrint('[PUSH] Nao foi possivel atualizar o token: $e');
      }
    });
  }

  Future<void> _registrarToken(String token) async {
    await _client.post(
      '/api/rastreio-app/registrar-token',
      data: {'token': token, 'plataforma': Platform.operatingSystem},
    );
  }
}
