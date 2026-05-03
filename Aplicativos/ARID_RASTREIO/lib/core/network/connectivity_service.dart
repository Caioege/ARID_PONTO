import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:dio/dio.dart';
import '../exception/validacao_server.dart';
import '../http/http_client.dart';

class ConnectivityService {
  static Future<bool> isConnected() async {
    final result = await Connectivity().checkConnectivity();
    if (result.contains(ConnectivityResult.none)) return false;

    return _backendRespondendo();
  }

  static Future<void> ensureConnected() async {
    if (!await isConnected()) {
      throw ValidacaoServer.erroConexao();
    }
  }

  static Future<bool> _backendRespondendo() async {
    try {
      final dio = AppHttpClient().dio;
      final response = await dio.get(
        '/api/rastreio-app/conectividade',
        options: Options(
          sendTimeout: const Duration(seconds: 3),
          receiveTimeout: const Duration(seconds: 3),
        ),
      );

      return response.statusCode != null &&
          response.statusCode! >= 200 &&
          response.statusCode! < 300;
    } catch (_) {
      return false;
    }
  }
}
