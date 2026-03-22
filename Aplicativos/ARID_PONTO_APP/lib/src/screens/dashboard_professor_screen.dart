import 'dart:convert';
import 'package:app_arid_escolas/src/constants.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';

class DashboardProfessorScreen extends StatefulWidget {
  const DashboardProfessorScreen({super.key});

  @override
  State<DashboardProfessorScreen> createState() =>
      _DashboardProfessorScreenState();
}

class _DashboardProfessorScreenState extends State<DashboardProfessorScreen> {
  String _nomeProfessor = '';
  String _turmaDescricao = '';
  String _disciplinaNome = '';
  String _escolaNome = '';
  int? _turmaId;
  bool _carregando = true;
  List<Aluno> _alunos = [];
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregarDadosDoCache();
  }

  Future<void> _carregarDadosDoCache() async {
    setState(() => _carregando = true);
    final prefs = await SharedPreferences.getInstance();
    try {
      _nomeProfessor = prefs.getString('nome_professor') ?? 'Professor(a)';
      final turmaId = prefs.getInt('turma_selecionada_id');
      final disciplinaId = prefs.getInt('disciplina_selecionada_id');
      final dataCacheJson = prefs.getString('professor_data_cache');

      if (turmaId != null && disciplinaId != null && dataCacheJson != null) {
        final List<dynamic> dataCache = jsonDecode(dataCacheJson);
        final itemData = dataCache.firstWhere(
          (item) =>
              item['turmaId'] == turmaId &&
              item['disciplinaId'] == disciplinaId,
          orElse: () => null,
        );

        if (itemData != null) {
          _turmaId = itemData['turmaId'];
          _escolaNome = itemData['escolaNome'];
          _turmaDescricao = itemData['turmaDescricao'];
          _disciplinaNome = itemData['disciplinaNome'];
          final List alunosJson = itemData['alunos'] ?? [];
          setState(() {
            _alunos = alunosJson.map((json) => Aluno.fromJson(json)).toList();
            _erro = null;
          });
        } else {
          throw Exception(
            "Combinação Turma/Disciplina não encontrada no cache.",
          );
        }
      } else {
        throw Exception(
          "Dados de seleção não encontrados. Tente selecionar a turma novamente.",
        );
      }
    } catch (e) {
      setState(() => _erro = e.toString());
    } finally {
      setState(() => _carregando = false);
    }
  }

  Future<void> _navegarParaRegistroFrequencia() async {
    final prefs = await SharedPreferences.getInstance();
    final turmaId = prefs.getInt('turma_selecionada_id');
    final disciplinaId = prefs.getInt('disciplina_selecionada_id');
    final dataCacheJson = prefs.getString('professor_data_cache') ?? '';

    final List<dynamic> dataCache = jsonDecode(dataCacheJson);
    final itemData = dataCache.firstWhere(
      (item) =>
          item['turmaId'] == turmaId && item['disciplinaId'] == disciplinaId,
      orElse: () => null,
    );

    Navigator.pushNamed(
      context,
      '/professor/registro-frequencia',
      arguments: itemData,
    );
  }

  Future<void> _navegarParaConteudoAplicado() async {
    final prefs = await SharedPreferences.getInstance();
    final turmaId = prefs.getInt('turma_selecionada_id');
    final disciplinaId = prefs.getInt('disciplina_selecionada_id');
    final dataCacheJson = prefs.getString('professor_data_cache');

    if (turmaId != null && disciplinaId != null && dataCacheJson != null) {
      final List<dynamic> dataCache = jsonDecode(dataCacheJson);
      final itemData = dataCache.firstWhere(
        (item) =>
            item['turmaId'] == turmaId && item['disciplinaId'] == disciplinaId,
        orElse: () => null,
      );

      if (itemData != null && mounted) {
        Navigator.pushNamed(
          context,
          '/professor/conteudo-aplicado',
          arguments: itemData,
        );
      }
    }
  }

  void _trocarTurma() {
    Navigator.pushNamedAndRemoveUntil(
      context,
      '/selecao-turma',
      (route) => false,
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
            if (!_carregando && _erro == null) ...[
              _buildActionButtons(),
              const Divider(height: 1, indent: 16, endIndent: 16),
            ],
            Expanded(
              child: _carregando
                  ? const Center(child: CircularProgressIndicator())
                  : _erro != null
                  ? Center(
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Text(_erro!),
                      ),
                    )
                  : _buildListaAlunos(),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildActionButtons() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
      child: Row(
        children: [
          Expanded(
            child: ElevatedButton.icon(
              onPressed: _alunos.isEmpty
                  ? null
                  : _navegarParaRegistroFrequencia,
              icon: const Icon(Icons.check_circle_outline),
              label: const Text('Frequência'),
              style: ElevatedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 18),
                backgroundColor: const Color.fromARGB(255, 0, 53, 77),
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: OutlinedButton.icon(
              onPressed: _navegarParaConteudoAplicado,
              icon: const Icon(Icons.edit_note_outlined),
              label: const Text('Conteúdo'),
              style: OutlinedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 18),
                backgroundColor: const Color.fromARGB(255, 0, 53, 77),
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildListaAlunos() {
    if (_alunos.isEmpty) {
      return const Center(child: Text('Nenhum aluno encontrado nesta turma.'));
    }
    return ListView.builder(
      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
      itemCount: _alunos.length,
      itemBuilder: (context, index) {
        final aluno = _alunos[index];
        final formatadorDeData = DateFormat('dd/MM/yyyy');
        String periodo =
            'Entrada: ${formatadorDeData.format(aluno.entradaNaTurma)}';
        if (aluno.saidaDaTurma != null) {
          periodo +=
              ' | Saída: ${formatadorDeData.format(aluno.saidaDaTurma!)}';
        }
        final bool isAtivo = aluno.saidaDaTurma == null;

        return Card(
          elevation: 1.5,
          margin: const EdgeInsets.only(bottom: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          child: ListTile(
            contentPadding: const EdgeInsets.symmetric(
              vertical: 8,
              horizontal: 16,
            ),
            leading: CircleAvatar(
              backgroundColor: isAtivo
                  ? const Color.fromARGB(255, 0, 53, 77)
                  : Colors.grey.shade300,
              child: Text(
                aluno.pessoaNome.substring(0, 1),
                style: TextStyle(
                  color: isAtivo ? Colors.white : Colors.grey.shade600,
                ),
              ),
            ),
            title: Text(
              aluno.pessoaNome,
              style: TextStyle(
                color: isAtivo ? Colors.black87 : Colors.grey.shade600,
              ),
            ),
            subtitle: Text(
              periodo,
              style: TextStyle(
                color: isAtivo ? Colors.black54 : Colors.grey.shade600,
              ),
            ),
          ),
        );
      },
    );
  }

  Widget _header() {
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
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          const CircleAvatar(
            radius: 30,
            backgroundColor: Colors.white,
            child: Icon(
              Icons.school,
              size: 30,
              color: Color.fromARGB(255, 0, 53, 77),
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  _turmaDescricao,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  _disciplinaNome,
                  style: TextStyle(
                    color: Colors.white.withOpacity(0.8),
                    fontSize: 14,
                  ),
                ),
                Text(
                  _escolaNome,
                  style: TextStyle(
                    color: Colors.white.withOpacity(0.8),
                    fontSize: 14,
                  ),
                ),
              ],
            ),
          ),
          IconButton(
            onPressed: _trocarTurma,
            icon: const Icon(Icons.sync_alt, color: Colors.white, size: 24),
            tooltip: 'Trocar Turma',
          ),
        ],
      ),
    );
  }
}
