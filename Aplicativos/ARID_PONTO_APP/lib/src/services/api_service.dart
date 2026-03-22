import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';
import '../constants.dart' as Constants;

class ApiService {
  static final String _baseUrl = Constants.API_URL;

  static Future<List<dynamic>> getRegistros() async {
    final response = await http.get(Uri.parse("$_baseUrl/registros"));
    if (response.statusCode == 200) {
      return json.decode(response.body);
    } else {
      throw Exception('Erro ao buscar registros');
    }
  }
}
