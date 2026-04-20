import 'package:arid_rastreio/modules/login/dto/usuario_dto.dart';
import 'package:arid_rastreio/modules/login/services/login_service.dart';
import 'package:arid_rastreio/core/exception/validacao_server.dart';
import 'package:dio/dio.dart';
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
    try {
      final response = await _service.login(login, senha, tipoAcesso: tipoAcesso);
      return LoginResult(token: response.token, usuario: response.usuario);
    } on DioException catch (e) {
      if (e.error is ValidacaoServer) {
        throw (e.error as ValidacaoServer).mensagem;
      }
      throw e.message ?? 'Erro desconhecido de conexão';
    } catch (e) {
      throw e.toString();
    }
  }
}

/// Objeto simples para transportar o resultado do login
class LoginResult {
  final String token;
  final UsuarioDTO usuario;

  LoginResult({required this.token, required this.usuario});
}
