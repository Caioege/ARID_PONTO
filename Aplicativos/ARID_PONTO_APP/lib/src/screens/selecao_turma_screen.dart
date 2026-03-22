import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:http/http.dart' as http;
import 'package:connectivity_plus/connectivity_plus.dart';
import '../constants.dart' as Constants;

class SelecaoTurmaScreen extends StatefulWidget {
  const SelecaoTurmaScreen({super.key});

  @override
  State<SelecaoTurmaScreen> createState() => _SelecaoTurmaScreenState();
}

class _SelecaoTurmaScreenState extends State<SelecaoTurmaScreen> {
  List<dynamic> _turmasDisciplinas = [];
  String _nomeProfessor = '';
  String? _fotoBase64;
  bool _carregando = true;

  @override
  void initState() {
    super.initState();
    _carregarDadosIniciais();
  }

  Future<void> _carregarDadosIniciais() async {
    await _carregarDadosDoCache();
    _verificarEAtualizarCacheCompleto();
    _verificarDadosPendentesParaSincronizacao();
  }

  Future<void> _carregarDadosDoCache() async {
    final prefs = await SharedPreferences.getInstance();
    final String? dataCacheJson = prefs.getString('professor_data_cache');

    if (dataCacheJson != null) {
      if (mounted) {
        setState(() {
          _turmasDisciplinas = jsonDecode(dataCacheJson);
          _nomeProfessor = prefs.getString('nome_professor') ?? 'Professor(a)';
          _fotoBase64 = prefs.getString('fotoBase64');
          _carregando = false;
        });
      }
    } else {
      Navigator.pushReplacementNamed(context, '/login');
    }
  }

  Future<void> _verificarEAtualizarCacheCompleto() async {
    final connectivityResult = await Connectivity().checkConnectivity();
    if (connectivityResult.contains(ConnectivityResult.none)) {
      return;
    }

    final scaffoldMessenger = ScaffoldMessenger.of(context);
    scaffoldMessenger.showSnackBar(
      const SnackBar(
        content: Text('Buscando atualizações...'),
        duration: Duration(seconds: 2),
      ),
    );

    try {
      await atualizarCacheCompletoProfessor();
      await _carregarDadosDoCache();

      scaffoldMessenger.showSnackBar(
        const SnackBar(
          content: Text('Dados atualizados com sucesso!'),
          backgroundColor: Colors.green,
          duration: Duration(seconds: 2),
        ),
      );
    } catch (e) {
      print("Falha ao tentar atualizar o cache: $e");
    }
  }

  Future<void> atualizarCacheCompletoProfessor() async {
    final apiUrl = Constants.API_URL;
    final List<Map<String, dynamic>> professorDataCache = [];

    try {
      final prefs = await SharedPreferences.getInstance();
      final servidorId = prefs.getInt('servidorId');
      final token = prefs.getString('token');

      final turmasResponse = await http.get(
        Uri.parse('$apiUrl/turmas/$servidorId'),
        headers: {'Authorization': 'Bearer $token'},
      );

      if (turmasResponse.statusCode == 200) {
        await prefs.setString('professor_data_cache', turmasResponse.body);
      }
    } catch (e) {
      print("Não foi possível buscar os dados do servidor: $e");
    }
  }

