import 'package:arid_rastreio/shared/layout/dialogs/logout_dialog.dart';
import 'package:flutter/material.dart';

class BaseAppBar extends StatelessWidget implements PreferredSizeWidget {
  final GlobalKey<ScaffoldState> scaffoldKey;
  final bool mostrarVoltar;
  final VoidCallback? onVoltar;
  final Widget? title;
  final Function? onTapNotificacao;

  const BaseAppBar({
    super.key,
    required this.scaffoldKey,
    this.mostrarVoltar = false,
    this.onVoltar,
    this.title,
    this.onTapNotificacao,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return AppBar(
      backgroundColor: colors.primary,
      foregroundColor: colors.onPrimary,
      centerTitle: true,
      leading: mostrarVoltar
          ? IconButton(
              icon: const Icon(Icons.arrow_back),
              onPressed: onVoltar ?? () => Navigator.pop(context),
            )
          : IconButton(
              icon: const Icon(Icons.menu),
              onPressed: () => scaffoldKey.currentState?.openDrawer(),
            ),
      title:
          title ??
          Image.asset(
            'assets/images/smart-car.png',
            height: 26,
            color: colors.onPrimary,
          ),
      actions: [
        Padding(
          padding: const EdgeInsets.only(right: 12),
          child: IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () {
              LogoutDialog.show(context);
            },
          ),
        ),
      ],
    );
  }

  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight);
}

