import 'dart:async';

import 'package:arid_rastreio/core/network/connectivity_service.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/offline/service/offline_rastreio_service.dart';
import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';

class OfflineStatusBanner extends StatefulWidget {
  const OfflineStatusBanner({super.key});

  @override
  State<OfflineStatusBanner> createState() => _OfflineStatusBannerState();
}

class _OfflineStatusBannerState extends State<OfflineStatusBanner> {
  final _rotasController = locator<MotoristaRotasController>();
  final _offlineService = locator<OfflineRastreioService>();

  StreamSubscription<List<ConnectivityResult>>? _subscription;
  Timer? _timer;
  bool _offline = false;
  bool _avisoExibidoNestaQueda = false;
  bool _conexaoRestabelecida = false;
  bool _sincronizando = false;
  int _pendencias = 0;

  @override
  void initState() {
    super.initState();
    _inicializarConectividade();
  }

  Future<void> _inicializarConectividade() async {
    final atual = await Connectivity().checkConnectivity();
    if (!mounted) return;
    await _atualizarConectividade(atual, exibirAviso: false);

    _subscription = Connectivity().onConnectivityChanged.listen(
      (result) => _atualizarConectividade(result),
    );
    _timer = Timer.periodic(const Duration(seconds: 5), (_) async {
      final atual = await Connectivity().checkConnectivity();
      await _atualizarConectividade(atual, exibirAviso: false);
    });
  }

  Future<void> _atualizarConectividade(
    List<ConnectivityResult> result, {
    bool exibirAviso = true,
  }) async {
    final estaOffline =
        result.contains(ConnectivityResult.none) ||
        !await ConnectivityService.isConnected();
    final rotaAtiva = _rotasController.rotaIniciada;

    if (estaOffline) {
      setState(() {
        _offline = true;
        _conexaoRestabelecida = false;
      });

      if (exibirAviso && rotaAtiva && !_avisoExibidoNestaQueda && mounted) {
        _avisoExibidoNestaQueda = true;
        await showAppDialog(
          context: context,
          titulo: 'Você está offline',
          mensagem:
              'As informações da rota serão salvas neste aparelho. Se a conexão voltar durante o processo, a sincronização será feita automaticamente. Caso contrário, abra o aplicativo depois para enviar os dados pendentes.',
          tipo: AppDialogType.alerta,
        );
      }
      return;
    }

    final estavaOffline = _offline;
    var pendencias = await _offlineService.contarPendencias();
    if (pendencias > 0 && !_sincronizando) {
      _sincronizando = true;
      try {
        await _offlineService.sincronizarPendencias();
        pendencias = await _offlineService.contarPendencias();
      } catch (_) {
        pendencias = await _offlineService.contarPendencias();
      } finally {
        _sincronizando = false;
      }
    }

    if (!mounted) return;
    setState(() {
      _offline = false;
      _avisoExibidoNestaQueda = false;
      _pendencias = pendencias;
      _conexaoRestabelecida = estavaOffline && pendencias > 0;
    });

    if (_conexaoRestabelecida) {
      Future.delayed(const Duration(seconds: 5), () {
        if (mounted) {
          setState(() => _conexaoRestabelecida = false);
        }
      });
    }
  }

  @override
  void dispose() {
    _subscription?.cancel();
    _timer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Observer(
      builder: (_) {
        final rotaAtiva = _rotasController.rotaIniciada;

        if (_offline) {
          return _Banner(
            icon: Icons.cloud_off_rounded,
            label: 'OFFLINE',
            detail: rotaAtiva
                ? 'Registros salvos localmente'
                : 'Sem comunicação com o servidor',
            backgroundColor: Colors.red.shade700,
          );
        }

        if (_conexaoRestabelecida) {
          return _Banner(
            icon: Icons.sync_rounded,
            label: 'SINCRONIZAÇÃO PENDENTE',
            detail: '$_pendencias registro(s) aguardando envio',
            backgroundColor: Colors.orange.shade800,
          );
        }

        return const SizedBox.shrink();
      },
    );
  }
}

class _Banner extends StatelessWidget {
  final IconData icon;
  final String label;
  final String detail;
  final Color backgroundColor;

  const _Banner({
    required this.icon,
    required this.label,
    required this.detail,
    required this.backgroundColor,
  });

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      bottom: false,
      child: Align(
        alignment: Alignment.topCenter,
        child: Container(
          width: double.infinity,
          margin: const EdgeInsets.fromLTRB(12, 8, 12, 0),
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
          decoration: BoxDecoration(
            color: backgroundColor,
            borderRadius: BorderRadius.circular(8),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withValues(alpha: .2),
                blurRadius: 8,
                offset: const Offset(0, 3),
              ),
            ],
          ),
          child: Row(
            children: [
              Icon(icon, color: Colors.white, size: 20),
              const SizedBox(width: 8),
              Text(
                label,
                style: const TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                  letterSpacing: .8,
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  detail,
                  textAlign: TextAlign.right,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(color: Colors.white, fontSize: 12),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
