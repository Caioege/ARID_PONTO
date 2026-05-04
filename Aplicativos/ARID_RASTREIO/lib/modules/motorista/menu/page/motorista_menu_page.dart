import 'package:arid_rastreio/core/service/rota_tracking_service.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/modules/motorista/rotas/page/motorista_rotas_page.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';
import 'package:flutter_mobx/flutter_mobx.dart';

class MotoristaMenuPage extends StatefulWidget {
  const MotoristaMenuPage({super.key});

  @override
  State<MotoristaMenuPage> createState() => _MotoristaMenuPageState();
}

class _MotoristaMenuPageState extends State<MotoristaMenuPage>
    with SingleTickerProviderStateMixin {
  late final AnimationController _pageController;
  late final Animation<double> _fadeAnimation;

  @override
  void initState() {
    super.initState();

    _pageController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 280),
    );

    _fadeAnimation = CurvedAnimation(
      parent: _pageController,
      curve: Curves.easeOut,
    );

    _pageController.forward();

    WidgetsBinding.instance.addPostFrameCallback((_) async {
      final rotasCtl = locator<MotoristaRotasController>();
      final execucao = await rotasCtl.obterRotaEmAndamento();

      if (execucao != null) {
        await FlutterForegroundTask.saveData(
          key: 'rotaExecucaoId',
          value: execucao.id.toString(),
        );
        await FlutterForegroundTask.saveData(
          key: 'execucaoOffline',
          value: execucao.execucaoOffline.toString(),
        );
        await FlutterForegroundTask.saveData(
          key: 'localExecucaoId',
          value: execucao.localExecucaoId ?? '',
        );

        await RotaTrackingService.start(
          descricaoRota: execucao.descricao,
          execucaoOffline: execucao.execucaoOffline,
        );

        if (mounted) {
          showAppDialog(
            context: context,
            titulo: 'Sessão restaurada',
            mensagem:
                'Você possui uma rota em andamento. Acompanhe e registre os pontos nesta tela.',
            tipo: AppDialogType.alerta,
          );
        }
      }
    });
  }

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    final rotasController = locator<MotoristaRotasController>();

    return Observer(
      builder: (_) {
        return Stack(
          children: [
            FadeTransition(
              opacity: _fadeAnimation,
              child: Scaffold(
                backgroundColor: colors.surface,
                body: const SafeArea(child: MotoristaRotasPage()),
              ),
            ),
            if (rotasController.recuperandoSessao)
              Container(
                color: Colors.black54,
                child: const Center(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      CircularProgressIndicator(color: Colors.white),
                      SizedBox(height: 16),
                      Text(
                        'Restaurando sua sessão...',
                        style: TextStyle(
                          color: Colors.white,
                          fontSize: 18,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
          ],
        );
      },
    );
  }
}
