import 'package:arid_motorista/core/http/http_client.dart';
import 'package:arid_motorista/ioc/service_locator.dart';
import 'package:arid_motorista/modules/login/dto/auth_response_dto.dart';
import 'package:arid_motorista/modules/login/dto/usuario_dto.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

class LoginService {
  final _client = locator<AppHttpClient>().dio;

  /// Realiza login do usuário
  /// Pode ser FAKE (mock) ou REAL (API),
  /// dependendo da variável USE_FAKE_LOGIN no .env
  Future<AuthResponseDTO> login(String login, String senha) async {
    final useFakeLogin = dotenv.env['USE_FAKE_LOGIN']?.toLowerCase() == 'true';

    // 🔹 CENÁRIO FAKE (mock local)
    if (useFakeLogin) {
      // Simula um pequeno delay de rede
      await Future.delayed(const Duration(milliseconds: 800));

      // Usuário mockado (depois você ajusta os campos)
      final usuarioMock = UsuarioDTO(
        id: 1,
        nome: 'Motorista Teste',
        login: login,
        foto: null,
      );

      // Retorna como se fosse resposta da API
      return AuthResponseDTO(
        token: 'fake-token-arid-motorista',
        usuario: usuarioMock,
      );
    }

    // 🔹 CENÁRIO REAL (API)
    final response = await _client.post(
      '/api/rota-app/autentique',
      data: {'login': login, 'senha': senha},
    );

    return AuthResponseDTO.fromJson(response.data);
  }
}
