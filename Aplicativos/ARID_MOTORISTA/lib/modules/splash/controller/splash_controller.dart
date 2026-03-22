import 'package:arid_motorista/ioc/service_locator.dart';

import '../../../core/auth/session_manager.dart';

class SplashController {
  final _sessionManager = locator<SessionManager>();

  Future<String> getNextRoute() async {
    // Garante que a sessão foi carregada (token, user, etc)
    if (!_sessionManager.estaInicializado) {
      await _sessionManager.carregarSessao();
    }

    // Mantém o tempo mínimo do splash (UX)
    await Future.delayed(const Duration(milliseconds: 1600));

    if (!_sessionManager.estaLogado) {
      return '/login';
    }

    return '/home';
  }
}
