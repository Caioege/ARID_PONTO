import 'package:mobx/mobx.dart';
import 'package:arid_motorista/shared/enum/enumerador_page.dart';

part 'motorista_splash_controller.g.dart';

class MotoristaSplashController = MotoristaSplashControllerBase
    with _$MotoristaSplashController;

abstract class MotoristaSplashControllerBase with Store {
  @observable
  ObservableFuture<void>? splashObserver;

  @observable
  String? erroProcessamento;

  @computed
  StatusRequest get statusSplash {
    if (splashObserver == null) {
      return StatusRequest.inicial;
    }

    if (splashObserver!.status == FutureStatus.pending) {
      return StatusRequest.processando;
    }

    return StatusRequest.finalizado;
  }

  @action
  Future<void> initialize() async {
    erroProcessamento = null;

    try {
      splashObserver = ObservableFuture(_carregarTudo());
      await splashObserver;
    } catch (e) {
      erroProcessamento = 'Erro ao carregar ambiente da unidade';
    }
  }

  Future<void> _carregarTudo() async {
    await Future.delayed(const Duration(seconds: 2));
  }
}
