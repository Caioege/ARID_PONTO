import 'package:arid_motorista/ioc/service_locator.dart';

import 'session_manager.dart';

/// Classe utilitária para verificar estado de autenticação
/// Usada principalmente por rotas, guards e decisões de navegação
class AuthGuard {
  final _session = locator<SessionManager>();

  /// Indica se existe uma sessão válida carregada
  bool get estaLogado => _session.estaLogado;

  /// Retorna o usuário logado (se existir)
  /// Útil para telas de perfil, drawer, etc
  // ignore: strict_top_level_inference
  get usuario => _session.usuario;
}
