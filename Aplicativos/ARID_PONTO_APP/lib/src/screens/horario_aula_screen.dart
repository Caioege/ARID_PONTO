import 'dart:convert';
import 'package:app_arid_escolas/src/constants.dart' as Constants;
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

/// Tela: Horário de Aula (agrupando a partir de uma lista plana de ItemHorarioDeAulaAppDTO).
/// Backend retorna uma lista de itens no formato:
/// [
///   {
///     "diaDaSemana": 1,
///     "siglaDiaDaSemana": "SEG",
///     "disciplinaId": 10,
///     "disciplinaNome": "Matemática",
///     "inicio": "07:30:00",
///     "fim": "08:20:00"
///   },
///   ...
/// ]
class HorariosAulaScreen extends StatefulWidget {
  const HorariosAulaScreen({super.key, this.turmaId});

  final int? turmaId;

  @override
  State<HorariosAulaScreen> createState() => _HorariosAulaScreenState();
}

class _HorariosAulaScreenState extends State<HorariosAulaScreen> {
  bool carregando = true;
  String? erro;
  // Estrutura agrupada para render:
  // [{ dia: 1, sigla: "SEG", itens: [{disciplinaNome, inicio, fim}, ...] }, ...]
  List<Map<String, dynamic>> dias = [];

  @override
  void initState() {
    super.initState();
    _carregar();
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
      final turmaId = prefs.getInt('turmaId');

      final uri = Uri.parse('$apiUrl/horarios-aula/$turmaId');

      final resp = await http.get(
        uri,
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      if (resp.statusCode >= 200 && resp.statusCode < 300) {
        final list = jsonDecode(resp.body);
        if (list is! List) {
          setState(() {
            erro = 'Formato inesperado do retorno.';
            carregando = false;
          });
          return;
        }

        // Normaliza itens
        final itens = list.map<Map<String, dynamic>>((e) {
          final m = Map<String, dynamic>.from(e as Map);
          return {
            'dia': _toInt(m['diaDaSemana']),
            'sigla': (m['siglaDiaDaSemana'] ?? '').toString(),
            'disciplinaId': _toInt(m['disciplinaId']),
            'disciplinaNome': (m['disciplinaNome'] ?? '').toString(),
            'inicio': _fmtHora(m['inicio']),
            'fim': _fmtHora(m['fim']),
          };
        }).toList();

        // Agrupa por dia 1..5 (SEG..SEX), garante todos os dias
        const ordem = [1, 2, 3, 4, 5];
        const siglasPadrao = {1: 'SEG', 2: 'TER', 3: 'QUA', 4: 'QUI', 5: 'SEX'};

        final porDia = <int, List<Map<String, dynamic>>>{};
        for (final cod in ordem) {
          porDia[cod] = [];
        }

        for (final it in itens) {
          final d = it['dia'] as int?;
          if (d == null || d < 1 || d > 5) continue;
          porDia[d]!.add(it);
        }

        // Ordena por horário de início e monta lista final
        final resultado = <Map<String, dynamic>>[];
        for (final d in ordem) {
          porDia[d]!.sort(
            (a, b) => (a['inicio'] as String).compareTo(b['inicio'] as String),
          );
          final sigla = porDia[d]!.isNotEmpty
              ? (porDia[d]!.first['sigla'] as String? ?? siglasPadrao[d]!)
              : siglasPadrao[d]!;
          resultado.add({'dia': d, 'sigla': sigla, 'itens': porDia[d]});
        }

        setState(() {
          dias = resultado;
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

  static int? _toInt(dynamic v) {
    if (v == null) return null;
    if (v is int) return v;
    return int.tryParse(v.toString());
  }

  /// Aceita "HH:mm:ss", "HH:mm" ou ISO 8601 de TimeSpan (se vier como "PT01H30M"... retornará literal).
  static String _fmtHora(dynamic v) {
    final s = (v ?? '').toString();
    if (s.isEmpty) return s;
    // HH:mm:ss -> HH:mm
    final parts = s.split(':');
    if (parts.length >= 2 && parts[0].length <= 2 && parts[1].length == 2) {
      return '${parts[0].padLeft(2, '0')}:${parts[1]}';
    }
    return s; // fallback
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        backgroundColor: Color.fromARGB(255, 0, 53, 77),
        title: const Text(
          'Horário de Aula',
          style: TextStyle(color: Colors.white),
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
    return RefreshIndicator(
      onRefresh: _carregar,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [...dias.map(_buildDiaCard), const SizedBox(height: 24)],
      ),
    );
  }

  Widget _buildDiaCard(Map<String, dynamic> dia) {
    final String sigla = (dia['sigla'] ?? '').toString();
    final List itens = (dia['itens'] as List? ?? []);

    return Card(
      elevation: 1,
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 10,
                    vertical: 6,
                  ),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(
                    sigla,
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 20,
                    ),
                  ),
                ),
                const Spacer(),
                Text(
                  itens.isEmpty
                      ? 'Sem aulas'
                      : '${itens.length} ${itens.length == 1 ? 'aula' : 'aulas'}',
                  style: TextStyle(color: Colors.grey.shade700),
                ),
              ],
            ),
            const SizedBox(height: 18),
            if (itens.isEmpty)
              Text('—', style: TextStyle(color: Colors.white))
            else
              Column(
                children: [for (final it in itens) _buildIntervaloTile(it)],
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildIntervaloTile(Map<String, dynamic> it) {
    final String inicio = (it['inicio'] ?? '').toString();
    final String fim = (it['fim'] ?? '').toString();
    final String disciplina = (it['disciplinaNome'] ?? '').toString();

    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: Colors.white, // ✅ fundo branco
        border: Border.all(
          color: Colors.grey.shade300,
        ), // dica: branco some no branco
        borderRadius: BorderRadius.circular(12),
        // opcional: sombra leve
        boxShadow: const [
          BoxShadow(
            blurRadius: 6,
            offset: Offset(0, 2),
            color: Color(0x1A000000),
          ),
        ],
      ),
      child: Row(
        children: [
          const Icon(Icons.schedule, size: 18),
          const SizedBox(width: 8),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '$inicio  —  $fim',
                  style: const TextStyle(
                    fontSize: 18,
                    color: Colors.black,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                if (disciplina.isNotEmpty) const SizedBox(height: 2),
                if (disciplina.isNotEmpty)
                  Text(
                    disciplina,
                    style: TextStyle(
                      fontSize: 18,
                      color: Colors.black,
                      fontWeight: FontWeight.w800,
                    ),
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
