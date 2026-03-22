import 'package:app_arid_escolas/src/screens/conteudo_aplicado_screen.dart';
import 'package:app_arid_escolas/src/screens/dashboard_professor_screen.dart';
import 'package:app_arid_escolas/src/screens/dashboard_screen.dart';
import 'package:app_arid_escolas/src/screens/login_screen.dart';
import 'package:app_arid_escolas/src/screens/registro_frequencia_screen.dart';
import 'package:app_arid_escolas/src/screens/selecao_aluno_screen.dart';
import 'package:app_arid_escolas/src/screens/selecao_turma_screen.dart';
import 'package:app_arid_escolas/src/screens/splash_screen.dart';
import 'package:flutter/material.dart';
import 'screens/home_screen.dart';
import 'screens/horario_aula_screen.dart';
import 'screens/visualizacao_registros_screen.dart';

final Map<String, WidgetBuilder> routes = {
  '/': (_) => const SplashScreen(),
  '/aluno': (_) => const HomeScreen(),
  '/registros': (_) => const VisualizacaoRegistrosScreen(),
  '/horarios': (_) => const HorariosAulaScreen(),
  '/login': (_) => const LoginScreen(),
  '/selecao-aluno': (_) => const SelecaoAlunoScreen(),
  '/selecao-turma': (_) => const SelecaoTurmaScreen(),
  '/dashboard': (_) => const DashboardEscolaScreen(),
  '/professor/dashboard': (context) => const DashboardProfessorScreen(),
  '/professor/registro-frequencia': (context) =>
      const RegistroFrequenciaScreen(),
  '/professor/conteudo-aplicado': (context) => ConteudoAplicadoScreen(),
};
