import 'package:arid_rastreio/shared/enum/enumerador_page.dart';
import 'package:mobx/mobx.dart';

part 'motorista_menu_controller.g.dart';

class MotoristaMenuController = MotoristaMenuBaseController
    with _$MotoristaMenuController;

abstract class MotoristaMenuBaseController with Store {
  @observable
  String? erroProcessamento;

  @observable
  int selectedindex = MenuEnum.home.index;

  @action
  void mudarIndex(int index) {
    var valor = MenuEnum.home.index;
    if (index >= valor) {
      valor = index;
    }
    selectedindex = valor;
  }

  @action
  void retornarTela(int index) {
    selectedindex = MenuEnum.home.index;
  }

  @action
  void limpar() {
    selectedindex = MenuEnum.home.index;
  }
}

