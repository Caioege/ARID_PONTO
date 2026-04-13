import 'package:arid_rastreio/modules/login/dto/usuario_dto.dart';
import 'package:arid_rastreio/modules/login/services/login_service.dart';

class LoginController {
  // Service responsável por autenticação (fake ou real)
  final LoginService _service = LoginService();

  /// Realiza o login do usuário
  /// Independentemente de ser fake ou API real,
  /// o retorno sempre simula um login válido
  Future<LoginResult> login({
    required String login,
    required String senha,
    String? tipoAcesso,
  }) async {
    // Chamada ao service (decide internamente se é fake ou real)
    final response = await _service.login(login, senha, tipoAcesso: tipoAcesso);

    // Retorna token + usuário como se tivesse vindo da API
    return LoginResult(token: response.token, usuario: response.usuario);
  }
}

/// Objeto simples para transportar o resultado do login
class LoginResult {
  final String token;
  final UsuarioDTO usuario;

  LoginResult({required this.token, required this.usuario});
}
