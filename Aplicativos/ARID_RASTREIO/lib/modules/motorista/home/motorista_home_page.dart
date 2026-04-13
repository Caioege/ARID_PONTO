import 'package:arid_rastreio/core/auth/session_manager.dart';
import 'package:arid_rastreio/modules/motorista/menu/controller/motorista_menu_controller.dart';
import 'package:arid_rastreio/modules/motorista/menu/page/motorista_menu_page.dart';
import 'package:arid_rastreio/modules/motorista/perfil/pages/motorista_perfil_page.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/shared/layout/app_bar/base_app_bar.dart';
import 'package:arid_rastreio/shared/layout/drawer/controller/drawer_navegacao_controller.dart';
import 'package:arid_rastreio/shared/layout/drawer/page/base_drawer.dart';

// ignore_for_file: unnecessary_underscores

class MotoristaHomePage extends StatefulWidget {
  const MotoristaHomePage({super.key});

  @override
  State<MotoristaHomePage> createState() => _MotoristaHomePageState();
}

class _MotoristaHomePageState extends State<MotoristaHomePage>
    with SingleTickerProviderStateMixin {
  final GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>();

  final _session = locator<SessionManager>();

  late final List<Widget> _widgetOptions = <Widget>[
    const MotoristaMenuPage(),
    MotoristaPerfilPage(usuario: _session.usuario!),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      key: _scaffoldKey,
      appBar: BaseAppBar(
        scaffoldKey: _scaffoldKey,
        onTapNotificacao: () {
          locator<MotoristaMenuController>().retornarTela(0);
          Navigator.pushReplacement(
            context,
            PageRouteBuilder(
              pageBuilder: (_, __, ___) => const MotoristaHomePage(),
              transitionDuration: Duration.zero,
            ),
          );
          locator<DrawerNavegacaoController>().mudarIndex(2);
        },
      ),
      drawer: BaseDrawer(),
      body: Observer(
        builder: (_) {
          return Center(
            child: _widgetOptions.elementAt(
              locator<DrawerNavegacaoController>().selectedIndex,
            ),
          );
        },
      ),
    );
  }
}

