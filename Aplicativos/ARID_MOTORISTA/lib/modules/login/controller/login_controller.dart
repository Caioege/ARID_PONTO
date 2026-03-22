import 'package:arid_motorista/modules/login/dto/usuario_dto.dart';
import 'package:arid_motorista/modules/login/services/login_service.dart';

class LoginController {
  // Service responsável por autenticação (fake ou real)
  final LoginService _service = LoginService();

  /// Realiza o login do usuário
  /// Independentemente de ser fake ou API real,
  /// o retorno sempre simula um login válido
  Future<LoginResult> login({
    required String login,
    required String senha,
  }) async {
    // Chamada ao service (decide internamente se é fake ou real)
    final response = await _service.login(login, senha);

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
