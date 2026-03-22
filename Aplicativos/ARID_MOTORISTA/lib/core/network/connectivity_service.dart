import 'package:connectivity_plus/connectivity_plus.dart';
import '../exception/validacao_server.dart';

class ConnectivityService {
  static Future<void> ensureConnected() async {
    List<ConnectivityResult> result = await Connectivity().checkConnectivity();

    if (result.contains(ConnectivityResult.none)) {
      throw ValidacaoServer.erroConexao();
    }
  }
}
