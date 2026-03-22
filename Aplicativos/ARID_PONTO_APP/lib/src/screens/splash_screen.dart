import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _verificarSessao();
  }

  Future<void> _verificarSessao() async {
    await Future.delayed(const Duration(milliseconds: 50));

    final prefs = await SharedPreferences.getInstance();
    final timestamp = prefs.getInt('login_timestamp');
    final perfil = prefs.getString('user_type');

    if (!mounted) return;

    if (timestamp != null && perfil != null && perfil.isNotEmpty) {
      String? rotaDestino;

      switch (perfil) {
        case 'professor':
          rotaDestino = '/selecao-turma';
          break;
        case 'escola':
          rotaDestino = '/dashboard';
          break;
        case 'responsavel':
          final listaAlunos = prefs.getString('lista_alunos') ?? '';
          final alunoSelecionado = prefs.getInt('alunoId') != null;
          if (listaAlunos.isNotEmpty &&
              listaAlunos != '[]' &&
              !alunoSelecionado) {
            rotaDestino = '/selecao-aluno';
          } else {
            rotaDestino = '/aluno';
          }
          break;
        default:
          if (prefs.getInt('alunoId') != null) {
            rotaDestino = '/aluno';
          }
      }

      if (rotaDestino != null) {
        Navigator.pushReplacementNamed(context, rotaDestino);
        return;
      }
    }

    Navigator.pushReplacementNamed(context, '/login');
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(body: Center(child: CircularProgressIndicator()));
  }
}
