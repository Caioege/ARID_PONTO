import 'dart:async';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:http/http.dart' as http;
import '../constants.dart' as Constants;

class FeriadosScreen extends StatefulWidget {
  const FeriadosScreen({super.key});

  @override
  State<FeriadosScreen> createState() => _FeriadosScreenState();
}

class _FeriadosScreenState extends State<FeriadosScreen> {
  bool carregando = true;
  List<Map<String, String>> feriados = [];

  @override
  void initState() {
    super.initState();
    _carregarFeriados();
  }

  Future<void> _carregarFeriados() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');
    final organizacaoId = prefs.getInt('organizacaoId');

    final apiUrl = Constants.API_URL;

    final response = await http.get(
      Uri.parse('$apiUrl/eventos/$organizacaoId'),
      headers: {'Authorization': 'Bearer $token'},
    );

    if (response.statusCode == 200) {
      final List dados = json.decode(response.body);

      feriados = dados.map<Map<String, String>>((e) {
        final data = DateTime.parse(e['data']);
        final tipo = e['tipoEvento'] == 0 ? 'Feriado' : 'Facultativo';
        return {
          'data':
              '${data.day.toString().padLeft(2, '0')}/${data.month.toString().padLeft(2, '0')}/${data.year}',
          'descricao': e['descricao'],
          'tipo': tipo,
        };
      }).toList();

      if (feriados.isEmpty) {
        if (mounted) {
          _mostrarAlerta('Nenhum registro encontrado.');
          Navigator.pop(context);
        }
      } else {
        setState(() {
          carregando = false;
        });
      }
    } else {
      if (mounted) {
        _mostrarAlerta('Erro ao carregar eventos.');
        Navigator.pop(context);
      }
    }
  }

  void _mostrarAlerta(String mensagem) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text(
          'Atenção',
          style: TextStyle(
            color: Color.fromARGB(255, 0, 53, 77),
            fontWeight: FontWeight.bold,
          ),
        ),
        content: Text(mensagem),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
            },
            child: const Text(
              'OK',
              style: TextStyle(
                color: Color.fromARGB(255, 0, 53, 77),
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey.shade100,
      body: SafeArea(
        child: Column(
          children: [
            _header(),
            if (carregando)
              const Expanded(child: Center(child: CircularProgressIndicator()))
            else
              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 16,
                    vertical: 8,
                  ),
                  itemCount: feriados.length,
                  itemBuilder: (context, index) {
                    final item = feriados[index];
                    final bool isFeriado = item['tipo'] == 'Feriado';

                    return Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Column(
                          children: [
                            Container(
                              width: 20,
                              height: 20,
                              decoration: BoxDecoration(
                                color: isFeriado
                                    ? const Color.fromARGB(255, 0, 53, 77)
                                    : Colors.orange.shade700,
                                shape: BoxShape.circle,
                              ),
                            ),
                            if (index != feriados.length - 1)
                              Container(
                                width: 2,
                                height: 80,
                                color: Colors.grey.shade400,
                              ),
                          ],
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Container(
                            margin: const EdgeInsets.only(bottom: 16),
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius: BorderRadius.circular(12),
                              boxShadow: const [
                                BoxShadow(
                                  color: Colors.black12,
                                  blurRadius: 4,
                                  offset: Offset(0, 2),
                                ),
                              ],
                            ),
                            child: ListTile(
                              leading: CircleAvatar(
                                radius: 22,
                                backgroundColor: isFeriado
                                    ? const Color.fromARGB(255, 0, 53, 77)
                                    : Colors.orange.shade700,
                                child: Icon(
                                  isFeriado
                                      ? Icons.flag
                                      : Icons.event_available_outlined,
                                  color: Colors.white,
                                ),
                              ),
                              title: Text(
                                item['descricao']!,
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                  fontSize: 15,
                                ),
                              ),
                              subtitle: Text(
                                '${item['data']} • ${item['tipo']}',
                                style: const TextStyle(
                                  fontSize: 13,
                                  color: Colors.grey,
                                ),
                              ),
                            ),
                          ),
                        ),
                      ],
                    );
                  },
                ),
              ),
          ],
        ),
      ),
    );
  }

  Widget _header() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: const BoxDecoration(
        color: Color.fromARGB(255, 0, 53, 77),
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(40),
          bottomRight: Radius.circular(40),
        ),
      ),
      child: Row(
        children: [
          IconButton(
            onPressed: () {
              Navigator.pop(context);
            },
            icon: const Icon(Icons.arrow_back, color: Colors.white),
          ),
          const SizedBox(width: 8),
          const Icon(Icons.calendar_month, color: Colors.white, size: 28),
          const SizedBox(width: 8),
          const Text(
            'Feriados e Facultativos',
            style: TextStyle(
              color: Colors.white,
              fontSize: 20,
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
      ),
    );
  }
}
