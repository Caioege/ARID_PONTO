import 'dart:async';

import 'package:arid_rastreio/core/network/connectivity_service.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/offline/repository/offline_rastreio_repository.dart';
import 'package:arid_rastreio/modules/motorista/offline/service/offline_rastreio_service.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class OfflineConfigPage extends StatefulWidget {
  const OfflineConfigPage({super.key});

  @override
  State<OfflineConfigPage> createState() => _OfflineConfigPageState();
}

class _OfflineConfigPageState extends State<OfflineConfigPage> {
  final _service = locator<OfflineRastreioService>();

  late Future<ResumoPacoteOffline> _resumoFuture;
  StreamSubscription<List<ConnectivityResult>>? _connectivitySubscription;
  Timer? _connectivityTimer;
  bool _processando = false;
  bool _verificandoConectividade = false;
  bool? _conectado;

  @override
  void initState() {
    super.initState();
    _carregarResumo();
    _atualizarConectividade();
    _connectivitySubscription = Connectivity().onConnectivityChanged.listen(
      (_) => _atualizarConectividade(),
    );
    _connectivityTimer = Timer.periodic(
      const Duration(seconds: 5),
      (_) => _atualizarConectividade(),
    );
  }

  @override
  void dispose() {
    _connectivitySubscription?.cancel();
    _connectivityTimer?.cancel();
    super.dispose();
  }

  void _carregarResumo() {
    _resumoFuture = _service.obterResumoPacote();
  }

  Future<void> _atualizarConectividade() async {
    if (_verificandoConectividade) return;

    setState(() => _verificandoConectividade = true);
    try {
      final conectado = await ConnectivityService.isConnected();
      if (!mounted) return;
      setState(() => _conectado = conectado);
    } finally {
      if (mounted) {
        setState(() => _verificandoConectividade = false);
      }
    }
  }

  Future<void> _baixarPacote() async {
    setState(() => _processando = true);
    try {
      await _atualizarConectividade();
      final pacote = await _service.baixarESalvarPacote();
      if (!mounted) return;

      _carregarResumo();
      await showAppDialog(
        context: context,
        titulo: 'Pacote offline atualizado',
        mensagem:
            'Foram baixadas ${pacote.rotas.length} rota(s). O pacote é válido até ${_formatarDataHora(pacote.validoAte)}.',
        tipo: AppDialogType.sucesso,
      );
    } catch (e) {
      if (!mounted) return;
      await showAppDialog(
        context: context,
        titulo: 'Atenção',
        mensagem: e.toString(),
        tipo: AppDialogType.alerta,
      );
    } finally {
      if (mounted) setState(() => _processando = false);
    }
  }

  Future<void> _alterarModoOffline(bool habilitar) async {
    setState(() => _processando = true);
    try {
      if (habilitar) {
        await _atualizarConectividade();
      }
      await _service.habilitarModoOffline(habilitar);
      if (!mounted) return;

      _carregarResumo();
      await showAppDialog(
        context: context,
        titulo: habilitar
            ? 'Modo offline habilitado'
            : 'Modo offline desabilitado',
        mensagem: habilitar
            ? 'O pacote offline foi baixado e salvo neste aparelho.'
            : 'O funcionamento offline foi desabilitado neste aparelho.',
        tipo: AppDialogType.sucesso,
      );
    } catch (e) {
      if (!mounted) return;
      await showAppDialog(
        context: context,
        titulo: 'Atenção',
        mensagem: e.toString(),
        tipo: AppDialogType.alerta,
      );
    } finally {
      if (mounted) setState(() => _processando = false);
    }
  }

