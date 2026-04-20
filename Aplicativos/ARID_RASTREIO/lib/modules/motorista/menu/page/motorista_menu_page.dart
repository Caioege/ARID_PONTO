import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/checklist/page/motorista_checklist_page.dart';
import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/modules/motorista/rotas/page/motorista_rotas_page.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/menu/controller/motorista_menu_controller.dart';

class MotoristaMenuPage extends StatefulWidget {
  const MotoristaMenuPage({super.key});

  @override
  State<MotoristaMenuPage> createState() => _MotoristaMenuPageState();
}

class _MotoristaMenuPageState extends State<MotoristaMenuPage>
    with SingleTickerProviderStateMixin {
  final controller = locator<MotoristaMenuController>();

  late final AnimationController _pageController;
  late final Animation<double> _fadeAnimation;

  final List<Widget> _pages = const [
    MotoristaChecklistPage(),
    MotoristaRotasPage(),
  ];

  @override
  void initState() {
    super.initState();

    _pageController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 400),
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
        controller.mudarIndex(1);

        await FlutterForegroundTask.saveData(
          key: 'rotaExecucaoId',
          value: execucao.id.toString(),
        );

        await RotaTrackingService.start();

        if (mounted) {
          showAppDialog(
            context: context,
            titulo: 'Sessão Restaurada',
            mensagem: 'Você possui uma rota em andamento bloqueada. Não feche o aplicativo.',
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
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return Observer(
      builder: (_) {
        return FadeTransition(
          opacity: _fadeAnimation,
          child: Scaffold(
            backgroundColor: colors.surface,
            body: _pages.elementAt(controller.selectedindex),
            bottomNavigationBar: SafeArea(
              top: false,
              child: Container(
                height: 68,
                padding: const EdgeInsets.symmetric(horizontal: 10),
                decoration: BoxDecoration(
                  color: colors.primary,
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.2),
                      blurRadius: 10,
                      offset: const Offset(0, -4),
                    ),
                  ],
                ),

                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _BottomNavPillItem(
                      icon: Icons.checklist_rounded,
                      label: 'Checklist',
                      selected: controller.selectedindex == 0,
                      onTap: () => controller.mudarIndex(0),
                      color: colors.primary,
                    ),
                    _BottomNavPillItem(
                      icon: Icons.alt_route_rounded,
                      label: 'Rotas',
                      selected: controller.selectedindex == 1,
                      onTap: () {
                        final checklistController =
                            locator<ChecklistController>();

                        if (checklistController.temAlteracoesNaoSalvas) {
                          showAppDialog(
                            context: context,
                            titulo: 'Alterações não salvas',
                            mensagem:
                                'Existem alterações no checklist que ainda não foram salvas. Salve antes de acessar as rotas.',
                            tipo: AppDialogType.alerta,
                          );
                          return;
                        }

                        controller.mudarIndex(1);
                      },

                      color: colors.primary,
                    ),
                  ],
                ),
              ),
            ),
          ),
        );
      },
    );
  }
}

class _BottomNavPillItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final bool selected;
  final VoidCallback onTap;
  final Color color;

  const _BottomNavPillItem({
    required this.icon,
    required this.label,
    required this.selected,
    required this.onTap,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return InkWell(
      borderRadius: BorderRadius.circular(24),
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 250),
        curve: Curves.easeOut,
        padding: selected
            ? const EdgeInsets.symmetric(horizontal: 18, vertical: 10)
            : const EdgeInsets.all(10),
        decoration: BoxDecoration(
          color: selected ? Colors.white : Colors.transparent,
          borderRadius: BorderRadius.circular(24),
        ),
        child: Row(
          children: [
            Icon(icon, size: 22, color: selected ? color : Colors.white),
            if (selected) ...[
              const SizedBox(width: 8),
              Text(
                label,
                style: theme.textTheme.labelMedium?.copyWith(
                  color: color,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

