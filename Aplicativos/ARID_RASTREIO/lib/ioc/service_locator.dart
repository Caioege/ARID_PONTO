import 'package:arid_rastreio/core/service/rota_background_service.dart';
import 'package:arid_rastreio/core/storage/offline_database.dart';
import 'package:arid_rastreio/modules/login/services/login_service.dart';
import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/checklist/service/checklist_service.dart';
import 'package:arid_rastreio/modules/motorista/menu/controller/motorista_menu_controller.dart';
import 'package:arid_rastreio/modules/motorista/offline/repository/offline_rastreio_repository.dart';
import 'package:arid_rastreio/modules/motorista/offline/service/offline_rastreio_service.dart';
import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/modules/motorista/rotas/service/motorista_rotas_service.dart';
import 'package:arid_rastreio/modules/motorista/splash/controller/motorista_splash_controller.dart';
import 'package:get_it/get_it.dart';
import 'package:arid_rastreio/shared/layout/drawer/controller/drawer_navegacao_controller.dart';
import '../core/auth/session_manager.dart';
import '../core/http/http_client.dart';

final locator = GetIt.instance;

void setupLocator() {
  // INFRA
  if (!locator.isRegistered<SessionManager>()) {
    locator.registerLazySingleton<SessionManager>(() => SessionManager());
  }

  if (!locator.isRegistered<AppHttpClient>()) {
    locator.registerLazySingleton<AppHttpClient>(() => AppHttpClient());
  }

  if (!locator.isRegistered<OfflineDatabase>()) {
    locator.registerLazySingleton<OfflineDatabase>(() => OfflineDatabase());
  }

  if (!locator.isRegistered<OfflineRastreioRepository>()) {
    locator.registerLazySingleton<OfflineRastreioRepository>(
      () => OfflineRastreioRepository(locator<OfflineDatabase>()),
    );
  }

  // SERVICES
  if (!locator.isRegistered<LoginService>()) {
    locator.registerLazySingleton<LoginService>(() => LoginService());
  }

  if (!locator.isRegistered<ChecklistService>()) {
    locator.registerLazySingleton<ChecklistService>(() => ChecklistService());
  }

  if (!locator.isRegistered<MotoristaRotasService>()) {
    locator.registerLazySingleton<MotoristaRotasService>(
      () => MotoristaRotasService(),
    );
  }

  if (!locator.isRegistered<RotaBackgroundService>()) {
    locator.registerLazySingleton<RotaBackgroundService>(
      () => RotaBackgroundService(
        locator<MotoristaRotasService>(),
        locator<OfflineRastreioService>(),
      ),
    );
  }

  if (!locator.isRegistered<OfflineRastreioService>()) {
    locator.registerLazySingleton<OfflineRastreioService>(
      () => OfflineRastreioService(locator<OfflineRastreioRepository>()),
    );
  }

  // CONTROLLERS
  if (!locator.isRegistered<ChecklistController>()) {
    locator.registerLazySingleton<ChecklistController>(
      () => ChecklistController(),
    );
  }

  if (!locator.isRegistered<MotoristaMenuController>()) {
    locator.registerLazySingleton<MotoristaMenuController>(
      () => MotoristaMenuController(),
    );
  }

  if (!locator.isRegistered<MotoristaRotasController>()) {
    locator.registerLazySingleton<MotoristaRotasController>(
      () => MotoristaRotasController(),
    );
  }

  if (!locator.isRegistered<MotoristaSplashController>()) {
    locator.registerLazySingleton<MotoristaSplashController>(
      () => MotoristaSplashController(),
    );
  }

  if (!locator.isRegistered<DrawerNavegacaoController>()) {
    locator.registerLazySingleton<DrawerNavegacaoController>(
      () => DrawerNavegacaoController(),
    );
  }
}
