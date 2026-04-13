import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:dio/dio.dart';
import '../auth/session_manager.dart';
import '../auth/auth_refresh_service.dart';
import '../exception/validacao_server.dart';
import '../http/http_client.dart';

class ErrorInterceptor extends Interceptor {
  final _sessionManager = locator<SessionManager>();
  final _refreshService = AuthRefreshService();

  bool ehRefresh = false;

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    final response = err.response;

    // TOKEN EXPIRADO → tenta refresh
    if (response?.statusCode == 401 && !ehRefresh) {
      ehRefresh = true;

      final newToken = await _refreshService.refreshToken();

      if (newToken != null) {
        await _sessionManager.atualizarToken(newToken);

        final requestOptions = err.requestOptions;
        requestOptions.headers['Authorization'] = 'Bearer $newToken';

        final dio = locator<AppHttpClient>().dio;

        final retryResponse = await dio.fetch(requestOptions);

        ehRefresh = false;
        return handler.resolve(retryResponse);
      }

      ehRefresh = false;
      await _sessionManager.limparSessao();

      return handler.reject(
        DioException(
          requestOptions: err.requestOptions,
          error: ValidacaoServer(
            sucesso: false,
            mensagem: 'Sua sessão expirou. Faça login novamente.',
          ),
        ),
      );
    }

    if (response?.data is Map<String, dynamic>) {
      return handler.reject(
        DioException(
          requestOptions: err.requestOptions,
          error: ValidacaoServer.fromMap(response!.data),
        ),
      );
    }

    return handler.reject(
      DioException(
        requestOptions: err.requestOptions,
        error: ValidacaoServer.erroGenerico(),
      ),
    );
  }
}