  Future<void> _sincronizarAgora() async {
    setState(() => _processando = true);
    try {
      await _atualizarConectividade();
      final quantidade = await _service.sincronizarPendencias();
      if (!mounted) return;

      _carregarResumo();
      await showAppDialog(
        context: context,
        titulo: 'Sincronização concluída',
        mensagem: 'Foram sincronizada(s) $quantidade execução(ões) offline.',
        tipo: AppDialogType.sucesso,
      );
    } catch (e) {
      if (!mounted) return;
      await showAppDialog(
        context: context,
        titulo: 'Falha na sincronização',
        mensagem: e.toString(),
        tipo: AppDialogType.alerta,
      );
    } finally {
      if (mounted) setState(() => _processando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return Scaffold(
      backgroundColor: colors.surface,
      body: FutureBuilder<ResumoPacoteOffline>(
        future: _resumoFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          final resumo =
              snapshot.data ??
              const ResumoPacoteOffline(
                modoOfflineHabilitado: false,
                ultimoDownload: null,
                validoAte: null,
                quantidadeRotas: 0,
                quantidadePendencias: 0,
              );

          final pacoteValido =
              resumo.validoAte != null &&
              !DateTime.now().isAfter(resumo.validoAte!);

          return RefreshIndicator(
            onRefresh: () async {
              setState(_carregarResumo);
              await _resumoFuture;
            },
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                _header(theme),
                const SizedBox(height: 16),
                _conectividadeCard(theme),
                const SizedBox(height: 12),
                _statusCard(theme, resumo, pacoteValido),
                const SizedBox(height: 12),
                _acaoCard(theme, resumo),
                const SizedBox(height: 12),
                _pendenciasCard(theme, resumo),
                const SizedBox(height: 24),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed:
                        _processando ||
                            _verificandoConectividade ||
                            _conectado != true
                        ? null
                        : _baixarPacote,
                    icon: _processando
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : const Icon(
                            Icons.download_rounded,
                            color: Colors.white,
                          ),
                    label: const Text(
                      'Atualizar pacote offline',
                      style: TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: colors.primary,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14),
                      ),
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _header(ThemeData theme) {
    final colors = theme.colorScheme;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: colors.primary.withValues(alpha: .12),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        children: [
          CircleAvatar(
            backgroundColor: colors.primary,
            child: const Icon(Icons.cloud_off_rounded, color: Colors.white),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              'Funcionamento offline',
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _statusCard(
    ThemeData theme,
    ResumoPacoteOffline resumo,
    bool pacoteValido,
  ) {
    final colors = theme.colorScheme;
    final statusColor = pacoteValido ? Colors.green : Colors.orange;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: _cardDecoration(),
      child: Column(
        children: [
          Row(
            children: [
              Icon(
                pacoteValido ? Icons.verified_rounded : Icons.warning_rounded,
                color: statusColor,
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  pacoteValido
                      ? 'Pacote offline válido'
                      : 'Pacote offline indisponível',
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colors.onSurface,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          _infoLine('Rotas baixadas', resumo.quantidadeRotas.toString()),
          _infoLine(
            'Último download',
            _formatarDataHora(resumo.ultimoDownload),
          ),
          _infoLine('Válido até', _formatarDataHora(resumo.validoAte)),
        ],
      ),
    );
  }

  Widget _conectividadeCard(ThemeData theme) {
    final verificando = _verificandoConectividade || _conectado == null;
    final conectado = _conectado == true;
    final color = verificando
        ? Colors.blueGrey
        : (conectado ? Colors.green : Colors.red);
    final icon = verificando
        ? Icons.sync_rounded
        : (conectado ? Icons.cloud_done_rounded : Icons.cloud_off_rounded);
    final titulo = verificando
        ? 'Verificando conexão'
        : (conectado ? 'Aplicativo online' : 'Aplicativo offline');
    final detalhe = verificando
        ? 'Validando comunicação com o servidor.'
        : (conectado
              ? 'O servidor de rastreio está acessível.'
              : 'Sem comunicação com o servidor. Pacotes e sincronização ficam bloqueados até a conexão voltar.');

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: color.withValues(alpha: .10),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withValues(alpha: .35)),
      ),
      child: Row(
        children: [
          Icon(icon, color: color),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  titulo,
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: color,
                  ),
                ),
                const SizedBox(height: 4),
                Text(detalhe, style: theme.textTheme.bodySmall),
              ],
            ),
          ),
          IconButton(
            tooltip: 'Atualizar conexão',
            onPressed: _processando || _verificandoConectividade
                ? null
                : _atualizarConectividade,
            icon: _verificandoConectividade
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.refresh_rounded),
          ),
        ],
      ),
    );
  }

  Widget _acaoCard(ThemeData theme, ResumoPacoteOffline resumo) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: _cardDecoration(),
      child: SwitchListTile(
        contentPadding: EdgeInsets.zero,
        secondary: Icon(
          resumo.modoOfflineHabilitado ? Icons.toggle_on : Icons.toggle_off,
          color: theme.colorScheme.primary,
        ),
        title: const Text(
          'Funcionamento offline',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        subtitle: const Text(
          'Ao habilitar, o app baixa os dados necessários para executar rotas sem conexão.',
        ),
        value: resumo.modoOfflineHabilitado,
        onChanged: _processando || _verificandoConectividade
            ? null
            : (habilitar) {
                if (habilitar && _conectado == false) {
                  showAppDialog(
                    context: context,
                    titulo: 'Aplicativo offline',
                    mensagem:
                        'Conecte o aparelho ao servidor para habilitar o funcionamento offline e baixar o pacote de rotas.',
                    tipo: AppDialogType.alerta,
                  );
                  return;
                }

                _alterarModoOffline(habilitar);
              },
      ),
    );
  }

  Widget _pendenciasCard(ThemeData theme, ResumoPacoteOffline resumo) {
    final temPendencia = resumo.quantidadePendencias > 0;

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: _cardDecoration(),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                temPendencia ? Icons.sync_problem_rounded : Icons.sync_rounded,
                color: temPendencia ? Colors.orange : Colors.green,
              ),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  'Dados pendentes de sincronização',
                  style: theme.textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          _infoLine(
            'Pendências locais',
            resumo.quantidadePendencias.toString(),
          ),
          const SizedBox(height: 12),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed:
                  temPendencia &&
                      !_processando &&
                      !_verificandoConectividade &&
                      _conectado == true
                  ? _sincronizarAgora
                  : null,
              icon: const Icon(Icons.sync_rounded),
              label: const Text('Sincronizar agora'),
            ),
          ),
        ],
      ),
    );
  }

  Widget _infoLine(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 5),
      child: Row(
        children: [
          Expanded(
            child: Text(label, style: TextStyle(color: Colors.grey[700])),
          ),
          Text(value, style: const TextStyle(fontWeight: FontWeight.w600)),
        ],
      ),
    );
  }

  BoxDecoration _cardDecoration() {
    return BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(8),
      boxShadow: [
        BoxShadow(
          color: Colors.black.withValues(alpha: .08),
          blurRadius: 8,
          offset: const Offset(0, 3),
        ),
      ],
    );
  }

  String _formatarDataHora(DateTime? data) {
    if (data == null) return '-';
    return DateFormat('dd/MM/yyyy HH:mm').format(data);
  }
}
