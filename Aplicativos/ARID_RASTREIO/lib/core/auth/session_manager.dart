import 'dart:convert';

import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/login/dto/usuario_dto.dart';
import 'package:arid_rastreio/shared/layout/drawer/controller/drawer_navegacao_controller.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:package_info_plus/package_info_plus.dart';

/// Gerencia toda a sessão do usuário logado
/// Responsável por:
/// - armazenar token
/// - armazenar usuário
/// - carregar sessão ao iniciar o app
/// - controlar splashs
/// - notificar listeners quando algo muda
class SessionManager extends ChangeNotifier {
  static final SessionManager _instance = SessionManager._internal();
  SessionManager._internal();
  factory SessionManager() => _instance;

  static const _tokenKey = 'access_token';
  static const _userKey = 'logged_user';

  final _storage = const FlutterSecureStorage();

  String? _token;
  UsuarioDTO? _usuario;

  /// Versão do app (útil para debug, perfil, etc)
  String versao = '';

  bool _inicializado = false;

  /// Flag para controlar o Splash do Motorista
  bool splashMotoristaFinalizado = false;

  /// Indica se a sessão já foi carregada do storage
  bool get estaInicializado => _inicializado;

  /// Sessão válida quando há token e usuário
  bool get estaLogado => _token != null && _usuario != null;

  String? get token => _token;
  UsuarioDTO? get usuario => _usuario;

  /// Carrega sessão salva localmente (secure storage)
  /// Chamado normalmente no Splash Global
  Future<void> carregarSessao() async {
    if (_inicializado) return;

    _token = await _storage.read(key: _tokenKey);

    final userJson = await _storage.read(key: _userKey);
    if (userJson != null) {
      _usuario = UsuarioDTO.fromJson(
        Map<String, dynamic>.from(jsonDecode(userJson)),
      );
    }

    versao = (await PackageInfo.fromPlatform()).version;

    _inicializado = true;
    notifyListeners();
  }

  /// Salva sessão após login bem-sucedido
  Future<void> salvarSessao({
    required String token,
    required UsuarioDTO usuario,
  }) async {
    _token = token;
    _usuario = usuario;

    await _storage.write(key: _tokenKey, value: token);
    await _storage.write(key: _userKey, value: jsonEncode(usuario.toJson()));

    versao = (await PackageInfo.fromPlatform()).version;

    splashMotoristaFinalizado = false;
    _inicializado = true;

    locator<DrawerNavegacaoController>().carregueDrawerPadrao();

    notifyListeners();
  }

  /// Chamado quando o Splash do Motorista termina
  void finalizarSplashMotorista() {
    splashMotoristaFinalizado = true;
    notifyListeners();
  }

  /// Atualiza apenas o token (uso futuro com refresh token)
  Future<void> atualizarToken(String token) async {
    _token = token;
    await _storage.write(key: _tokenKey, value: token);
    notifyListeners();
  }

  /// Limpa toda a sessão (logout)
  Future<void> limparSessao() async {
    _token = null;
    _usuario = null;
    splashMotoristaFinalizado = false;
    _inicializado = true;

    await _storage.deleteAll();
    notifyListeners();
  }
}

