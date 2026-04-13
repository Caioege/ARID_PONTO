import 'package:arid_rastreio/core/http/http_client.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/parada_rota_dto.dart';

class AcompanhanteService {
  final _client = locator<AppHttpClient>().dio;

  Future<List<Map<String, dynamic>>> obterMinhasRotas() async {
    final response = await _client.get('/api/rastreio-app/rotas-acompanhante');
    return List<Map<String, dynamic>>.from(response.data['data']);
  }

  Future<Map<String, dynamic>?> obterUltimaLocalizacao(int rotaId) async {
    try {
      final response = await _client.get(
        '/api/rastreio-app/rotas/ultima-localizacao',
        queryParameters: {'rotaId': rotaId},
      );
      return response.data;
    } catch (e) {
      return null;
    }
  }

  Future<List<Map<String, dynamic>>> obterTrajeto(int rotaId, DateTime data) async {
    final response = await _client.get(
      '/api/rastreio-app/rotas/trajeto',
      queryParameters: {
        'rotaId': rotaId,
        'data': data.toIso8601String(),
      },
    );
    return List<Map<String, dynamic>>.from(response.data['data']);
  }
}
