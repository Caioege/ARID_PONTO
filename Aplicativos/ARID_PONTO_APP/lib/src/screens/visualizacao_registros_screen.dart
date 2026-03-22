import 'dart:convert';
import 'package:app_arid_escolas/src/constants.dart' as Constants;
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

/// Visualização de Registros de Frequência (mês atual - servidor)
/// Reflete o método:
///   GET /frequencia-aluno-mes/{turmaId}/{alunoId}
/// Retorno esperado (JSON):
/// [
///   {"key":"2025-08-01T00:00:00","value":"PRESENÇA"},
///   {"key":"2025-08-02T00:00:00","value":"FALTA"},
///   {"key":"2025-08-03T00:00:00","value":"<SEM REGISTRO>"},
///   {"key":"2025-08-30T00:00:00","value":"<DATA FUTURA>"}
/// ]
class VisualizacaoRegistrosScreen extends StatefulWidget {
  const VisualizacaoRegistrosScreen({super.key, this.turmaId, this.alunoId});

  final int? turmaId;
  final int? alunoId;

  @override
  State<VisualizacaoRegistrosScreen> createState() =>
      _VisualizacaoRegistrosScreenState();
}

class _VisualizacaoRegistrosScreenState
    extends State<VisualizacaoRegistrosScreen> {
  bool carregando = true;
  String? erro;

  late int _turmaId;
  late int _alunoId;

  // mapa: dia -> status normalizado
  Map<int, String> _registros = {};

  late int _mesAtual;
  late int _anoAtual;

  @override
  void initState() {
    super.initState();
    final hoje = DateTime.now();
    _mesAtual = hoje.month;
    _anoAtual = hoje.year;
    _initAndLoad();
  }

  Future<void> _initAndLoad() async {
    setState(() {
      carregando = true;
      erro = null;
    });

    try {
      final prefs = await SharedPreferences.getInstance();
      _turmaId = prefs.getInt('turmaId') ?? 0;
      _alunoId = prefs.getInt('alunoId') ?? 0;
      await _carregar();
    } catch (e) {
      setState(() {
        erro = 'Falha ao inicializar: $e';
        carregando = false;
      });
    }
  }

  Future<void> _carregar() async {
    setState(() {
      carregando = true;
      erro = null;
    });

    try {
      final prefs = await SharedPreferences.getInstance();
      final token = prefs.getString('token');
      final apiUrl = Constants.API_URL;

      final uri = Uri.parse('$apiUrl/frequencia-aluno-mes/$_turmaId/$_alunoId');

      final resp = await http.get(
        uri,
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      if (resp.statusCode >= 200 && resp.statusCode < 300) {
        final body = jsonDecode(resp.body);
        if (body is! List) {
          setState(() {
            erro = 'Formato inesperado do retorno.';
            carregando = false;
          });
          return;
        }

        final map = <int, String>{};
        for (final e in body) {
          if (e is Map) {
            final k = e['key'];
            final v = (e['value'] ?? '').toString();
            final data = _parseDate(k);
            if (data != null &&
                data.year == _anoAtual &&
                data.month == _mesAtual) {
              map[data.day] = _normalizaStatus(v);
            }
          }
        }

        // Garante todos os dias do mês (que o servidor está usando) presentes
        final totalDias = _ultimoDiaMes(_anoAtual, _mesAtual);
        for (int d = 1; d <= totalDias; d++) {
          map[d] = map[d] ?? 'Sem informação';
        }

        setState(() {
          _registros = map;
          carregando = false;
        });
      } else {
        setState(() {
          erro = 'Erro ${resp.statusCode}: ${resp.body}';
          carregando = false;
        });
      }
    } catch (e) {
      setState(() {
        erro = 'Falha ao carregar: $e';
        carregando = false;
      });
    }
  }

  static DateTime? _parseDate(dynamic v) {
    if (v == null) return null;
    final s = v.toString();
    try {
      return DateTime.parse(s);
    } catch (_) {
      return null;
    }
  }

  static int _ultimoDiaMes(int ano, int mes) {
    return DateTime(ano, mes + 1, 0).day;
  }

  /// Normaliza o valor textual do servidor:
  /// "PRESENÇA" -> "Presente"
  /// "FALTA" -> "Falta"
  /// "<SEM REGISTRO>" -> "Sem registro"
  /// "<DATA FUTURA>" -> "Data futura"
  static String _normalizaStatus(String s) {
    final up = s.trim().toUpperCase();
    if (up == 'PRESENÇA') return 'Presente';
    if (up == 'FALTA') return 'Falta';
    if (up.contains('SEM REGISTRO')) return 'Sem informação';
    if (up.contains('DATA FUTURA')) return 'Data futura';
    return s.isEmpty ? 'Sem informação' : s;
  }

  Color _corStatus(String status) {
    switch (status) {
      case 'Presente':
        return Colors.green.shade600;
      case 'Falta':
        return Colors.red.shade600;
      case 'Data futura':
        return Colors.orange.shade700;
      default:
        return Colors.grey.shade600; // Sem registro
    }
  }

  IconData _iconeStatus(String status) {
    switch (status) {
      case 'Presente':
        return Icons.check_circle;
      case 'Falta':
        return Icons.cancel;
      case 'Data futura':
        return Icons.schedule;
      default:
        return Icons.remove_circle_outline;
    }
  }

  @override
  Widget build(BuildContext context) {
    final nomeMes = _nomeMes(_mesAtual);
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        backgroundColor: Color.fromARGB(255, 0, 53, 77),
        title: Text(
          'Registros - $nomeMes/$_anoAtual',
          style: const TextStyle(color: Colors.white),
        ),
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: carregando
          ? const Center(child: CircularProgressIndicator())
          : erro != null
          ? _buildErro()
          : _buildConteudo(),
    );
  }

  Widget _buildErro() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 48, color: Colors.red),
            const SizedBox(height: 12),
            Text(erro ?? 'Erro', textAlign: TextAlign.center),
            const SizedBox(height: 16),
            ElevatedButton.icon(
              onPressed: _carregar,
              icon: const Icon(Icons.refresh),
              label: const Text('Tentar novamente'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildConteudo() {
    final totalDias = _ultimoDiaMes(_anoAtual, _mesAtual);
    final dias = List.generate(totalDias, (i) => i + 1);

    return RefreshIndicator(
      onRefresh: _carregar,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          _buildLegenda(),
          const SizedBox(height: 12),
          ...dias.map(
            (d) => _buildLinhaDia(d, _registros[d] ?? 'Sem informação'),
          ),
          const SizedBox(height: 24),
        ],
      ),
    );
  }

  Widget _buildLegenda() {
    return Wrap(
      spacing: 12,
      runSpacing: 8,
      children: [
        _chipLegenda('Presente', _corStatus('Presente'), Icons.check_circle),
        _chipLegenda('Falta', _corStatus('Falta'), Icons.cancel),
        _chipLegenda(
          'Sem informação',
          _corStatus('Sem informação'),
          Icons.remove_circle_outline,
        ),
        _chipLegenda('Data futura', _corStatus('Data futura'), Icons.schedule),
      ],
    );
  }

  Widget _chipLegenda(String texto, Color cor, IconData icone) {
    return Chip(
      avatar: Icon(icone, size: 18, color: Colors.white),
      label: Text(texto, style: const TextStyle(color: Colors.white)),
      backgroundColor: cor,
      padding: const EdgeInsets.symmetric(horizontal: 8),
    );
  }

  Widget _buildLinhaDia(int dia, String status) {
    final cor = _corStatus(status);
    final icone = _iconeStatus(status);

    return Card(
      elevation: 0.5,
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: Color.fromARGB(255, 0, 53, 77),
          child: Text(
            dia.toString().padLeft(2, '0'),
            style: const TextStyle(color: Color.fromARGB(255, 0, 53, 77)),
          ),
        ),
        title: Text(
          'Dia $dia',
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        subtitle: Text(status),
        trailing: Icon(icone, color: cor),
      ),
    );
  }

  static String _nomeMes(int m) {
    const nomes = [
      '',
      'Janeiro',
      'Fevereiro',
      'Março',
      'Abril',
      'Maio',
      'Junho',
      'Julho',
      'Agosto',
      'Setembro',
      'Outubro',
      'Novembro',
      'Dezembro',
    ];
    return (m >= 1 && m <= 12) ? nomes[m] : 'Mês';
  }
}
