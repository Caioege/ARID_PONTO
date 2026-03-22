import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:app_arid_escolas/src/constants.dart' as Constants;
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  String? _nome;
  String? _cpf;
  String? _escolaNome;
  String? _turmaDescricao;
  String? _fotoBase64;
  String? _redeDeEnsino;
  bool _perfilResponsavel = false;

  bool _carregandoRegistros = false;
  List<Map<String, String>> _registros = [];

  @override
  void initState() {
    super.initState();

    _carregarDados();
  }

  void _trocarDeAluno() {
    Navigator.pushNamedAndRemoveUntil(
      context,
      '/selecao-aluno',
      (route) => false,
    );
  }

  Future<void> _carregarDados() async {
    final prefs = await SharedPreferences.getInstance();
    setState(() {
      _nome = prefs.getString('nome') ?? '---';
      _cpf = prefs.getString('cpf') ?? '---';
      _turmaDescricao = prefs.getString('turmaDescricao') ?? '---';
      _escolaNome = prefs.getString('escolaNome') ?? '---';
      _fotoBase64 = prefs.getString('fotoBase64');
      _redeDeEnsino = prefs.getString('redeDeEnsino') ?? '---';
      _perfilResponsavel = (prefs.getString('lista_alunos') ?? '').length > 0;
    });

    await _carregarFrequenciaDaSemana();
  }

  void _confirmarSair(BuildContext context) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text(
          'Deseja sair?',
          style: TextStyle(
            color: Color.fromARGB(255, 0, 53, 77),
            fontWeight: FontWeight.bold,
          ),
        ),
        content: const Text('Você realmente deseja sair do aplicativo?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text(
              'Cancelar',
              style: TextStyle(color: Colors.grey, fontWeight: FontWeight.bold),
            ),
          ),
          TextButton(
            onPressed: () async {
              final prefs = await SharedPreferences.getInstance();
              await prefs.clear();

              if (context.mounted) {
                Navigator.pushNamedAndRemoveUntil(
                  context,
                  '/login',
                  (route) => false,
                );
              }
            },
            child: const Text(
              'Sair',
              style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold),
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
            _header(context),
            Expanded(
              child: SingleChildScrollView(
                child: Column(
                  children: [_ultimosRegistros(), _menuOpcoes(context)],
                ),
              ),
            ),
            _footer(context),
          ],
        ),
      ),
    );
  }

  Widget _footer(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Image.asset('assets/images/logo-sistema.png', width: 60),
          InkWell(
            onTap: () {
              _confirmarSair(context);
            },
            borderRadius: BorderRadius.circular(50),
            child: Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: const Color.fromARGB(255, 0, 53, 77),
                borderRadius: BorderRadius.circular(50),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.2),
                    blurRadius: 4,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: const Icon(Icons.logout, color: Colors.white, size: 18),
            ),
          ),
        ],
      ),
    );
  }

  Widget _header(BuildContext context) {
    return Container(
      width: double.infinity,
      decoration: const BoxDecoration(
        color: Color.fromARGB(255, 0, 53, 77),
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(40),
          bottomRight: Radius.circular(40),
        ),
      ),
      padding: const EdgeInsets.fromLTRB(24, 24, 16, 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Text(
            _redeDeEnsino ?? '',
            style: const TextStyle(
              color: Color.fromARGB(150, 255, 255, 255),
              fontSize: 12,
              fontWeight: FontWeight.w600,
              letterSpacing: 0.5,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Container(
                width: 90,
                height: 110,
                padding: const EdgeInsets.all(4),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: _fotoBase64 != null
                      ? Image.memory(
                          base64Decode(_fotoBase64!),
                          fit: BoxFit.cover,
                        )
                      : Image.asset(
                          'assets/images/avatar.png',
                          fit: BoxFit.cover,
                        ),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      '\n${_nome ?? ''}',
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    Text(
                      'CPF: ${_cpf ?? ''}',
                      style: const TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                      ),
                    ),
                    Text(
                      '\n${_escolaNome ?? ''}',
                      style: const TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                      ),
                    ),
                    Text(
                      'Turma: ${_turmaDescricao ?? ''}',
                      style: const TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                      ),
                    ),
                    if (_perfilResponsavel)
                      TextButton.icon(
                        onPressed: _trocarDeAluno,
                        icon: const Icon(Icons.sync_alt, size: 20),
                        label: const Text('Trocar Aluno'),
                        style: TextButton.styleFrom(
                          foregroundColor: Colors.white,
                          backgroundColor: Colors.white.withOpacity(0.2),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(10.0),
                          ),
                          padding: const EdgeInsets.symmetric(
                            horizontal: 16,
                            vertical: 8,
                          ),
                        ),
                      ),
                  ],
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _ultimosRegistros() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          const Text(
            'Últimos registros de frequência',
            textAlign: TextAlign.center,
            style: TextStyle(
              color: Color.fromARGB(255, 0, 53, 77),
              fontSize: 12,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 8),
          if (_carregandoRegistros)
            const CircularProgressIndicator()
          else if (_registros.isEmpty)
            Container(
              padding: const EdgeInsets.all(16),
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
              child: const Text(
                'Nenhum registro encontrado.',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey),
              ),
            )
          else
            Container(
              width: double.infinity,
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
              child: Table(
                columnWidths: const {
                  0: FlexColumnWidth(2),
                  1: FlexColumnWidth(2),
                },
                border: TableBorder.symmetric(
                  inside: const BorderSide(width: 0.5, color: Colors.black12),
                  outside: const BorderSide(width: 0.5, color: Colors.black26),
                ),
                children: _registros.map((registro) {
                  final dataHora = DateTime.parse(registro['data']!);
                  final data =
                      '${dataHora.day.toString().padLeft(2, '0')}/${dataHora.month.toString().padLeft(2, '0')}/${dataHora.year}';
                  final tipo = registro['tipo']!;
                  return _linhaRegistro(data, tipo);
                }).toList(),
              ),
            ),
        ],
      ),
    );
  }

  Future<void> _carregarFrequenciaDaSemana() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final turmaId = prefs.getInt('turmaId');
      final alunoId = prefs.getInt('alunoId');
      final token = prefs.getString('token');

      if (alunoId == null) {
        _exibirAlerta('Erro', 'Aluno não identificado.', true);
        return;
      }

      final apiUrl = Constants.API_URL;
      final resposta = await http.get(
        Uri.parse('$apiUrl/frequencia-aluno/$turmaId/$alunoId'),
        headers: {'Authorization': 'Bearer $token'},
      );

      if (resposta.statusCode == 200) {
        final List<dynamic> lista = json.decode(resposta.body);
        setState(() {
          _registros = lista
              .map<Map<String, String>>(
                (item) => {'data': item['key'], 'tipo': item['value']},
              )
              .toList();
        });
      } else {
        _exibirAlerta('Erro', 'Não foi possível carregar os registros.', true);
      }
    } catch (e) {
      _exibirAlerta('Erro', 'Erro ao carregar registros.', true);
    } finally {
      setState(() {
        _carregandoRegistros = false;
      });
    }
  }

  void _exibirAlerta(String titulo, String mensagem, bool sair) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: Text(
          titulo,
          style: const TextStyle(fontWeight: FontWeight.bold),
        ),
        content: Text(mensagem),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              if (sair) Navigator.pop(context);
            },
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  TableRow _linhaRegistro(String data, String tipo) {
    return TableRow(
      children: [
        Padding(
          padding: const EdgeInsets.all(8),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text(data, style: const TextStyle(fontWeight: FontWeight.bold)),
            ],
          ),
        ),
        Padding(
          padding: const EdgeInsets.all(8),
          child: Center(
            child: Text(
              tipo,
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
        ),
      ],
    );
  }

  Widget _menuOpcoes(BuildContext context) {
    final opcoes = [
      {
        'icon': Icons.access_time,
        'label': 'Horário\nAula',
        'route': '/horarios',
      },
      {
        'icon': Icons.calendar_month,
        'label': 'Registros\nMês',
        'route': '/registros',
      },
    ];

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: GridView.builder(
        shrinkWrap: true,
        physics: const NeverScrollableScrollPhysics(),
        itemCount: opcoes.length,
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2,
          crossAxisSpacing: 12,
          mainAxisSpacing: 12,
          childAspectRatio: 1.4,
        ),
        itemBuilder: (context, index) {
          final opcao = opcoes[index];
          return GestureDetector(
            onTap: () {
              Navigator.pushNamed(context, opcao['route'] as String);
            },
            child: Container(
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(12),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.3),
                    blurRadius: 6,
                    spreadRadius: 1,
                    offset: const Offset(2, 3),
                  ),
                ],
              ),
              padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 6),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    opcao['icon'] as IconData,
                    size: 30,
                    color: const Color.fromARGB(255, 0, 53, 77),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    opcao['label'] as String,
                    textAlign: TextAlign.center,
                    style: const TextStyle(
                      color: Color.fromARGB(255, 0, 53, 77),
                      fontWeight: FontWeight.w600,
                      fontSize: 14,
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
