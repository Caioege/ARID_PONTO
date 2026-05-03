import 'package:flutter/material.dart';
import 'package:mobx/mobx.dart';
import 'package:arid_rastreio/shared/enum/enumerador_page.dart';
import 'package:arid_rastreio/shared/layout/drawer/models/drawer_menu_item.dart';

part 'drawer_navegacao_controller.g.dart';

class DrawerNavegacaoController = DrawerNavegacaoControllerBase
    with _$DrawerNavegacaoController;

abstract class DrawerNavegacaoControllerBase with Store {
  @observable
  int selectedIndex = DrawerEnum.home.index;

  @action
  void mudarIndex(int novoIndex) {
    selectedIndex = novoIndex;
  }

  @observable
  List<DrawerMenuItem> menusSuperiorDisponiveis = <DrawerMenuItem>[];

  void carregueDrawerPadrao() {
    menusSuperiorDisponiveis = <DrawerMenuItem>[
      DrawerMenuItem(0, 'Início', Icons.home),
      DrawerMenuItem(1, 'Perfil', Icons.person),
      DrawerMenuItem(2, 'Offline', Icons.cloud_off),
    ];
  }

  @action
  void limpar() {
    selectedIndex = DrawerEnum.home.index;
    menusSuperiorDisponiveis = <DrawerMenuItem>[];
  }
}
