import 'dart:io';
import 'package:dio/dio.dart';
import 'package:dio/io.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:arid_motorista/core/http/error_interceptor.dart';
import 'auth_interceptor.dart';

class AppHttpClient {
  static final AppHttpClient _instance = AppHttpClient._internal();
  late final Dio dio;

  factory AppHttpClient() => _instance;

  AppHttpClient._internal() {
    dio = Dio(
      BaseOptions(
        baseUrl: dotenv.env['URL_BASE'] ?? '',
        connectTimeout: const Duration(seconds: 20),
        receiveTimeout: const Duration(seconds: 20),
      ),
    );

    dio.interceptors.add(AuthInterceptor());
    dio.interceptors.add(ErrorInterceptor());

    dio.httpClientAdapter = IOHttpClientAdapter(
      createHttpClient: () {
        final client = HttpClient(
          context: SecurityContext(withTrustedRoots: false),
        );

        client.badCertificateCallback =
            (X509Certificate cert, String host, int port) => true;

        return client;
      },
    );
  }
}
