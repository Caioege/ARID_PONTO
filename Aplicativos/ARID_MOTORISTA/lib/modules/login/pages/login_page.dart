import 'package:arid_motorista/modules/login/controller/login_controller.dart';
import 'package:arid_motorista/modules/login/widgets/login_button.dart';
import 'package:arid_motorista/modules/login/widgets/login_form.dart';
import 'package:arid_motorista/modules/login/widgets/success_animation.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:arid_motorista/ioc/service_locator.dart';
import 'package:arid_motorista/shared/constants.dart';
import 'package:arid_motorista/core/storage/shared_preferences_util.dart';
import 'package:arid_motorista/core/auth/session_manager.dart';

// ignore_for_file: use_build_context_synchronously

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> with TickerProviderStateMixin {
  final controller = LoginController();

  final _usuarioController = TextEditingController();
  final _senhaController = TextEditingController();
  final GlobalKey _buttonKey = GlobalKey();

  final sessionManager = locator<SessionManager>();

  bool logando = false;
  bool relembrarUsuario = false;
  bool logadoComSucesso = false;
  String? mensagemErro;

  Offset _buttonPosition = Offset.zero;
  Size _buttonSize = Size.zero;

  LoginResult? retornoLogin;

  late final AnimationController _shakeController;
  late final Animation<double> _shakeAnimation;

  late final AnimationController _buttonController;
  late final Animation<double> _buttonOpacity;
  late final Animation<Offset> _buttonOffset;

  late final AnimationController _backgroundController;
  late final Animation<double> _gradientAnimation;

  @override
  void initState() {
    super.initState();

    carregarRelembrarUsuario();

    // Animação de erro (shake)
    _shakeController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 500),
    );

    _shakeAnimation = TweenSequence<double>([
      TweenSequenceItem(tween: Tween(begin: 0, end: -10), weight: 1),
      TweenSequenceItem(tween: Tween(begin: -10, end: 10), weight: 2),
      TweenSequenceItem(tween: Tween(begin: 10, end: -10), weight: 2),
      TweenSequenceItem(tween: Tween(begin: -10, end: 0), weight: 1),
    ]).animate(_shakeController);

    // Animação de entrada do botão
    _buttonController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 700),
    );

    _buttonOpacity = Tween(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _buttonController, curve: Curves.easeOut),
    );

    _buttonOffset = Tween(begin: const Offset(0, 0.3), end: Offset.zero)
        .animate(
          CurvedAnimation(parent: _buttonController, curve: Curves.easeOut),
        );

    // Gradiente animado de fundo (sem imagem)
    _backgroundController = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 3),
    )..repeat(reverse: true);

    _gradientAnimation = Tween(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _backgroundController, curve: Curves.easeInOut),
    );

    Future.delayed(const Duration(milliseconds: 600), () {
      if (mounted) _buttonController.forward();
    });
  }

  Future<void> carregarRelembrarUsuario() async {
    final lembrarUsuario = await SharedPreferenceUtil.getBool(
      Constants.lembrarUsuario,
    );
    final usuarioSalvo = await SharedPreferenceUtil.getString(
      Constants.usuarioLogin,
    );

    if (!mounted) return;

    setState(() {
      relembrarUsuario = lembrarUsuario;
      if (lembrarUsuario && usuarioSalvo.isNotEmpty) {
        _usuarioController.text = usuarioSalvo;
      }
    });
  }

  void _captureButtonPosition() {
    final renderBox =
        _buttonKey.currentContext?.findRenderObject() as RenderBox?;
    if (renderBox != null) {
      _buttonPosition = renderBox.localToGlobal(Offset.zero);
      _buttonSize = renderBox.size;
    }
  }

  @override
  void dispose() {
    _shakeController.dispose();
    _buttonController.dispose();
    _backgroundController.dispose();
    _usuarioController.dispose();
    _senhaController.dispose();
    super.dispose();
  }

  /// Executa o login (fake ou real)
  Future<void> logar() async {
    FocusScope.of(context).unfocus();

    if (_usuarioController.text.isEmpty || _senhaController.text.isEmpty) {
      setState(() => mensagemErro = 'Informe usuário e senha');
      _shakeController.forward(from: 0);
      return;
    }

    if (relembrarUsuario) {
      await SharedPreferenceUtil.setString(
        Constants.usuarioLogin,
        _usuarioController.text,
      );
    } else {
      await SharedPreferenceUtil.setString(Constants.usuarioLogin, '');
    }

    setState(() {
      logando = true;
      mensagemErro = null;
    });

    try {
      final retorno = await controller.login(
        login: _usuarioController.text,
        senha: _senhaController.text,
      );

      if (!mounted) return;

      retornoLogin = retorno;
      _captureButtonPosition();

      setState(() {
        logando = false;
        logadoComSucesso = true;
      });
    } catch (e) {
      setState(() {
        mensagemErro = e.toString();
        logando = false;
      });
      _shakeController.forward(from: 0);
    }
  }

  /// Após animação de sucesso, salva sessão e navega
  Future<void> animacaoLoginSucesso() async {
    if (retornoLogin == null) return;

    await sessionManager.salvarSessao(
      token: retornoLogin!.token,
      usuario: retornoLogin!.usuario,
    );

    context.go('/motorista/splash');
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;
    final colors = Theme.of(context).colorScheme;

    return Scaffold(
      body: Stack(
        children: [
          // Fundo em gradiente animado
          AnimatedBuilder(
            animation: _gradientAnimation,
            builder: (context, child) {
              return Container(
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: [
                      Color.lerp(
                        colors.primary,
                        colors.primary.withValues(alpha: 0.85),
                        _gradientAnimation.value * 0.35,
                      )!,
                      Color.lerp(
                        colors.primary.withValues(alpha: 0.9),
                        colors.primary.withValues(alpha: 0.7),
                        _gradientAnimation.value * 0.5,
                      )!,
                    ],
                  ),
                ),
              );
            },
          ),
          SafeArea(
            child: SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: 24),
              child: SizedBox(
                height: size.height - MediaQuery.of(context).padding.top,
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Spacer(flex: 2),
                    FadeTransition(
                      opacity: _buttonOpacity,
                      child: Column(
                        children: [
                          Container(
                            height: 90,
                            width: 90,
                            decoration: BoxDecoration(
                              color: Colors.white,
                              shape: BoxShape.circle,
                              boxShadow: [
                                BoxShadow(
                                  color: Colors.black.withValues(alpha: 0.15),
                                  blurRadius: 20,
                                  offset: const Offset(0, 8),
                                ),
                              ],
                            ),
                            child: Padding(
                              padding: const EdgeInsets.all(15),
                              child: Image.asset(
                                'assets/images/route.png',
                                fit: BoxFit.contain,
                              ),
                            ),
                          ),
                          const SizedBox(height: 12),
                          Text(
                            'AriD Motorista',
                            style: Theme.of(context).textTheme.headlineSmall
                                ?.copyWith(
                                  color: Colors.white,
                                  fontWeight: FontWeight.bold,
                                ),
                          ),
                          Text(
                            'Controle de viagens e checklist',
                            style: Theme.of(context).textTheme.bodyMedium
                                ?.copyWith(
                                  color: Colors.white.withValues(alpha: 0.85),
                                ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 25),
                    AnimatedBuilder(
                      animation: _shakeAnimation,
                      builder: (_, child) {
                        return Transform.translate(
                          offset: Offset(_shakeAnimation.value, 0),
                          child: child,
                        );
                      },
                      child: Container(
                        padding: const EdgeInsets.all(24),
                        decoration: BoxDecoration(
                          color: colors.surface.withValues(alpha: 0.95),
                          borderRadius: BorderRadius.circular(32),
                        ),
                        child: Column(
                          children: [
                            LoginForm(
                              emailController: _usuarioController,
                              passwordController: _senhaController,
                              hasError: mensagemErro != null,
                            ),
                            const SizedBox(height: 12),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Transform.scale(
                                  scale: 0.7,
                                  child: Switch(
                                    value: relembrarUsuario,
                                    onChanged: (value) async {
                                      setState(() => relembrarUsuario = value);
                                      await SharedPreferenceUtil.setBool(
                                        Constants.lembrarUsuario,
                                        value,
                                      );
                                    },
                                  ),
                                ),
                                const SizedBox(width: 8),
                                const Text('Lembrar meu usuário'),
                              ],
                            ),
                            const SizedBox(height: 12),
                            Container(
                              key: _buttonKey,
                              child: LoginButton(
                                loading: logando,
                                onPressed: logar,
                                opacity: _buttonOpacity,
                                offset: _buttonOffset,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                    const Spacer(flex: 3),
                  ],
                ),
              ),
            ),
          ),

          if (logadoComSucesso)
            SuccessAnimation(
              onComplete: animacaoLoginSucesso,
              buttonPosition: _buttonPosition,
              buttonSize: _buttonSize,
            ),
        ],
      ),
    );
  }
}
