import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:dio/dio.dart';
import '../auth/session_manager.dart';

class AuthInterceptor extends Interceptor {
  final _sessionManager = locator<SessionManager>();

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    final token = _sessionManager.token;

    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }

    options.headers['Content-Type'] = 'application/json';
    options.headers['Accept'] = 'application/json';

    super.onRequest(options, handler);
  }
}