  Future<void> _selecionarTurmaDisciplina(Map<String, dynamic> item) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setInt('turma_selecionada_id', item['turmaId']);
    await prefs.setInt('disciplina_selecionada_id', item['disciplinaId']);
    Navigator.pushReplacementNamed(context, '/professor/dashboard');
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
        child: _carregando
            ? const Center(child: CircularProgressIndicator())
            : Column(
                children: [
                  _header(context),
                  Expanded(child: _buildListaTurmas()),
                  _footer(context),
                ],
              ),
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
      child: Row(
        children: [
          CircleAvatar(
            radius: 35,
            backgroundColor: Colors.white,
            child: _fotoBase64 != null && _fotoBase64!.isNotEmpty
                ? ClipOval(
                    child: Image.memory(
                      base64Decode(_fotoBase64!),
                      fit: BoxFit.cover,
                      width: 70,
                      height: 70,
                    ),
                  )
                : const Icon(
                    Icons.school,
                    size: 40,
                    color: Color.fromARGB(255, 0, 53, 77),
                  ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Bem-vindo(a),',
                  style: TextStyle(color: Colors.white.withOpacity(0.8)),
                ),
                Text(
                  _nomeProfessor,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                  ),
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildListaTurmas() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Selecione a turma e disciplina',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: Color.fromARGB(255, 0, 53, 77),
            ),
          ),
          const SizedBox(height: 16),
          ListView.builder(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            itemCount: _turmasDisciplinas.length,
            itemBuilder: (context, index) {
              final item = _turmasDisciplinas[index];
              return Card(
                elevation: 2,
                margin: const EdgeInsets.only(bottom: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                child: ListTile(
                  contentPadding: const EdgeInsets.symmetric(
                    vertical: 8,
                    horizontal: 16,
                  ),
                  onTap: () => _selecionarTurmaDisciplina(item),
                  title: Text(
                    '${item['turmaDescricao'] ?? ''}',
                    style: const TextStyle(fontWeight: FontWeight.bold),
                  ),
                  subtitle: Text(
                    'Disciplina: ${item['disciplinaNome'] ?? ''}\nEscola: ${item['escolaNome'] ?? ''}',
                  ),
                  trailing: const Icon(
                    Icons.arrow_forward_ios,
                    color: Colors.grey,
                  ),
                  isThreeLine: true,
                ),
              );
            },
          ),
        ],
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
            onTap: () => _confirmarSair(context),
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

  Future<void> _verificarDadosPendentesParaSincronizacao() async {
    final connectivityResult = await Connectivity().checkConnectivity();
    if (connectivityResult.contains(ConnectivityResult.none)) {
      return;
    }

    final prefs = await SharedPreferences.getInstance();
    final frequenciasJson = prefs.getString('frequencias_pendentes');
    final conteudosJson = prefs.getString('conteudos_pendentes');

    final bool temFrequencias =
        frequenciasJson != null &&
        frequenciasJson.isNotEmpty &&
        frequenciasJson != "[]";
    final bool temConteudos =
        conteudosJson != null &&
        conteudosJson.isNotEmpty &&
        conteudosJson != "[]";

    if (!temFrequencias && !temConteudos) return;

    String mensagem = 'Encontramos dados salvos offline ';
    if (temFrequencias && temConteudos) {
      mensagem += '(frequências e conteúdos).';
    } else if (temFrequencias) {
      mensagem += '(frequências).';
    } else {
      mensagem += '(conteúdos).';
    }
    mensagem += ' Deseja sincronizar agora?';

    if (mounted) {
      showDialog(
        context: context,
        builder: (context) => AlertDialog(
          title: const Text('Sincronização Pendente'),
          content: Text(mensagem),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('Depois'),
            ),
            TextButton(
              onPressed: () {
                Navigator.pop(context);
                _sincronizarTudoAgora();
              },
              child: const Text('Sincronizar'),
            ),
          ],
        ),
      );
    }
  }

  Future<bool> _sincronizarFrequencias() async {
    final prefs = await SharedPreferences.getInstance();
    final registrosJson = prefs.getString('frequencias_pendentes');
    if (registrosJson == null || registrosJson.isEmpty || registrosJson == "[]")
      return true; // Nada a fazer

    try {
      final List<dynamic> registros = jsonDecode(registrosJson);
      final apiUrl = Constants.API_URL;
      final token = prefs.getString('token');

      final response = await http.post(
        Uri.parse('$apiUrl/frequencia/registrar-lote'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(registros),
      );

      if (response.statusCode >= 200 && response.statusCode < 300) {
        await prefs.remove('frequencias_pendentes');
        print("Frequências sincronizadas com sucesso.");
        return true;
      } else {
        print("Erro ao sincronizar frequências: ${response.body}");
        return false;
      }
    } catch (e) {
      print("Exceção ao sincronizar frequências: $e");
      return false;
    }
  }

  Future<bool> _sincronizarConteudos() async {
    final prefs = await SharedPreferences.getInstance();
    final registrosJson = prefs.getString('conteudos_pendentes');
    if (registrosJson == null || registrosJson.isEmpty || registrosJson == "[]")
      return true;

    try {
      final List<dynamic> registros = jsonDecode(registrosJson);
      final apiUrl = Constants.API_URL;
      final token = prefs.getString('token');

      final response = await http.post(
        Uri.parse('$apiUrl/conteudo/registrar-lote'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(registros),
      );

      if (response.statusCode >= 200 && response.statusCode < 300) {
        await prefs.remove('conteudos_pendentes');
        print("Conteúdos sincronizados com sucesso.");
        return true;
      } else {
        print("Erro ao sincronizar conteúdos: ${response.body}");
        return false;
      }
    } catch (e) {
      print("Exceção ao sincronizar conteúdos: $e");
      return false;
    }
  }

  Future<void> _sincronizarTudoAgora() async {
    final scaffoldMessenger = ScaffoldMessenger.of(context);
    scaffoldMessenger.showSnackBar(
      const SnackBar(content: Text('Iniciando sincronização...')),
    );

    try {
      bool sucessoFrequencia = await _sincronizarFrequencias();
      bool sucessoConteudo = await _sincronizarConteudos();

      if (sucessoFrequencia && sucessoConteudo) {
        scaffoldMessenger.showSnackBar(
          const SnackBar(
            content: Text('Todos os dados foram sincronizados!'),
            backgroundColor: Colors.green,
          ),
        );
      } else {
        scaffoldMessenger.showSnackBar(
          const SnackBar(
            content: Text(
              'Alguns dados não puderam ser sincronizados. Tente novamente.',
            ),
            backgroundColor: Colors.orange,
          ),
        );
      }
    } catch (e) {
      scaffoldMessenger.showSnackBar(
        SnackBar(
          content: Text('Ocorreu um erro geral na sincronização: $e'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }
}
