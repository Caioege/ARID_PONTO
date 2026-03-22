import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:http/http.dart' as http;
import 'package:mask_text_input_formatter/mask_text_input_formatter.dart';
import '../constants.dart' as Constants;

enum TipoLogin { AlunoResponsavel, Professor }

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _senhaController = TextEditingController();

  final _cpfMaskFormatter = MaskTextInputFormatter(
    mask: '###.###.###-##',
    filter: {"#": RegExp(r'[0-9]')},
  );

  bool _loading = false;
  String? _mensagemErro;

  bool _senhaOculta = true;

  @override
  void initState() {
    super.initState();
    _verificarLoginSalvo();
  }

  Future<void> _verificarLoginSalvo() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');
    final timestamp = prefs.getInt('login_timestamp');

    if (token != null && timestamp != null) {
      final dataLogin = DateTime.fromMillisecondsSinceEpoch(timestamp);
      final agora = DateTime.now();
      final diferenca = agora.difference(dataLogin);

      if (diferenca.inDays <= 3) {
        _navegarParaHome();
      }
    }
  }

  void _exibirAjuda() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: const Color.fromARGB(255, 255, 255, 255),
        title: const Text(
          'Precisa de ajuda?',
          style: TextStyle(
            color: Color.fromARGB(255, 0, 53, 77),
            fontWeight: FontWeight.bold,
          ),
        ),
        content: const Text(
          'Entre em contato com o responsável da sua unidade para que seu acesso possa ser verificado.',
          style: TextStyle(color: Colors.black87),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
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

  void _navegarParaHomeEscola() {
    Navigator.pushReplacementNamed(context, '/dashboard');
  }

  void _navegarParaHome() {
    Navigator.pushReplacementNamed(context, '/aluno');
  }

  Future<void> _fazerLogin() async {
    setState(() {
      _loading = true;
      _mensagemErro = null;
    });

    final email = _emailController.text.trim();
    final senha = _senhaController.text.trim();

    if (email.isEmpty || senha.isEmpty) {
      setState(() {
        _mensagemErro = 'Informe o usuário e a senha.';
        _loading = false;
      });
      return;
    }

    final apiUrl = Constants.API_URL;

    try {
      final response = await http.post(
        Uri.parse('$apiUrl/autentique'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'usuario': email, 'senha': senha}),
      );

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);

        final prefs = await SharedPreferences.getInstance();

        await _limparDadosSessao(prefs);

        if (json.containsKey('usuarioEscola') &&
            json['usuarioEscola'] != null) {
          await prefs.setString('user_type', 'escola');
          _salvarDadosEscola(prefs, json['usuarioEscola']);
          _navegarParaHomeEscola();
        } else {
          if (json.containsKey('alunos') && (json['alunos'] ?? []).length > 0) {
            await prefs.setString('user_type', 'responsavel');

            final List<dynamic> alunos = json['alunos'] ?? [];

            if (alunos.length == 1) {
              await _salvarDadosAluno(prefs, alunos.first);
              _navegarParaHome();
            } else if (alunos.length > 1) {
              await prefs.setString('lista_alunos', jsonEncode(alunos));
              _navegarParaSelecaoAluno();
            } else {
              final isLegacyLogin = json.containsKey('alunoId');
              if (isLegacyLogin) {
                await _salvarDadosAluno(prefs, json);
                _navegarParaHome();
              } else {
                setState(() {
                  _mensagemErro = 'Nenhum aluno vinculado a este usuário.';
                });
              }
            }
          } else if (json.containsKey('turmas') &&
              (json['turmas'] ?? []).isNotEmpty) {
            await _processarLoginProfessor(prefs, json);
          } else {
            setState(() {
              _mensagemErro = 'Nenhum perfil de usuário encontrado.';
            });
          }
        }

        await prefs.setInt(
          'login_timestamp',
          DateTime.now().millisecondsSinceEpoch,
        );
      } else if (response.statusCode == 400 || response.statusCode == 401) {
        final msg =
            jsonDecode(response.body)['message'] ??
            'Usuário ou senha inválidos';
        setState(() {
          _mensagemErro = msg;
        });
      } else {
        setState(() {
          _mensagemErro =
              'Erro inesperado: ${response.statusCode} ${response.reasonPhrase}';
        });
      }
    } catch (e) {
      setState(() {
        _mensagemErro =
            'Erro ao conectar com o servidor. Verifique sua conexão.';
      });
    }

    setState(() {
      _loading = false;
    });
  }

  Future<void> _processarLoginProfessor(
    SharedPreferences prefs,
    Map<String, dynamic> loginData,
  ) async {
    final scaffoldMessenger = ScaffoldMessenger.of(context);
    scaffoldMessenger.showSnackBar(
      const SnackBar(content: Text('Preparando dados para acesso offline...')),
    );

    await prefs.setString('user_type', 'professor');
    await prefs.setInt('servidorId', loginData['servidorId']);
    await prefs.setString('nome_professor', loginData['nomePessoa']);
    await prefs.setString('imagemPessoa', loginData['fotoBase64']);
    await prefs.setInt('redeDeEnsinoId', loginData['redeDeEnsinoId']);
    await prefs.setString('redeDeEnsinoNome', loginData['redeDeEnsinoNome']);

    final token = loginData['token'];
    await prefs.setString('token', token ?? '');
    final List<dynamic> turmas = loginData['turmas'];
    await prefs.setString('professor_data_cache', jsonEncode(turmas));
    _navegarParaSelecaoTurma();
  }

  void _navegarParaSelecaoAluno() {
    Navigator.pushReplacementNamed(context, '/selecao-aluno');
  }

  void _navegarParaSelecaoTurma() {
    Navigator.pushReplacementNamed(context, '/selecao-turma');
  }

  Future<void> _salvarDadosEscola(
    SharedPreferences prefs,
    Map<String, dynamic> escolaJson,
  ) async {
    await prefs.setString('nome', escolaJson['pessoaNome']);
    await prefs.setString('escolaNome', escolaJson['escolaNome']);
    await prefs.setString('pessoaNome', escolaJson['pessoaNome']);
    await prefs.setInt('escolaId', escolaJson['escolaId']);
    await prefs.setInt('usuarioId', escolaJson['usuarioId']);
    await prefs.setInt('redeDeEnsinoId', escolaJson['redeDeEnsinoId']);
    await prefs.setString('redeDeEnsinoNome', escolaJson['redeDeEnsinoNome']);
  }

  Future<void> _salvarDadosAluno(
    SharedPreferences prefs,
    Map<String, dynamic> alunoJson,
  ) async {
    await prefs.setString('nome', alunoJson['pessoaNome']);
    await prefs.setInt('escolaId', alunoJson['escolaId']);
    await prefs.setString('escolaNome', alunoJson['escolaNome']);
    await prefs.setInt('alunoId', alunoJson['alunoId']);
    await prefs.setInt('turmaId', alunoJson['turmaId']);
    await prefs.setString('turmaDescricao', alunoJson['turmaDescricao']);
    await prefs.setInt('redeDeEnsinoId', alunoJson['redeDeEnsinoId']);
    await prefs.setString('cpf', alunoJson['cpf']);
    await prefs.setString('redeDeEnsino', alunoJson['redeDeEnsinoNome']);
    await prefs.setString('fotoBase64', alunoJson['fotoBase64']);
  }

  Future<void> _limparDadosSessao(SharedPreferences prefs) async {
    await prefs.remove('login_timestamp');
    await prefs.remove('lista_alunos');
    await prefs.remove('nome');
    await prefs.remove('escolaId');
    await prefs.remove('escolaNome');
    await prefs.remove('alunoId');
    await prefs.remove('turmaId');
    await prefs.remove('turmaDescricao');
    await prefs.remove('redeDeEnsinoId');
    await prefs.remove('cpf');
    await prefs.remove('redeDeEnsino');
    await prefs.remove('fotoBase64');
    await prefs.remove('nome_professor');
    await prefs.remove('turma_selecionada');
    await prefs.remove('redeDeEnsinoNome');
    await prefs.remove('user_type');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color.fromARGB(88, 3, 9, 99),
      body: Stack(
        children: [
          // Fundo com imagem e opacidade
          Opacity(
            opacity: 0.85,
            child: Image.asset(
              'assets/images/fundo-login.jpeg',
              fit: BoxFit.cover,
              width: double.infinity,
              height: double.infinity,
            ),
          ),
          SafeArea(
            child: Padding(
              padding: const EdgeInsets.all(20),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Spacer(),
                  SizedBox(
                    width: 120,
                    height: 120,
                    child: Image.asset('assets/images/logo-sistema.png'),
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    'FREQUÊNCIA ESCOLAR',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 24,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  const SizedBox(height: 15),
                  _inputField('Usuário', _emailController),
                  const SizedBox(height: 16),
                  _inputField('Senha', _senhaController, isPassword: true),
                  const SizedBox(height: 8),
                  if (_mensagemErro != null)
                    Container(
                      margin: const EdgeInsets.only(top: 8),
                      padding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 10,
                      ),
                      decoration: BoxDecoration(
                        color: Colors.black.withOpacity(0.4),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Row(
                        children: [
                          const Icon(
                            Icons.error_outline,
                            color: Colors.redAccent,
                            size: 20,
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              _mensagemErro!,
                              style: const TextStyle(
                                color: Colors.white,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  const SizedBox(height: 16),
                  SizedBox(
                    width: double.infinity,
                    height: 50,
                    child: ElevatedButton(
                      onPressed: _loading ? null : _fazerLogin,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Color.fromARGB(255, 0, 53, 77),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: _loading
                          ? const CircularProgressIndicator(color: Colors.white)
                          : const Text(
                              'ENTRAR',
                              style: TextStyle(color: Colors.white),
                            ),
                    ),
                  ),
                  const Spacer(),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      TextButton(
                        onPressed: _exibirAjuda,
                        child: const Text(
                          'Não consegue acessar?',
                          style: TextStyle(
                            color: Color.fromARGB(255, 0, 53, 77),
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _inputField(
    String label,
    TextEditingController controller, {
    bool isPassword = false,
    List<TextInputFormatter>? inputFormatters,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: const TextStyle(
            color: Colors.white,
            fontWeight: FontWeight.bold,
            fontSize: 14,
          ),
        ),
        const SizedBox(height: 4),
        Container(
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(8),
          ),
          child: TextField(
            controller: controller,
            obscureText: isPassword ? _senhaOculta : false,
            inputFormatters: inputFormatters,
            keyboardType: TextInputType.number,
            decoration: const InputDecoration(
              border: InputBorder.none,
              contentPadding: EdgeInsets.symmetric(
                horizontal: 12,
                vertical: 12,
              ),
            ),
          ),
        ),
      ],
    );
  }
}
