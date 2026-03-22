import 'package:arid_motorista/ioc/service_locator.dart';
import 'package:arid_motorista/modules/motorista/home/motorista_home_page.dart';
import 'package:arid_motorista/modules/motorista/splash/page/motorista_splash_page.dart';
import 'package:go_router/go_router.dart';
import '../core/auth/session_manager.dart';
import '../modules/splash/pages/splash_page.dart';
import '../modules/login/pages/login_page.dart';

// ignore_for_file: unnecessary_underscores

class AppRoutes {
  static final SessionManager sessionManager = locator<SessionManager>();

  static final GoRouter router = GoRouter(
    initialLocation: '/',
    refreshListenable: sessionManager,

    routes: [
      GoRoute(
        path: '/',
        builder: (_, __) => const SplashPage(), // Splash Global
      ),
      GoRoute(path: '/login', builder: (_, __) => const LoginPage()),
      GoRoute(
        path: '/motorista/splash',
        builder: (_, __) => const MotoristaSplashPage(),
      ),
      GoRoute(
        path: '/motorista/home',
        builder: (_, __) => const MotoristaHomePage(),
      ),
    ],

    redirect: (context, state) {
      final session = sessionManager;
      final location = state.matchedLocation;

      // 1️⃣ Ainda carregando sessão
      if (!session.estaInicializado) {
        return location == '/' ? null : '/';
      }

      // 2️⃣ Não logado
      if (!session.estaLogado) {
        return location == '/login' ? null : '/login';
      }

      // 3️⃣ Logado → sai do splash global ou login
      if (location == '/' || location == '/login') {
        return '/motorista/splash';
      }

      // 4️⃣ Splash do motorista finalizado → Home
      if (location == '/motorista/splash' &&
          session.splashMotoristaFinalizado) {
        return '/motorista/home';
      }

      return null;
    },
  );
}
