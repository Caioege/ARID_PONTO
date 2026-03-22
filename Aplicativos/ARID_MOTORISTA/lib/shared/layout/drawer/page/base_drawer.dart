import 'package:arid_motorista/modules/motorista/home/motorista_home_page.dart';
import 'package:arid_motorista/modules/motorista/menu/controller/motorista_menu_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:arid_motorista/ioc/service_locator.dart';
import 'package:arid_motorista/shared/layout/dialogs/logout_dialog.dart';
import 'package:arid_motorista/shared/layout/drawer/controller/drawer_navegacao_controller.dart';
import 'package:arid_motorista/core/auth/session_manager.dart';

//ignore_for_file: unnecessary_underscores

class BaseDrawer extends StatefulWidget {
  const BaseDrawer({super.key});

  @override
  BaseDrawerState createState() => BaseDrawerState();
}

class BaseDrawerState extends State<BaseDrawer> {
  late final drawerController = locator<DrawerNavegacaoController>();
  late final sessionManager = locator<SessionManager>();
  late final unidadeMenuController = locator<MotoristaMenuController>();
  late IconData icone;
  late String nome;
  late String perfil;

  @override
  void initState() {
    icone = Icons.directions_bus;
    nome = sessionManager.usuario!.nome;
    perfil = 'Motorista';

    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return Drawer(
      backgroundColor: Colors.white,
      child: Column(
        children: [
          Container(
            width: double.infinity,
            padding: const EdgeInsets.fromLTRB(16, 48, 16, 24),
            color: colors.primary,
            child: Column(
              children: [
                Container(
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: Colors.white.withValues(alpha: 0.15),
                  ),
                  child: Icon(icone, size: 48, color: Colors.white),
                ),
                const SizedBox(height: 12),
                Text(
                  nome,
                  style: theme.textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: Colors.white,
                  ),
                ),
                Text(
                  perfil,
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: Colors.white.withValues(alpha: 0.85),
                  ),
                ),
              ],
            ),
          ),
          ...drawerController.menusSuperiorDisponiveis.map(
            (menu) => Observer(
              builder: (_) => _menuItem(
                context,
                icon: menu.icon,
                label: menu.label,
                selected: drawerController.selectedIndex == menu.index,
                onTap: () {
                  drawerController.mudarIndex(menu.index);
                  unidadeMenuController.retornarTela(0);
                  Navigator.pushReplacement(
                    context,
                    PageRouteBuilder(
                      pageBuilder: (_, __, ___) => const MotoristaHomePage(),
                      transitionDuration: Duration.zero,
                    ),
                  );

                  WidgetsBinding.instance.addPostFrameCallback((_) {
                    Navigator.pop(context);
                  });
                },
              ),
            ),
          ),
          const Spacer(),
          const Divider(height: 1),
          Container(
            width: double.infinity,
            color: colors.primary.withValues(alpha: 0.2),
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
            child: Column(
              children: [
                ListTile(
                  contentPadding: EdgeInsets.zero,
                  leading: Icon(Icons.logout, color: colors.primary),
                  title: Text(
                    'Sair',
                    style: theme.textTheme.bodyLarge?.copyWith(
                      color: colors.primary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  onTap: () => LogoutDialog.show(context),
                ),
                Text(
                  'Versão ${sessionManager.versao}',
                  style: theme.textTheme.bodySmall?.copyWith(
                    color: colors.primary.withValues(alpha: 0.85),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _menuItem(
    BuildContext context, {
    required IconData icon,
    required String label,
    required bool selected,
    required VoidCallback onTap,
  }) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return InkWell(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          color: selected ? colors.primary.withValues(alpha: 0.2) : null,
          border: selected
              ? Border(left: BorderSide(color: colors.primary, width: 4))
              : null,
        ),
        child: ListTile(
          leading: Icon(
            icon,
            color: selected ? colors.primary : colors.onSurfaceVariant,
          ),
          title: Text(
            label,
            style: theme.textTheme.bodyLarge?.copyWith(
              color: selected ? colors.primary : colors.onSurface,
              fontWeight: selected ? FontWeight.w600 : FontWeight.normal,
            ),
          ),
        ),
      ),
    );
  }
}
