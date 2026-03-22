import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class SelecaoAlunoScreen extends StatefulWidget {
  const SelecaoAlunoScreen({super.key});

  @override
  State<SelecaoAlunoScreen> createState() => _SelecaoAlunoScreenState();
}

class _SelecaoAlunoScreenState extends State<SelecaoAlunoScreen> {
  List<dynamic> _alunos = [];
  bool _carregando = true;

  @override
  void initState() {
    super.initState();
    _carregarAlunos();
  }

  Future<void> _carregarAlunos() async {
    final prefs = await SharedPreferences.getInstance();
    final String? alunosJsonString = prefs.getString('lista_alunos');

    if (alunosJsonString != null) {
      setState(() {
        _alunos = jsonDecode(alunosJsonString);
        _carregando = false;
      });
    } else {
      // Se por algum motivo não encontrar a lista, volta para o login
      Navigator.pushReplacementNamed(context, '/login');
    }
  }

  Future<void> _selecionarAluno(Map<String, dynamic> aluno) async {
    final prefs = await SharedPreferences.getInstance();

    // Salva os dados do aluno selecionado no SharedPreferences
    await prefs.setString('nome', aluno['pessoaNome']);
    await prefs.setInt('escolaId', aluno['escolaId']);
    await prefs.setString('escolaNome', aluno['escolaNome']);
    await prefs.setInt('alunoId', aluno['alunoId']);
    await prefs.setInt('turmaId', aluno['turmaId']);
    await prefs.setString('turmaDescricao', aluno['turmaDescricao']);
    await prefs.setInt('redeDeEnsinoId', aluno['redeDeEnsinoId']);
    await prefs.setString('cpf', aluno['cpf']);
    await prefs.setString('redeDeEnsino', aluno['redeDeEnsinoNome']);
    await prefs.setString('fotoBase64', aluno['fotoBase64']);

    // Navega para a home
    Navigator.pushReplacementNamed(context, '/');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color.fromARGB(255, 0, 53, 77),
      body: SafeArea(
        child: _carregando
            ? const Center(
                child: CircularProgressIndicator(color: Colors.white),
              )
            : Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Image.asset('assets/images/logo-sistema.png', height: 80),
                    const SizedBox(height: 24),
                    const Text(
                      'SELECIONE O ALUNO',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 22,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 8),
                    const Text(
                      'Escolha qual aluno você deseja visualizar as informações.',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: Colors.white70, fontSize: 16),
                    ),
                    const SizedBox(height: 32),
                    Expanded(
                      child: ListView.builder(
                        itemCount: _alunos.length,
                        itemBuilder: (context, index) {
                          final aluno = _alunos[index];
                          final fotoBase64 = aluno['fotoBase64'];
                          return Card(
                            margin: const EdgeInsets.only(bottom: 16),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: ListTile(
                              onTap: () => _selecionarAluno(aluno),
                              leading: CircleAvatar(
                                radius: 25,
                                backgroundColor: Colors.grey.shade200,
                                backgroundImage: fotoBase64 != null
                                    ? MemoryImage(base64Decode(fotoBase64))
                                    : const AssetImage(
                                            'assets/images/avatar.png',
                                          )
                                          as ImageProvider,
                              ),
                              title: Text(
                                aluno['pessoaNome'] ?? 'Nome não informado',
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              subtitle: Text(
                                '${aluno['escolaNome'] ?? ''}\nTurma: ${aluno['turmaDescricao'] ?? ''}',
                              ),
                              trailing: const Icon(Icons.arrow_forward_ios),
                              isThreeLine: true,
                            ),
                          );
                        },
                      ),
                    ),
                  ],
                ),
              ),
      ),
    );
  }
}
