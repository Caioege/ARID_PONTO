import 'package:arid_rastreio/modules/login/pages/login_page.dart';
import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/menu/controller/motorista_menu_controller.dart';
import 'package:flutter/material.dart';
import 'package:lottie/lottie.dart';
import 'package:arid_rastreio/core/auth/session_manager.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/main.dart';
import 'package:arid_rastreio/shared/layout/drawer/controller/drawer_navegacao_controller.dart';

class LogoutDialog {
  static Future<void> show(BuildContext context) async {
    final theme = Theme.of(context).colorScheme;
    await showDialog(
      builder: (BuildContext context) {
        return AlertDialog(
          backgroundColor: Colors.white,
          titlePadding: const EdgeInsets.symmetric(horizontal: 25),
          actionsPadding: const EdgeInsets.symmetric(horizontal: 8),
          contentPadding: const EdgeInsets.all(12),
          shape: const RoundedRectangleBorder(
            borderRadius: BorderRadius.all(Radius.circular(35)),
          ),
          icon: Center(
            child: SizedBox(
              height: 70,
              width: 70,
              child: Lottie.asset(
                'assets/json/interrogacao.json',
                repeat: false,
              ),
            ),
          ),
          iconPadding: const EdgeInsets.fromLTRB(0, 8, 0, 0),
          title: Container(
            padding: const EdgeInsets.symmetric(vertical: 10),
            decoration: BoxDecoration(border: Border(bottom: BorderSide())),
            child: const Text(
              'Deseja sair?',
              textAlign: TextAlign.center,
              style: TextStyle(
                color: Color.fromARGB(255, 0, 0, 0),
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          actionsAlignment: MainAxisAlignment.center,
          actions: [
            Padding(
              padding: const EdgeInsets.all(10),
              child: ElevatedButton(
                style: ButtonStyle(
                  minimumSize: WidgetStateProperty.all<Size>(
                    const Size(90, 40),
                  ),
                  backgroundColor: WidgetStateProperty.all<Color>(
                    theme.primary,
                  ),
                  shape: WidgetStateProperty.all<RoundedRectangleBorder>(
                    RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(35),
                      side: BorderSide(color: theme.primary),
                    ),
                  ),
                ),
                onPressed: () async {
                  Navigator.pop(context);
                  await _executarLogout();
                  Future.delayed(const Duration(seconds: 1), () async {
                    await Navigator.of(
                      navigatorKey.currentContext!,
                    ).pushAndRemoveUntil(
                      MaterialPageRoute(
                        builder: (BuildContext context) => LoginPage(),
                      ),
                      (Route route) => false,
                    );
                  });
                },
                child: Text(
                  'Sim',
                  style: TextStyle(fontSize: 16, color: Colors.white),
                ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(10),
              child: ElevatedButton(
                style: ButtonStyle(
                  minimumSize: WidgetStateProperty.all<Size>(
                    const Size(90, 40),
                  ),
                  backgroundColor: WidgetStateProperty.all<Color>(Colors.white),
                  surfaceTintColor: WidgetStateProperty.all<Color>(
                    theme.primary,
                  ),
                  shape: WidgetStateProperty.all<RoundedRectangleBorder>(
                    RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(35),
                      side: BorderSide(color: theme.primary),
                    ),
                  ),
                ),
                onPressed: () => Navigator.pop(context),
                child: Text('Não', style: TextStyle(fontSize: 16)),
              ),
            ),
          ],
        );
      },
      context: context,
    );
  }

  static Future<void> _executarLogout() async {
    locator<DrawerNavegacaoController>().limpar();
    await locator<SessionManager>().limparSessao();

    locator<ChecklistController>().limpar();
    locator<MotoristaMenuController>().limpar();
  }
}

