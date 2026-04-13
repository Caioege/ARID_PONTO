import 'package:arid_rastreio/core/auth/session_manager.dart';
import 'package:arid_rastreio/modules/motorista/splash/controller/motorista_splash_controller.dart';
import 'package:arid_rastreio/shared/layout/drawer/controller/drawer_navegacao_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:loading_animation_widget/loading_animation_widget.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/shared/enum/enumerador_page.dart';

class MotoristaSplashPage extends StatefulWidget {
  const MotoristaSplashPage({super.key});

  @override
  State<MotoristaSplashPage> createState() => _MotoristaSplashPageState();
}

class _MotoristaSplashPageState extends State<MotoristaSplashPage>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _scaleAnimation;

  bool _finalizou = false;

  final controller = MotoristaSplashController();
  static const _heroTag = 'arid_rastreio_logo';

  @override
  void initState() {
    super.initState();

    locator<DrawerNavegacaoController>().carregueDrawerPadrao();

    _animationController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1100),
    );

    _scaleAnimation = CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOutBack,
    );

    _animationController.forward();
    controller.initialize();
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;

    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [colors.primary, colors.primary.withValues(alpha: 0.85)],
          ),
        ),
        child: Observer(
          builder: (_) {
            if (controller.erroProcessamento != null) {
              return _erro(context);
            }

            switch (controller.statusSplash) {
              case StatusRequest.inicial:
              case StatusRequest.processando:
                return _loading(context);

              case StatusRequest.finalizado:
                if (!_finalizou) {
                  _finalizou = true;
                  WidgetsBinding.instance.addPostFrameCallback((_) {
                    if (mounted) {
                      locator<SessionManager>().finalizarSplashMotorista();
                    }
                  });
                }
                return _loading(context);
            }
          },
        ),
      ),
    );
  }

  Widget _loading(BuildContext context) {
    final colors = Theme.of(context).colorScheme;

    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Hero(
            tag: _heroTag,
            child: ScaleTransition(
              scale: _scaleAnimation,
              child: Container(
                padding: const EdgeInsets.all(28),
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: Colors.white,
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.25),
                      blurRadius: 30,
                      offset: const Offset(0, 12),
                    ),
                  ],
                ),
                child: Icon(
                  Icons.route_rounded,
                  size: 64,
                  color: colors.primary,
                ),
              ),
            ),
          ),
          const SizedBox(height: 28),
          LoadingAnimationWidget.staggeredDotsWave(
            color: Colors.white,
            size: 42,
          ),
          const SizedBox(height: 16),
          Text(
            'Preparando sua rota...',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
              color: Colors.white.withValues(alpha: 0.9),
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  Widget _erro(BuildContext context) {
    return Center(
      child: Text(
        'Erro ao carregar ambiente',
        style: Theme.of(context).textTheme.bodyMedium,
      ),
    );
  }
}

