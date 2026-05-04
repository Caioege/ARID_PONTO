import 'dart:async';

import 'package:arid_rastreio/core/network/connectivity_service.dart';
import 'package:arid_rastreio/core/service/rota_tracking_service.dart';
import 'package:arid_rastreio/core/widgets/rastreio_tile_layer.dart';
import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/presenca_rota_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/page/rota_chat_page.dart';
import 'package:arid_rastreio/modules/motorista/rotas/service/motorista_rotas_service.dart';
import 'package:arid_rastreio/modules/motorista/rotas/store/parada_store.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
// ignore: depend_on_referenced_packages
import 'package:latlong2/latlong.dart';
import 'package:mobx/mobx.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/shared/functions/functions.dart';

// ignore_for_file: deprecated_member_use, use_build_context_synchronously

class MotoristaRotasPage extends StatefulWidget {
  const MotoristaRotasPage({super.key});

  @override
  State<MotoristaRotasPage> createState() => _MotoristaRotasPageState();
}

class _MotoristaRotasPageState extends State<MotoristaRotasPage> {
  final checklistController = locator<ChecklistController>();
  final rotasController = locator<MotoristaRotasController>();
  final MotoristaRotasService _rotasService = MotoristaRotasService();

  Timer? _chatBadgeTimer;
  int _chatNaoLidas = 0;
  bool _consultandoChatNaoLidas = false;
  int? _trajetoExecucaoId;
  bool _carregandoTrajeto = false;
  List<LatLng> _trajetoPontos = [];
  bool _rotaAberta = true;
  bool _veiculoAberto = false;
  bool _checklistAberto = false;

  @override
  void initState() {
    super.initState();
    if (checklistController.rotasFuture == null) {
      checklistController.carregarRotas();
    }
    _sincronizarEtapasPreparacao();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _atualizarChatNaoLidas();
    });
    _chatBadgeTimer = Timer.periodic(const Duration(seconds: 45), (_) {
      _atualizarChatNaoLidas();
    });
  }

  @override
  void dispose() {
    _chatBadgeTimer?.cancel();
    super.dispose();
  }

  void _sincronizarEtapasPreparacao() {
    _rotaAberta = checklistController.rotaSelecionada == null;
    _veiculoAberto =
        checklistController.rotaSelecionada != null &&
        checklistController.veiculoSelecionado == null;
    _checklistAberto =
        checklistController.rotaSelecionada != null &&
        checklistController.veiculoSelecionado != null &&
        !checklistController.checklistSalvo;
  }

  Future<void> _atualizarChatNaoLidas() async {
    if (_consultandoChatNaoLidas) return;

    final rota = rotasController.rotaAtual;
    if (rota == null) {
      if (mounted && _chatNaoLidas != 0) {
        setState(() => _chatNaoLidas = 0);
      }
      return;
    }

    _consultandoChatNaoLidas = true;
    try {
      if (!await ConnectivityService.isConnected()) {
        if (mounted && _chatNaoLidas != 0) {
          setState(() => _chatNaoLidas = 0);
        }
        return;
      }

      final quantidade = await _rotasService.obterQuantidadeChatNaoLida(
        rota.id,
      );
      if (!mounted) return;
      setState(() => _chatNaoLidas = quantidade);
    } catch (_) {
      // A consulta do badge não deve interromper o acompanhamento da rota.
    } finally {
      _consultandoChatNaoLidas = false;
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Observer(
      builder: (_) {
        if (checklistController.erroProcessamento != null) {
          WidgetsBinding.instance.addPostFrameCallback((_) async {
            await showAppDialog(
              context: context,
              titulo: 'Atenção',
              mensagem: checklistController.erroProcessamento!,
              tipo: AppDialogType.alerta,
            );
            checklistController.erroProcessamento = null;
          });
        }

        return Stack(
          children: [
            RefreshIndicator(
              onRefresh: () async {
                await checklistController.carregarRotas();
                await rotasController.obterRotaEmAndamento();
                await _atualizarChatNaoLidas();
              },
              child: SingleChildScrollView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.fromLTRB(16, 12, 16, 112),
                child: Column(
                  children: [
                    _header(theme),
                    const SizedBox(height: 14),
                    rotasController.rotaAtual != null
                        ? _rotaAtiva(theme)
                        : _estadoAntesDeIniciar(theme),
                  ],
                ),
              ),
            ),
            Positioned(
              left: 16,
              right: 16,
              bottom: 16,
              child: _acaoPrincipalFixa(theme),
            ),
          ],
        );
      },
    );
  }

  Widget _header(ThemeData theme) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: .25),
            blurRadius: 3,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: theme.primaryColor.withValues(alpha: .18),
          borderRadius: BorderRadius.circular(18),
        ),
        child: Row(
          children: [
            CircleAvatar(
              backgroundColor: theme.primaryColor,
              child: const Icon(Icons.alt_route, color: Colors.white),
            ),
            const SizedBox(width: 12),
            const Expanded(
              child: Text(
                'Rotas / Viagens',
                style: TextStyle(fontWeight: FontWeight.bold),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _acaoPrincipalFixa(ThemeData theme) {
    final rotaAtual = rotasController.rotaAtual;
    final emAndamento = rotaAtual?.emAndamento == true;
    final podeIniciar =
        rotaAtual == null &&
        checklistController.rotaSelecionada != null &&
        checklistController.veiculoSelecionado != null &&
        checklistController.checklistSalvo;

    if (emAndamento) {
      return _botaoGrande(
        theme,
        texto: 'Encerrar rota',
        icone: Icons.stop_circle,
        cor: Colors.red,
        carregando: rotasController.carregando,
        onPressed: () async {
          final confirmar = await showAppDialog(
            context: context,
            titulo: 'Encerrar rota',
            mensagem: 'Deseja realmente encerrar a rota?',
            tipo: AppDialogType.interrogacao,
          );

          if (confirmar == true) {
            await rotasController.encerrarRota();
            await RotaTrackingService.stop();
            await _mostrarResumoFinalizacao();
          }
        },
      );
    }

    if (rotaAtual != null) {
      return _botaoGrande(
        theme,
        texto: 'Escolher outra rota',
        icone: Icons.arrow_back,
        cor: theme.primaryColor,
        carregando: rotasController.carregando,
        onPressed: _sairDaVisualizacaoExecucao,
      );
    }

    return _botaoGrande(
      theme,
      texto: podeIniciar ? 'Iniciar rota agora' : 'Prepare rota e checklist',
      icone: podeIniciar ? Icons.play_arrow_rounded : Icons.checklist,
      cor: podeIniciar ? theme.primaryColor : Colors.grey,
      carregando: rotasController.carregando,
      onPressed: podeIniciar ? _iniciarRotaSelecionada : null,
    );
  }

  Future<void> _sairDaVisualizacaoExecucao() async {
    final rotaAtual = rotasController.rotaAtual;
    if (rotaAtual?.emAndamento == true) return;

    rotasController.limpar();
    checklistController.limpar();

    setState(() {
      _chatNaoLidas = 0;
      _trajetoExecucaoId = null;
      _trajetoPontos = [];
      _carregandoTrajeto = false;
      _rotaAberta = true;
      _veiculoAberto = false;
      _checklistAberto = false;
    });

    await checklistController.carregarRotas();
  }

  Widget _botaoGrande(
    ThemeData theme, {
    required String texto,
    required IconData icone,
    required Color cor,
    required VoidCallback? onPressed,
    bool carregando = false,
  }) {
    return SafeArea(
      top: false,
      child: SizedBox(
        width: double.infinity,
        height: 68,
        child: ElevatedButton.icon(
          onPressed: carregando ? null : onPressed,
          icon: carregando
              ? const SizedBox(
                  width: 24,
                  height: 24,
                  child: CircularProgressIndicator(
                    strokeWidth: 2.4,
                    color: Colors.white,
                  ),
                )
              : Icon(icone, color: Colors.white, size: 30),
          label: Text(
            texto,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 18,
              fontWeight: FontWeight.w800,
            ),
          ),
          style: ElevatedButton.styleFrom(
            backgroundColor: cor,
            disabledBackgroundColor: Colors.grey.shade500,
            elevation: 10,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(22),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _iniciarRotaSelecionada() async {
    if (rotasController.carregando) return;
    final rota = checklistController.rotaSelecionada;
    final veiculo = checklistController.veiculoSelecionado;
    if (rota == null ||
        veiculo == null ||
        !checklistController.checklistSalvo) {
      return;
    }

    final presencas = await _abrirConferenciaPresenca(rota);
    if (presencas == null) return;
    if (!await _validarPresencasObrigatorias(rota, presencas)) return;

    try {
      await solicitarPermissoes();

      final RotaExecucaoDTO rotaExecucao = await rotasController.iniciarRota(
        rotaId: rota.id,
        veiculoId: veiculo.id,
        checklistId: checklistController.ultimaExecucaoId,
        pacientesPresenca: presencas.pacientes,
        profissionaisPresenca: presencas.profissionais,
      );

      await FlutterForegroundTask.saveData(
        key: 'rotaExecucaoId',
        value: rotaExecucao.id.toString(),
      );
      await FlutterForegroundTask.saveData(
        key: 'execucaoOffline',
        value: rotaExecucao.execucaoOffline.toString(),
      );
      await FlutterForegroundTask.saveData(
        key: 'localExecucaoId',
        value: rotaExecucao.localExecucaoId ?? '',
      );

      await RotaTrackingService.start(
        descricaoRota: rotaExecucao.descricao,
        execucaoOffline: rotaExecucao.execucaoOffline,
      );
      _atualizarChatNaoLidas();

      if (context.mounted) {
        await showAppDialog(
          context: context,
          titulo: 'Rota iniciada',
          mensagem: 'A rota foi iniciada com sucesso.',
          tipo: AppDialogType.sucesso,
        );
      }
    } catch (e) {
      if (context.mounted) {
        await showAppDialog(
          context: context,
          titulo: 'Atenção',
          mensagem: extrairMensagemErro(e),
          tipo: AppDialogType.alerta,
        );
      }
    }
  }

  Future<bool> _validarPresencasObrigatorias(
    RotaChecklistDTO rota,
    _ManifestoPresenca presencas,
  ) async {
    final temPacienteOuAcompanhantePresente = presencas.pacientes.any(
      (p) => p.presente || p.acompanhantePresente,
    );
    final temProfissionalPresente = presencas.profissionais.any(
      (p) => p.presente,
    );

    if (!rota.permiteIniciarSemPacienteAcompanhante &&
        !temPacienteOuAcompanhantePresente) {
      await showAppDialog(
        context: context,
        titulo: 'Presença obrigatória',
        mensagem:
            'Esta rota exige ao menos um paciente ou acompanhante presente para iniciar.',
        tipo: AppDialogType.alerta,
      );
      return false;
    }

    if (!rota.permiteIniciarSemProfissional && !temProfissionalPresente) {
      await showAppDialog(
        context: context,
        titulo: 'Presença obrigatória',
        mensagem:
            'Esta rota exige ao menos um profissional presente para iniciar.',
        tipo: AppDialogType.alerta,
      );
      return false;
    }

    return true;
  }

  Future<_ManifestoPresenca?> _abrirConferenciaPresenca(
    RotaChecklistDTO rota,
  ) async {
    final pacientes = rota.pacientes
        .map(
          (p) => _PacientePresencaItem(
            pacienteId: p.pacienteId,
            nome: p.nome,
            cpf: p.cpf,
            telefone: p.telefone,
            presente: true,
            possuiAcompanhante: p.possuiAcompanhante,
            acompanhantePresente: p.possuiAcompanhante,
            acompanhanteNome: p.acompanhanteNome,
            acompanhanteCPF: p.acompanhanteCPF,
          ),
        )
        .toList();
    final profissionais = {
      for (final p in rota.profissionais) p.servidorId: true,
    };

    return showDialog<_ManifestoPresenca>(
      context: context,
      barrierDismissible: false,
      builder: (dialogContext) {
        return StatefulBuilder(
          builder: (context, setModalState) {
            final pacientesDisponiveis = rota.pacientesDisponiveis
                .where(
                  (p) =>
                      !pacientes.any((item) => item.pacienteId == p.pacienteId),
                )
                .toList();

            return Dialog(
              insetPadding: const EdgeInsets.all(12),
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 620),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'Conferir presença',
                        style: TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      const SizedBox(height: 4),
                      const Text(
                        'Marque quem está presente antes de iniciar a rota.',
                        style: TextStyle(color: Colors.black54),
                      ),
                      const SizedBox(height: 14),
                      Flexible(
                        child: SingleChildScrollView(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              _tituloSecaoPresenca('Pacientes e acompanhantes'),
                              if (pacientes.isEmpty)
                                const _TextoVazioPresenca(
                                  'Nenhum paciente vinculado à rota.',
                                ),
                              ...pacientes.map(
                                (p) => _cardPacientePresenca(p, setModalState),
                              ),
                              Row(
                                children: [
                                  Expanded(
                                    child: OutlinedButton.icon(
                                      onPressed: pacientesDisponiveis.isEmpty
                                          ? null
                                          : () async {
                                              final selecionado =
                                                  await _selecionarPacienteExistente(
                                                    pacientesDisponiveis,
                                                  );
                                              if (selecionado == null) return;
                                              setModalState(() {
                                                pacientes.add(
                                                  _PacientePresencaItem(
                                                    pacienteId:
                                                        selecionado.pacienteId,
                                                    nome: selecionado.nome,
                                                    cpf: selecionado.cpf,
                                                    telefone:
                                                        selecionado.telefone,
                                                    presente: true,
                                                    possuiAcompanhante:
                                                        selecionado
                                                            .acompanhanteNome
                                                            ?.isNotEmpty ==
                                                        true,
                                                    acompanhantePresente:
                                                        selecionado
                                                            .acompanhanteNome
                                                            ?.isNotEmpty ==
                                                        true,
                                                    acompanhanteNome:
                                                        selecionado
                                                            .acompanhanteNome,
                                                    acompanhanteCPF: selecionado
                                                        .acompanhanteCPF,
                                                    incluirNaRota: true,
                                                  ),
                                                );
                                              });
                                            },
                                      icon: const Icon(Icons.person_search),
                                      label: const Text('Adicionar existente'),
                                    ),
                                  ),
                                  const SizedBox(width: 8),
                                  Expanded(
                                    child: OutlinedButton.icon(
                                      onPressed: () async {
                                        final novo =
                                            await _preencherNovoPaciente();
                                        if (novo == null) return;
                                        setModalState(() {
                                          pacientes.add(novo);
                                        });
                                      },
                                      icon: const Icon(Icons.person_add_alt_1),
                                      label: const Text('Novo paciente'),
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 18),
                              _tituloSecaoPresenca('Profissionais'),
                              if (rota.profissionais.isEmpty)
                                const _TextoVazioPresenca(
                                  'Nenhum profissional vinculado à rota.',
                                ),
                              ...rota.profissionais.map(
                                (p) => CheckboxListTile(
                                  value: profissionais[p.servidorId] ?? true,
                                  onChanged: (value) => setModalState(
                                    () => profissionais[p.servidorId] =
                                        value ?? false,
                                  ),
                                  title: Text(
                                    p.nome,
                                    style: const TextStyle(
                                      fontWeight: FontWeight.w800,
                                    ),
                                  ),
                                  subtitle: Text(p.funcao ?? 'Profissional'),
                                  controlAffinity:
                                      ListTileControlAffinity.leading,
                                  contentPadding: EdgeInsets.zero,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 12),
                      Row(
                        children: [
                          TextButton(
                            onPressed: () => Navigator.pop(dialogContext),
                            child: const Text('Cancelar'),
                          ),
                          const Spacer(),
                          ElevatedButton.icon(
                            onPressed: () {
                              Navigator.pop(
                                dialogContext,
                                _ManifestoPresenca(
                                  pacientes: pacientes
                                      .map((p) => p.toPresenca())
                                      .toList(),
                                  profissionais: rota.profissionais
                                      .map(
                                        (p) => PresencaProfissionalRotaDTO(
                                          servidorId: p.servidorId,
                                          nome: p.nome,
                                          presente:
                                              profissionais[p.servidorId] ??
                                              true,
                                        ),
                                      )
                                      .toList(),
                                ),
                              );
                            },
                            icon: const Icon(Icons.play_arrow),
                            label: const Text('Iniciar rota'),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
            );
          },
        );
      },
    );
  }

  Widget _tituloSecaoPresenca(String texto) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Text(
        texto,
        style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w900),
      ),
    );
  }

  Widget _cardPacientePresenca(
    _PacientePresencaItem paciente,
    void Function(void Function()) setModalState,
  ) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 4),
        child: Column(
          children: [
            CheckboxListTile(
              value: paciente.presente,
              onChanged: (value) =>
                  setModalState(() => paciente.presente = value ?? false),
              title: Text(
                paciente.nome,
                style: const TextStyle(fontWeight: FontWeight.w800),
              ),
              subtitle: Text(paciente.cpf ?? paciente.telefone ?? 'Paciente'),
              controlAffinity: ListTileControlAffinity.leading,
            ),
            if (paciente.possuiAcompanhante ||
                (paciente.acompanhanteNome?.isNotEmpty ?? false))
              CheckboxListTile(
                value: paciente.acompanhantePresente,
                onChanged: (value) => setModalState(
                  () => paciente.acompanhantePresente = value ?? false,
                ),
                title: Text(
                  paciente.acompanhanteNome ?? 'Acompanhante',
                  style: const TextStyle(fontWeight: FontWeight.w700),
                ),
                subtitle: const Text('Acompanhante'),
                controlAffinity: ListTileControlAffinity.leading,
              ),
          ],
        ),
      ),
    );
  }

  Future<PacienteDisponivelDTO?> _selecionarPacienteExistente(
    List<PacienteDisponivelDTO> pacientes,
  ) {
    PacienteDisponivelDTO? selecionado;
    return showDialog<PacienteDisponivelDTO>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Adicionar paciente'),
        content: DropdownButtonFormField<PacienteDisponivelDTO>(
          isExpanded: true,
          items: pacientes
              .map((p) => DropdownMenuItem(value: p, child: Text(p.nome)))
              .toList(),
          onChanged: (value) => selecionado = value,
          decoration: const InputDecoration(labelText: 'Paciente cadastrado'),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, selecionado),
            child: const Text('Adicionar'),
          ),
        ],
      ),
    );
  }

  Future<_PacientePresencaItem?> _preencherNovoPaciente() {
    final nome = TextEditingController();
    final cpf = TextEditingController();
    final telefone = TextEditingController();
    final acompanhanteNome = TextEditingController();
    final acompanhanteCPF = TextEditingController();

    return showDialog<_PacientePresencaItem>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Novo paciente'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: nome,
                decoration: const InputDecoration(labelText: 'Nome completo'),
              ),
              TextField(
                controller: cpf,
                decoration: const InputDecoration(labelText: 'CPF'),
                keyboardType: TextInputType.number,
              ),
              TextField(
                controller: telefone,
                decoration: const InputDecoration(labelText: 'Telefone'),
                keyboardType: TextInputType.phone,
              ),
              TextField(
                controller: acompanhanteNome,
                decoration: const InputDecoration(
                  labelText: 'Nome do acompanhante',
                ),
              ),
              TextField(
                controller: acompanhanteCPF,
                decoration: const InputDecoration(
                  labelText: 'CPF do acompanhante',
                ),
                keyboardType: TextInputType.number,
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            onPressed: () {
              if (nome.text.trim().isEmpty) return;
              Navigator.pop(
                context,
                _PacientePresencaItem(
                  nome: nome.text.trim(),
                  cpf: cpf.text.trim().isEmpty ? null : cpf.text.trim(),
                  telefone: telefone.text.trim().isEmpty
                      ? null
                      : telefone.text.trim(),
                  presente: true,
                  novoPaciente: true,
                  incluirNaRota: true,
                  possuiAcompanhante: acompanhanteNome.text.trim().isNotEmpty,
                  acompanhantePresente: acompanhanteNome.text.trim().isNotEmpty,
                  acompanhanteNome: acompanhanteNome.text.trim().isEmpty
                      ? null
                      : acompanhanteNome.text.trim(),
                  acompanhanteCPF: acompanhanteCPF.text.trim().isEmpty
                      ? null
                      : acompanhanteCPF.text.trim(),
                ),
              );
            },
            child: const Text('Adicionar'),
          ),
        ],
      ),
    );
  }

  Widget _estadoAntesDeIniciar(ThemeData theme) {
    return _estadoPreparacaoAntesDeIniciar(theme);
  }

  Widget _estadoPreparacaoAntesDeIniciar(ThemeData theme) {
    final rota = checklistController.rotaSelecionada;
    if (rota != null &&
        (rota.rotaFinalizada || rotasController.rotaFinalizadaId == rota.id)) {
      return Column(
        children: [
          _painelPreparacao(theme),
          const SizedBox(height: 14),
          _rotaFinalizadaCard(theme),
        ],
      );
    }

    return Column(
      children: [
        _painelPreparacao(theme),
        if (checklistController.rotaSelecionada != null &&
            checklistController.veiculoSelecionado != null &&
            checklistController.checklistSalvo) ...[
          const SizedBox(height: 14),
          _cardResumoRota(theme),
          const SizedBox(height: 12),
          _localizacaoInicial(theme),
        ],
      ],
    );
  }

  Widget _painelPreparacao(ThemeData theme) {
    if (checklistController.rotasFuture == null ||
        checklistController.rotasFuture!.status == FutureStatus.pending) {
      return _cartaoSimples(
        child: const Padding(
          padding: EdgeInsets.all(22),
          child: Column(
            children: [
              CircularProgressIndicator(),
              SizedBox(height: 14),
              Text('Carregando rotas e veículos...'),
            ],
          ),
        ),
      );
    }

    final rotas = checklistController.rotasFuture!.value ?? [];

    return Column(
      children: [
        _progressoPreparacao(theme),
        const SizedBox(height: 12),
        _secaoRotaPreparacao(theme, rotas),
        const SizedBox(height: 12),
        _secaoVeiculoPreparacao(theme),
        const SizedBox(height: 12),
        _secaoChecklistPreparacao(theme),
      ],
    );
  }

  Widget _progressoPreparacao(ThemeData theme) {
    final rotaOk = checklistController.rotaSelecionada != null;
    final veiculoOk = checklistController.veiculoSelecionado != null;
    final checklistOk = checklistController.checklistSalvo;

    return _cartaoSimples(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            _etapaPreparacao(theme, 'Rota', rotaOk, true),
            _linhaEtapaPreparacao(theme, rotaOk),
            _etapaPreparacao(theme, 'Veículo', veiculoOk, rotaOk),
            _linhaEtapaPreparacao(theme, veiculoOk),
            _etapaPreparacao(theme, 'Checklist', checklistOk, veiculoOk),
          ],
        ),
      ),
    );
  }

  Widget _linhaEtapaPreparacao(ThemeData theme, bool ativa) {
    return Expanded(
      child: Container(
        height: 3,
        margin: const EdgeInsets.symmetric(horizontal: 8),
        color: ativa ? theme.primaryColor : Colors.grey.withValues(alpha: .25),
      ),
    );
  }

  Widget _etapaPreparacao(
    ThemeData theme,
    String texto,
    bool concluida,
    bool ativa,
  ) {
    final color = concluida
        ? Colors.green
        : ativa
        ? theme.primaryColor
        : Colors.grey;

    return Column(
      children: [
        Container(
          width: 40,
          height: 40,
          decoration: BoxDecoration(
            color: color.withValues(alpha: concluida ? .16 : .10),
            shape: BoxShape.circle,
          ),
          child: Icon(
            concluida ? Icons.check : Icons.circle,
            color: color,
            size: concluida ? 24 : 12,
          ),
        ),
        const SizedBox(height: 5),
        Text(
          texto,
          style: TextStyle(
            color: color,
            fontSize: 12,
            fontWeight: FontWeight.w700,
          ),
        ),
      ],
    );
  }

  Widget _secaoRotaPreparacao(ThemeData theme, List<RotaChecklistDTO> rotas) {
    final selecionada = checklistController.rotaSelecionada == null
        ? null
        : rotas
              .where((r) => r.id == checklistController.rotaSelecionada!.id)
              .firstOrNull;

    return _secaoCollapseMotorista(
      theme: theme,
      titulo: '1. Rota',
      subtitulo:
          checklistController.rotaSelecionada?.descricao ??
          'Selecione a rota de trabalho',
      icone: Icons.alt_route,
      concluida: checklistController.rotaSelecionada != null,
      aberta: _rotaAberta,
      onToggle: () => setState(() => _rotaAberta = !_rotaAberta),
      child: _campoSelecaoPreparacao(
        theme: theme,
        label: 'Rota',
        texto: selecionada == null
            ? 'Selecionar rota'
            : _descricaoRotaSelecao(selecionada),
        icone: Icons.alt_route,
        selecionado: selecionada != null,
        habilitado: !rotasController.rotaIniciada,
        onTap: () async {
          final rota = await _abrirSeletorRota(rotas);
          if (rota == null) return;
          await checklistController.selecionarRota(rota);
          if (!mounted) return;
          setState(() {
            _rotaAberta = false;
            _veiculoAberto = true;
            _checklistAberto = false;
          });
        },
      ),
    );
  }

  Widget _secaoVeiculoPreparacao(ThemeData theme) {
    if (checklistController.rotaSelecionada == null) {
      return _secaoCollapseMotorista(
        theme: theme,
        titulo: '2. Veículo',
        subtitulo: 'Selecione a rota para carregar os veículos.',
        icone: Icons.directions_car,
        concluida: false,
        aberta: false,
        onToggle: () {},
        child: const SizedBox.shrink(),
      );
    }

    return Observer(
      builder: (_) {
        if (checklistController.veiculosFuture == null ||
            checklistController.veiculosFuture!.status ==
                FutureStatus.pending) {
          return _secaoCollapseMotorista(
            theme: theme,
            titulo: '2. Veículo',
            subtitulo: 'Carregando veículos...',
            icone: Icons.directions_car,
            concluida: false,
            aberta: true,
            onToggle: () {},
            child: const Padding(
              padding: EdgeInsets.all(16),
              child: Center(child: CircularProgressIndicator()),
            ),
          );
        }

        final veiculos = checklistController.veiculosFuture!.value ?? [];
        final selecionado = checklistController.veiculoSelecionado == null
            ? null
            : veiculos
                  .where(
                    (v) => v.id == checklistController.veiculoSelecionado!.id,
                  )
                  .firstOrNull;

        return _secaoCollapseMotorista(
          theme: theme,
          titulo: '2. Veículo',
          subtitulo: checklistController.veiculoSelecionado == null
              ? 'Selecione o veículo'
              : '${checklistController.veiculoSelecionado!.nome} - ${checklistController.veiculoSelecionado!.placa}',
          icone: Icons.directions_car,
          concluida: checklistController.veiculoSelecionado != null,
          aberta: _veiculoAberto,
          onToggle: () => setState(() => _veiculoAberto = !_veiculoAberto),
          child: _campoSelecaoPreparacao(
            theme: theme,
            label: 'Veículo',
            texto: selecionado == null
                ? 'Selecionar veículo'
                : _descricaoVeiculoSelecao(selecionado),
            icone: Icons.directions_car,
            selecionado: selecionado != null,
            habilitado: !rotasController.rotaIniciada && veiculos.isNotEmpty,
            onTap: () async {
              final veiculo = await _abrirSeletorVeiculo(veiculos);
              if (veiculo == null) return;
              checklistController.selecionarVeiculo(veiculo);
              setState(() {
                _veiculoAberto = false;
                _checklistAberto = true;
              });
            },
          ),
        );
      },
    );
  }

  Widget _campoSelecaoPreparacao({
    required ThemeData theme,
    required String label,
    required String texto,
    required IconData icone,
    required bool selecionado,
    required bool habilitado,
    required VoidCallback onTap,
  }) {
    final cor = habilitado ? theme.primaryColor : Colors.grey;

    return InkWell(
      onTap: habilitado ? onTap : null,
      borderRadius: BorderRadius.circular(18),
      child: Container(
        width: double.infinity,
        constraints: const BoxConstraints(minHeight: 72),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        decoration: BoxDecoration(
          color: habilitado
              ? theme.primaryColor.withValues(alpha: .05)
              : Colors.grey.withValues(alpha: .07),
          borderRadius: BorderRadius.circular(18),
          border: Border.all(
            color: selecionado
                ? theme.primaryColor
                : Colors.grey.withValues(alpha: .28),
          ),
        ),
        child: Row(
          children: [
            CircleAvatar(
              radius: 22,
              backgroundColor: cor.withValues(alpha: .12),
              child: Icon(icone, color: cor),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    label,
                    style: TextStyle(
                      color: Colors.grey[700],
                      fontSize: 12,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 3),
                  Text(
                    texto,
                    maxLines: 3,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      color: habilitado ? Colors.black87 : Colors.grey[600],
                      fontSize: 16,
                      fontWeight: FontWeight.w800,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(width: 10),
            Icon(Icons.keyboard_arrow_up, color: cor, size: 30),
          ],
        ),
      ),
    );
  }

  Future<RotaChecklistDTO?> _abrirSeletorRota(List<RotaChecklistDTO> rotas) {
    return _abrirSeletorInferior<RotaChecklistDTO>(
      titulo: 'Selecionar rota',
      itens: rotas,
      selecionado: checklistController.rotaSelecionada,
      icone: Icons.alt_route,
      texto: _descricaoRotaSelecao,
      detalhe: (rota) => rota.codigo.isEmpty ? null : 'Código: ${rota.codigo}',
      itemSelecionado: (a, b) => a.id == b.id,
    );
  }

  Future<VeiculoChecklistDTO?> _abrirSeletorVeiculo(
    List<VeiculoChecklistDTO> veiculos,
  ) {
    return _abrirSeletorInferior<VeiculoChecklistDTO>(
      titulo: 'Selecionar veículo',
      itens: veiculos,
      selecionado: checklistController.veiculoSelecionado,
      icone: Icons.directions_car,
      texto: _descricaoVeiculoSelecao,
      detalhe: (veiculo) => veiculo.placa,
      itemSelecionado: (a, b) => a.id == b.id,
    );
  }

  String _descricaoRotaSelecao(RotaChecklistDTO rota) {
    return rota.rotaFinalizada
        ? '${rota.descricao} (finalizada hoje)'
        : rota.descricao;
  }

  String _descricaoVeiculoSelecao(VeiculoChecklistDTO veiculo) {
    return '${veiculo.nome} - ${veiculo.placa}';
  }

  Future<T?> _abrirSeletorInferior<T>({
    required String titulo,
    required List<T> itens,
    required T? selecionado,
    required IconData icone,
    required String Function(T item) texto,
    required String? Function(T item) detalhe,
    required bool Function(T a, T b) itemSelecionado,
  }) {
    return showModalBottomSheet<T>(
      context: context,
      isScrollControlled: true,
      useSafeArea: true,
      backgroundColor: Colors.transparent,
      builder: (context) {
        final theme = Theme.of(context);

        return FractionallySizedBox(
          heightFactor: .70,
          child: Container(
            decoration: const BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
            ),
            child: Column(
              children: [
                const SizedBox(height: 10),
                Container(
                  width: 44,
                  height: 5,
                  decoration: BoxDecoration(
                    color: Colors.grey.withValues(alpha: .35),
                    borderRadius: BorderRadius.circular(999),
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.fromLTRB(18, 16, 8, 12),
                  child: Row(
                    children: [
                      Icon(icone, color: theme.primaryColor),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          titulo,
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                      ),
                      IconButton(
                        onPressed: () => Navigator.pop(context),
                        icon: const Icon(Icons.close),
                      ),
                    ],
                  ),
                ),
                const Divider(height: 1),
                Expanded(
                  child: itens.isEmpty
                      ? const Center(child: Text('Nenhum item disponível.'))
                      : ListView.separated(
                          padding: const EdgeInsets.all(14),
                          itemCount: itens.length,
                          separatorBuilder: (context, index) =>
                              const SizedBox(height: 10),
                          itemBuilder: (context, index) {
                            final item = itens[index];
                            final marcado =
                                selecionado != null &&
                                itemSelecionado(item, selecionado);
                            final descricao = detalhe(item);

                            return InkWell(
                              onTap: () => Navigator.pop(context, item),
                              borderRadius: BorderRadius.circular(18),
                              child: Container(
                                constraints: const BoxConstraints(
                                  minHeight: 74,
                                ),
                                padding: const EdgeInsets.all(14),
                                decoration: BoxDecoration(
                                  color: marcado
                                      ? theme.primaryColor.withValues(
                                          alpha: .10,
                                        )
                                      : Colors.grey.withValues(alpha: .04),
                                  borderRadius: BorderRadius.circular(18),
                                  border: Border.all(
                                    color: marcado
                                        ? theme.primaryColor
                                        : Colors.grey.withValues(alpha: .20),
                                  ),
                                ),
                                child: Row(
                                  children: [
                                    Icon(
                                      marcado
                                          ? Icons.check_circle
                                          : Icons.radio_button_unchecked,
                                      color: marcado
                                          ? theme.primaryColor
                                          : Colors.grey,
                                      size: 30,
                                    ),
                                    const SizedBox(width: 12),
                                    Expanded(
                                      child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        mainAxisAlignment:
                                            MainAxisAlignment.center,
                                        children: [
                                          Text(
                                            texto(item),
                                            maxLines: 4,
                                            overflow: TextOverflow.ellipsis,
                                            style: const TextStyle(
                                              fontSize: 16,
                                              fontWeight: FontWeight.w800,
                                            ),
                                          ),
                                          if (descricao != null &&
                                              descricao.isNotEmpty) ...[
                                            const SizedBox(height: 4),
                                            Text(
                                              descricao,
                                              maxLines: 2,
                                              overflow: TextOverflow.ellipsis,
                                              style: TextStyle(
                                                color: Colors.grey[700],
                                                fontSize: 13,
                                                fontWeight: FontWeight.w600,
                                              ),
                                            ),
                                          ],
                                        ],
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _secaoChecklistPreparacao(ThemeData theme) {
    if (checklistController.veiculoSelecionado == null) {
      return _secaoCollapseMotorista(
        theme: theme,
        titulo: '3. Checklist',
        subtitulo: 'Selecione um veículo para exibir os itens.',
        icone: Icons.checklist,
        concluida: false,
        aberta: false,
        onToggle: () {},
        child: const SizedBox.shrink(),
      );
    }

    return _secaoCollapseMotorista(
      theme: theme,
      titulo: '3. Checklist',
      subtitulo:
          '${checklistController.itensMarcados} de ${checklistController.totalItens} itens verificados',
      icone: Icons.checklist,
      concluida: checklistController.checklistSalvo,
      aberta: _checklistAberto,
      onToggle: () => setState(() => _checklistAberto = !_checklistAberto),
      child: Column(
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(12),
            child: LinearProgressIndicator(
              value: checklistController.progressoChecklist,
              minHeight: 10,
              backgroundColor: Colors.grey.withValues(alpha: .2),
              valueColor: AlwaysStoppedAnimation(theme.primaryColor),
            ),
          ),
          const SizedBox(height: 14),
          ...checklistController.veiculoSelecionado!.checklist.map((item) {
            return Observer(
              builder: (_) {
                return InkWell(
                  borderRadius: BorderRadius.circular(16),
                  onTap: rotasController.rotaIniciada
                      ? null
                      : () => checklistController.alternarCheck(item),
                  child: Container(
                    constraints: const BoxConstraints(minHeight: 58),
                    margin: const EdgeInsets.only(bottom: 10),
                    padding: const EdgeInsets.all(14),
                    decoration: BoxDecoration(
                      color: item.checked
                          ? Colors.green.withValues(alpha: .10)
                          : Colors.grey.withValues(alpha: .04),
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(
                        color: item.checked
                            ? Colors.green
                            : Colors.grey.withValues(alpha: .25),
                      ),
                    ),
                    child: Row(
                      children: [
                        Icon(
                          item.checked
                              ? Icons.check_circle
                              : Icons.radio_button_unchecked,
                          color: item.checked ? Colors.green : Colors.grey,
                          size: 30,
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Text(
                            item.descricao,
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            );
          }),
          const SizedBox(height: 8),
          SizedBox(
            width: double.infinity,
            height: 58,
            child: ElevatedButton(
              onPressed:
                  checklistController.salvando ||
                      rotasController.rotaIniciada ||
                      checklistController.rotaSelecionada!.rotaFinalizada
                  ? null
                  : () async {
                      await checklistController.salvarChecklist();
                      if (!mounted ||
                          checklistController.erroProcessamento != null) {
                        return;
                      }

                      setState(() => _checklistAberto = false);
                      await showAppDialog(
                        context: context,
                        titulo: 'Checklist concluído',
                        mensagem:
                            'Checklist salvo com sucesso. Você já pode iniciar a rota.',
                        tipo: AppDialogType.sucesso,
                      );
                    },
              style: ElevatedButton.styleFrom(
                backgroundColor: theme.primaryColor,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(18),
                ),
              ),
              child: checklistController.salvando
                  ? const SizedBox(
                      width: 24,
                      height: 24,
                      child: CircularProgressIndicator(
                        strokeWidth: 2.4,
                        color: Colors.white,
                      ),
                    )
                  : const Text(
                      'Salvar checklist',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 17,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _secaoCollapseMotorista({
    required ThemeData theme,
    required String titulo,
    required String subtitulo,
    required IconData icone,
    required bool concluida,
    required bool aberta,
    required VoidCallback onToggle,
    required Widget child,
  }) {
    return _cartaoSimples(
      child: Column(
        children: [
          InkWell(
            borderRadius: BorderRadius.circular(18),
            onTap: onToggle,
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  CircleAvatar(
                    radius: 22,
                    backgroundColor: concluida
                        ? Colors.green.withValues(alpha: .12)
                        : theme.primaryColor.withValues(alpha: .10),
                    child: Icon(
                      concluida ? Icons.check : icone,
                      color: concluida ? Colors.green : theme.primaryColor,
                      size: 24,
                    ),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          titulo,
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                        const SizedBox(height: 3),
                        Text(
                          subtitulo,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(color: Colors.grey[700]),
                        ),
                      ],
                    ),
                  ),
                  Icon(
                    aberta
                        ? Icons.keyboard_arrow_up
                        : Icons.keyboard_arrow_down,
                    size: 30,
                    color: Colors.grey[700],
                  ),
                ],
              ),
            ),
          ),
          AnimatedCrossFade(
            firstChild: const SizedBox(width: double.infinity),
            secondChild: Padding(
              padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
              child: child,
            ),
            crossFadeState: aberta
                ? CrossFadeState.showSecond
                : CrossFadeState.showFirst,
            duration: const Duration(milliseconds: 180),
          ),
        ],
      ),
    );
  }

  Widget _cartaoSimples({required Widget child}) {
    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: .08),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: child,
    );
  }

  /*
  Widget _estadoAntesDeIniciarLegado(ThemeData theme) {
    final rota = checklistController.rotaSelecionada;
    final veiculo = checklistController.veiculoSelecionado;

    final temChecklistValido =
        rota != null &&
        veiculo != null &&
        checklistController.checklistSnapshots.containsKey(
          '${rota.id}_${veiculo.id}',
        );

    if (rota != null &&
        (rota.rotaFinalizada || rotasController.rotaFinalizadaId == rota.id)) {
      return _rotaFinalizadaCard(theme);
    }

    if (!temChecklistValido) {
      return _estadoSemChecklist(theme);
    }

    return Column(
      children: [
        _cardResumoRota(theme),
        const SizedBox(height: 12),
        _localizacaoInicial(theme),
        const SizedBox(height: 20),

        SizedBox(
          width: double.infinity,
          child: ElevatedButton(
            onPressed: rotasController.carregando
                ? null
                : () async {
                    if (rotasController.carregando) return;
                    print('[DEBUG] Botão Iniciar Rota clicado');

                    final confirmar = await showAppDialog(
                      context: context,
                      titulo: 'Iniciar rota',
                      mensagem:
                          'Deseja iniciar a rota agora?\nA localização inicial será registrada.',
                      tipo: AppDialogType.interrogacao,
                    );

                    if (confirmar == true) {
                      print('[DEBUG] Confirmação recebida');
                      try {
                        print('[DEBUG] Solicitando permissões...');
                        await solicitarPermissoes();
                        print('[DEBUG] Permissões solicitadas.');

                        print(
                          '[DEBUG] Chamando rotasController.iniciarRota...',
                        );
                        final RotaExecucaoDTO rotaExecucao =
                            await rotasController.iniciarRota(
                              rotaId: checklistController.rotaSelecionada!.id,
                              veiculoId:
                                  checklistController.veiculoSelecionado!.id,
                              checklistId: checklistController.ultimaExecucaoId,
                            );
                        print(
                          '[DEBUG] rotasController.iniciarRota concluído com sucesso: ID ${rotaExecucao.id}',
                        );

                        await FlutterForegroundTask.saveData(
                          key: 'rotaExecucaoId',
                          value: rotaExecucao.id.toString(),
                        );
                        await FlutterForegroundTask.saveData(
                          key: 'execucaoOffline',
                          value: rotaExecucao.execucaoOffline.toString(),
                        );
                        await FlutterForegroundTask.saveData(
                          key: 'localExecucaoId',
                          value: rotaExecucao.localExecucaoId ?? '',
                        );

                        await RotaTrackingService.start(
                          descricaoRota: rotaExecucao.descricao,
                          execucaoOffline: rotaExecucao.execucaoOffline,
                        );
                        print('[DEBUG] RotaTrackingService iniciado');
                        _atualizarChatNaoLidas();

                        if (context.mounted) {
                          await showAppDialog(
                            context: context,
                            titulo: 'Rota iniciada',
                            mensagem: 'A rota foi iniciada com sucesso.',
                            tipo: AppDialogType.sucesso,
                          );
                        }
                      } catch (e) {
                        print('[DEBUG] ERRO ao iniciar rota: $e');
                        if (context.mounted) {
                          await showAppDialog(
                            context: context,
                            titulo: 'Atenção',
                            mensagem: extrairMensagemErro(e),
                            tipo: AppDialogType.alerta,
                          );
                        }
                      }
                    } else {
                      print('[DEBUG] Início de rota cancelado pelo usuário');
                    }
                  },
            style: ElevatedButton.styleFrom(
              backgroundColor: theme.primaryColor,
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
              ),
            ),
            child: rotasController.carregando
                ? const SizedBox(
                    width: 22,
                    height: 22,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Colors.white,
                    ),
                  )
                : const Text(
                    'Iniciar rota',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: Colors.white,
                    ),
                  ),
          ),
        ),
      ],
    );
  }

  */

  // ignore: unused_element
  Widget _estadoSemChecklist(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: .25),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          Icon(Icons.assignment_late, size: 60, color: Colors.orange),
          const SizedBox(height: 16),
          const Text(
            'Checklist não realizado',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          const Text(
            'Para iniciar uma rota é necessário selecionar a rota, o veículo e salvar o checklist.',
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 20),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: () {
                setState(() {
                  _rotaAberta = true;
                  _veiculoAberto = checklistController.rotaSelecionada != null;
                  _checklistAberto =
                      checklistController.veiculoSelecionado != null;
                });
              },
              icon: const Icon(Icons.checklist),
              label: const Text('Ir para Checklist'),

              style: OutlinedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                backgroundColor: theme.primaryColor.withValues(alpha: .1),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _rotaFinalizadaCard(ThemeData theme) {
    final rota = checklistController.rotaSelecionada;

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: .2),
            blurRadius: 8,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        children: [
          const Icon(Icons.check_circle, size: 60, color: Colors.green),
          const SizedBox(height: 16),
          const Text(
            'Rota finalizada',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          const Text(
            'Esta rota já foi concluída hoje. Você pode consultar o trajeto executado, pontos tratados e mensagens registradas.',
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: rota == null || rotasController.carregando
                  ? null
                  : () async {
                      try {
                        await rotasController.carregarExecucaoFinalizadaDoDia(
                          rota.id,
                        );
                        _atualizarChatNaoLidas();
                      } catch (e) {
                        if (context.mounted) {
                          await showAppDialog(
                            context: context,
                            titulo: 'Atenção',
                            mensagem: extrairMensagemErro(e),
                            tipo: AppDialogType.alerta,
                          );
                        }
                      }
                    },
              icon: const Icon(Icons.map, color: Colors.white),
              label: const Text(
                'Consultar execução finalizada',
                style: TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                ),
              ),
              style: ElevatedButton.styleFrom(
                backgroundColor: theme.primaryColor,
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
              ),
            ),
          ),
          const SizedBox(height: 10),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: () {
                setState(() {
                  _rotaAberta = true;
                  _veiculoAberto = checklistController.rotaSelecionada != null;
                  _checklistAberto =
                      checklistController.veiculoSelecionado != null;
                });
              },
              icon: const Icon(Icons.checklist),
              label: const Text('Ir para Checklist'),

              style: OutlinedButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                backgroundColor: theme.primaryColor.withValues(alpha: .1),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _dialogoPausa() async {
    final TextEditingController motivoCmd = TextEditingController();
    final bool? result = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext context) {
        return AlertDialog(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          title: const Text(
            'Fazer Pausa',
            style: TextStyle(fontWeight: FontWeight.bold),
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('Isso irá parar a captura de sua localização.'),
              const SizedBox(height: 16),
              TextField(
                controller: motivoCmd,
                decoration: const InputDecoration(
                  labelText: 'Motivo da Pausa',
                  hintText: 'Ex: Almoço',
                  border: OutlineInputBorder(),
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(false),
              child: const Text(
                'Cancelar',
                style: TextStyle(color: Colors.grey),
              ),
            ),
            ElevatedButton(
              onPressed: () {
                if (motivoCmd.text.trim().isEmpty) return;
                Navigator.of(context).pop(true);
              },
              child: const Text('Confirmar Pausa'),
            ),
          ],
        );
      },
    );

    if (result == true) {
      await rotasController.iniciarPausa(motivoCmd.text.trim());
      if (context.mounted) {
        showAppDialog(
          context: context,
          titulo: 'Pausa Adicionada',
          mensagem: 'Rota pausada com sucesso. O rastreio foi suspenso.',
          tipo: AppDialogType.sucesso,
        );
      }
    }
  }

  Widget _rotaAtiva(ThemeData theme) {
    final emAndamento = rotasController.rotaAtual?.emAndamento == true;

    return Column(
      children: [
        if (!emAndamento) ...[
          _botaoSairVisualizacao(theme),
          const SizedBox(height: 12),
        ],
        _cardResumoRota(theme),
        const SizedBox(height: 12),
        _statusExecucao(theme),
        if (!emAndamento) ...[const SizedBox(height: 12), _mapaExecucao(theme)],
        const SizedBox(height: 12),
        _botaoRotaCompleta(theme),
        const SizedBox(height: 12),
        _botaoChatRota(theme),
        const SizedBox(height: 16),
        _timelineParadas(theme),

        if (emAndamento && rotasController.rotaAtual?.permitePausa == true) ...[
          const SizedBox(height: 24),
          Observer(
            builder: (_) {
              if (rotasController.estaPausada) {
                return SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: () async {
                      await rotasController.finalizarPausa();
                      if (context.mounted) {
                        showAppDialog(
                          context: context,
                          titulo: 'Pausa Finalizada',
                          mensagem:
                              'Rota retomada. Registro de localização ativo.',
                          tipo: AppDialogType.sucesso,
                        );
                      }
                    },
                    icon: const Icon(
                      Icons.play_circle_fill,
                      color: Colors.white,
                    ),
                    label: const Text(
                      'Finalizar Pausa (Retomar)',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        color: Colors.white,
                      ),
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green,
                      padding: const EdgeInsets.symmetric(vertical: 22),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(20),
                      ),
                      elevation: 4,
                    ),
                  ),
                );
              } else {
                final limiteOk =
                    rotasController.rotaAtual!.quantidadePausasRealizadas <
                    rotasController.rotaAtual!.quantidadePausas;
                return SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: limiteOk
                        ? () => _dialogoPausa()
                        : () => showAppDialog(
                            context: context,
                            titulo: 'Atenção',
                            mensagem:
                                'O limite de pausas desta rota já foi atingido!',
                            tipo: AppDialogType.alerta,
                          ),
                    icon: const Icon(
                      Icons.pause_circle_filled,
                      color: Colors.white,
                    ),
                    label: Text(
                      limiteOk ? 'Fazer Pausa' : 'Limite de Pausas Atingido',
                      style: const TextStyle(
                        fontWeight: FontWeight.bold,
                        color: Colors.white,
                      ),
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: limiteOk ? Colors.orange : Colors.grey,
                      padding: const EdgeInsets.symmetric(vertical: 22),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(20),
                      ),
                      elevation: 4,
                    ),
                  ),
                );
              }
            },
          ),
        ],

        if (!emAndamento && emAndamento) ...[
          const SizedBox(height: 24),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: () async {
                final confirmar = await showAppDialog(
                  context: context,
                  titulo: 'Encerrar rota',
                  mensagem: 'Deseja realmente encerrar a rota?',
                  tipo: AppDialogType.interrogacao,
                );

                if (confirmar == true) {
                  await rotasController.encerrarRota();
                  await RotaTrackingService.stop();
                  await _mostrarResumoFinalizacao();
                }
              },
              icon: const Icon(Icons.stop_circle, color: Colors.white),
              label: const Text(
                'Encerrar rota',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: Colors.white,
                ),
              ),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.red,
                padding: const EdgeInsets.symmetric(vertical: 18),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(20),
                ),
                elevation: 8,
              ),
            ),
          ),
        ],
      ],
    );
  }

  Widget _botaoSairVisualizacao(ThemeData theme) {
    return SizedBox(
      width: double.infinity,
      child: OutlinedButton.icon(
        onPressed: rotasController.carregando
            ? null
            : _sairDaVisualizacaoExecucao,
        icon: const Icon(Icons.arrow_back),
        label: const Text('Voltar para selecionar outra rota'),
        style: OutlinedButton.styleFrom(
          foregroundColor: theme.primaryColor,
          backgroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
        ),
      ),
    );
  }

  Future<void> _mostrarResumoFinalizacao() async {
    if (!mounted) return;

    final total = rotasController.paradas.length;
    final realizadas = rotasController.paradas
        .where((p) => p.entregue == true)
        .length;
    final naoRealizadas = rotasController.paradas
        .where((p) => p.entregue == false)
        .length;
    final tratadas = rotasController.paradas.where((p) => p.confirmada).length;
    final offline = rotasController.rotaAtual?.execucaoOffline == true;
    final offlineTexto = offline ? 'Sim' : 'Não';

    await showAppDialog(
      context: context,
      titulo: 'Rota finalizada',
      mensagem:
          'Resumo da execução:\n'
          'Pontos tratados: $tratadas de $total\n'
          'Pontos realizados: $realizadas\n'
          'Pontos não realizados: $naoRealizadas\n'
          'Execução offline: $offlineTexto\n\n'
          'O mapa, os pontos e o histórico do chat permanecem disponíveis para consulta nesta tela.',
      tipo: AppDialogType.sucesso,
    );
  }

  ParadaStore? get _proximaParada {
    for (final parada in rotasController.paradas) {
      if (!parada.confirmada) return parada;
    }
    return null;
  }

  String _statusParada(ParadaStore parada) {
    if (parada.entregue == true) return 'Concluída';
    if (parada.entregue == false) return 'Não realizada';
    return 'Pendente';
  }

  Color _corStatusParada(ParadaStore parada, ThemeData theme) {
    if (parada.entregue == true) return Colors.green;
    if (parada.entregue == false) return Colors.red;
    return Colors.orange;
  }

  Uri? _uriRotaCompleta() {
    final pontos = rotasController.paradas
        .where((p) => p.latitude != null && p.longitude != null)
        .map((p) => '${p.latitude},${p.longitude}')
        .toList();

    if (pontos.length < 2) return null;

    final origem = pontos.first;
    final destino = pontos.last;
    final intermediarios = pontos.length > 2
        ? pontos.sublist(1, pontos.length - 1).join('|')
        : null;

    return Uri.https('www.google.com', '/maps/dir/', {
      'api': '1',
      'origin': origem,
      'destination': destino,
      if (intermediarios != null && intermediarios.isNotEmpty)
        'waypoints': intermediarios,
      'travelmode': 'driving',
    });
  }

  Widget _statusExecucao(ThemeData theme) {
    final proxima = _proximaParada;
    final total = rotasController.paradas.length;
    final concluidas = rotasController.paradas
        .where((p) => p.confirmada)
        .length;
    final emAndamento = rotasController.rotaAtual?.emAndamento == true;

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: theme.primaryColor.withValues(alpha: .15)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.navigation, color: theme.primaryColor),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  emAndamento ? 'Rota em andamento' : 'Execução finalizada',
                  style: TextStyle(
                    color: theme.primaryColor,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 10,
                  vertical: 5,
                ),
                decoration: BoxDecoration(
                  color: (emAndamento ? Colors.green : Colors.blue).withValues(
                    alpha: .12,
                  ),
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Text(
                  emAndamento ? 'GPS ativo' : 'Histórico',
                  style: TextStyle(
                    color: emAndamento ? Colors.green : Colors.blue,
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 14),
          Text(
            proxima?.endereco ?? 'Todas as paradas foram tratadas.',
            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: 6),
          Text(
            proxima == null
                ? emAndamento
                      ? 'Confira e encerre a rota quando estiver pronto.'
                      : 'Todos os pontos registrados nesta execução.'
                : 'Próxima ação da viagem',
            style: TextStyle(color: Colors.grey[700]),
          ),
          const SizedBox(height: 12),
          ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: LinearProgressIndicator(
              value: total == 0 ? 0 : concluidas / total,
              minHeight: 8,
              backgroundColor: Colors.grey.withValues(alpha: .18),
              valueColor: AlwaysStoppedAnimation(theme.primaryColor),
            ),
          ),
          const SizedBox(height: 6),
          Text(
            '$concluidas de $total pontos tratados',
            style: TextStyle(color: Colors.grey[700], fontSize: 12),
          ),
        ],
      ),
    );
  }

  Widget _botaoRotaCompleta(ThemeData theme) {
    final uri = _uriRotaCompleta();

    return SizedBox(
      width: double.infinity,
      child: OutlinedButton.icon(
        onPressed: uri == null
            ? null
            : () async {
                await launchUrl(uri, mode: LaunchMode.externalApplication);
              },
        icon: const Icon(Icons.map),
        label: const Text('Abrir caminho completo no Google Maps'),
        style: OutlinedButton.styleFrom(
          foregroundColor: theme.primaryColor,
          backgroundColor: theme.primaryColor.withValues(alpha: .08),
          padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14),
          ),
        ),
      ),
    );
  }

  Widget _botaoChatRota(ThemeData theme) {
    final rota = rotasController.rotaAtual;

    return SizedBox(
      width: double.infinity,
      child: OutlinedButton.icon(
        onPressed: rota == null
            ? null
            : () async {
                if (rota.execucaoOffline ||
                    !await ConnectivityService.isConnected()) {
                  await showAppDialog(
                    context: context,
                    titulo: 'Chat indisponível offline',
                    mensagem:
                        'O chat não está disponível para funcionamento offline. As mensagens já sincronizadas ficam preservadas no histórico do servidor, mas é necessário estar conectado à internet para enviar, receber e atualizar mensagens da rota.',
                    tipo: AppDialogType.alerta,
                  );
                  return;
                }

                await Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => RotaChatPage(
                      rotaExecucaoId: rota.id,
                      rotaDescricao: rota.descricao,
                    ),
                  ),
                );
                _atualizarChatNaoLidas();
              },
        icon: const Icon(Icons.chat_bubble_outline),
        label: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Abrir chat da rota'),
            if (_chatNaoLidas > 0) ...[
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 2),
                decoration: BoxDecoration(
                  color: Colors.red,
                  borderRadius: BorderRadius.circular(999),
                ),
                child: Text(
                  _chatNaoLidas > 99 ? '99+' : _chatNaoLidas.toString(),
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 11,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ],
        ),
        style: OutlinedButton.styleFrom(
          foregroundColor: theme.primaryColor,
          backgroundColor: theme.primaryColor.withValues(alpha: .08),
          padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14),
          ),
        ),
      ),
    );
  }

  void _carregarTrajetoDaExecucaoSeNecessario() {
    final rota = rotasController.rotaAtual;
    if (rota == null || rota.execucaoOffline) return;
    if (rota.emAndamento) return;
    if (_trajetoExecucaoId == rota.id || _carregandoTrajeto) return;

    _trajetoExecucaoId = rota.id;
    _carregandoTrajeto = true;

    _rotasService
        .obterTrajetoExecucao(rota.id)
        .then((dados) {
          if (!mounted) return;
          setState(() {
            _trajetoPontos = dados
                .map(
                  (p) => LatLng(
                    double.parse(p['latitude'].toString()),
                    double.parse(p['longitude'].toString()),
                  ),
                )
                .toList();
          });
        })
        .catchError((_) {
          if (!mounted) return;
          setState(() => _trajetoPontos = []);
        })
        .whenComplete(() {
          if (!mounted) return;
          setState(() => _carregandoTrajeto = false);
        });
  }

  Widget _mapaExecucao(ThemeData theme) {
    final rota = rotasController.rotaAtual;
    if (rota == null || rota.execucaoOffline || rota.emAndamento) {
      return const SizedBox.shrink();
    }

    WidgetsBinding.instance.addPostFrameCallback((_) {
      _carregarTrajetoDaExecucaoSeNecessario();
    });

    return Container(
      width: double.infinity,
      height: 280,
      clipBehavior: Clip.antiAlias,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: theme.primaryColor.withValues(alpha: .15)),
      ),
      child: _carregandoTrajeto
          ? const Center(child: CircularProgressIndicator())
          : _trajetoPontos.isEmpty
          ? const Center(
              child: Text('Nenhum ponto de trajeto registrado para exibir.'),
            )
          : FlutterMap(
              options: MapOptions(
                initialCenter: _trajetoPontos.last,
                initialZoom: 14,
              ),
              children: [
                const RastreioTileLayer(),
                PolylineLayer(
                  polylines: [
                    Polyline(
                      points: _trajetoPontos,
                      color: theme.primaryColor,
                      strokeWidth: 4,
                    ),
                  ],
                ),
                MarkerLayer(markers: _marcadoresMapaExecucao(theme)),
              ],
            ),
    );
  }

  List<Marker> _marcadoresMapaExecucao(ThemeData theme) {
    final markers = <Marker>[
      Marker(
        point: _trajetoPontos.first,
        width: 38,
        height: 38,
        child: const Icon(
          Icons.play_circle_fill,
          color: Colors.green,
          size: 34,
        ),
      ),
      Marker(
        point: _trajetoPontos.last,
        width: 38,
        height: 38,
        child: const Icon(Icons.flag_circle, color: Colors.red, size: 34),
      ),
    ];

    for (final parada in rotasController.paradas) {
      final lat = parada.latitude;
      final lng = parada.longitude;
      if (lat == null || lng == null) continue;

      markers.add(
        Marker(
          point: LatLng(lat, lng),
          width: 34,
          height: 34,
          child: Icon(
            parada.confirmada ? Icons.check_circle : Icons.place,
            color: parada.confirmada ? Colors.green : theme.primaryColor,
            size: 28,
          ),
        ),
      );
    }

    return markers;
  }

  Widget _cardResumoRota(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(color: Colors.black.withValues(alpha: .15), blurRadius: 8),
        ],
      ),
      child: Column(
        children: [
          Row(
            children: [
              Icon(Icons.alt_route, color: theme.primaryColor),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  checklistController.rotaSelecionada?.descricao ?? '-',
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 15,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Divider(color: Colors.grey.withValues(alpha: .3)),
          const SizedBox(height: 10),
          Row(
            children: [
              Icon(Icons.directions_car, color: theme.primaryColor),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  '${checklistController.veiculoSelecionado?.nome ?? ''} • ${checklistController.veiculoSelecionado?.placa ?? ''}',
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _timelineParadas(ThemeData theme) {
    return Observer(
      builder: (_) {
        if (rotasController.paradas.isEmpty) {
          return Container(
            width: double.infinity,
            padding: const EdgeInsets.all(18),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
            ),
            child: const Text('Nenhum ponto cadastrado para esta rota.'),
          );
        }

        return Container(
          width: double.infinity,
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withValues(alpha: .08),
                blurRadius: 10,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Icon(Icons.route, color: theme.primaryColor),
                  const SizedBox(width: 8),
                  const Expanded(
                    child: Text(
                      'Pontos da rota',
                      style: TextStyle(fontWeight: FontWeight.bold),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              for (var i = 0; i < rotasController.paradas.length; i++)
                _paradaTimelineItem(
                  theme,
                  rotasController.paradas[i],
                  i,
                  i == rotasController.paradas.length - 1,
                ),
            ],
          ),
        );
      },
    );
  }

  Widget _paradaTimelineItem(
    ThemeData theme,
    ParadaStore parada,
    int index,
    bool ultima,
  ) {
    return Observer(
      builder: (_) {
        final emAndamento = rotasController.rotaAtual?.emAndamento == true;
        final ativa = emAndamento && _proximaParada?.id == parada.id;
        final corStatus = _corStatusParada(parada, theme);

        return IntrinsicHeight(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Column(
                children: [
                  Container(
                    width: 30,
                    height: 30,
                    decoration: BoxDecoration(
                      color: ativa
                          ? theme.primaryColor
                          : corStatus.withValues(alpha: .14),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      parada.confirmada
                          ? Icons.check
                          : ativa
                          ? Icons.navigation
                          : Icons.more_horiz,
                      size: 18,
                      color: ativa ? Colors.white : corStatus,
                    ),
                  ),
                  if (!ultima)
                    Expanded(
                      child: Container(
                        width: 2,
                        margin: const EdgeInsets.symmetric(vertical: 4),
                        color: Colors.grey.withValues(alpha: .25),
                      ),
                    ),
                ],
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Container(
                  margin: EdgeInsets.only(bottom: ultima ? 0 : 12),
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: ativa
                        ? theme.primaryColor.withValues(alpha: .08)
                        : Colors.grey.withValues(alpha: .04),
                    borderRadius: BorderRadius.circular(14),
                    border: Border.all(
                      color: ativa
                          ? theme.primaryColor.withValues(alpha: .35)
                          : Colors.grey.withValues(alpha: .18),
                    ),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Expanded(
                            child: Text(
                              parada.endereco,
                              style: const TextStyle(
                                fontWeight: FontWeight.w700,
                                fontSize: 14,
                              ),
                            ),
                          ),
                          const SizedBox(width: 8),
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 8,
                              vertical: 4,
                            ),
                            decoration: BoxDecoration(
                              color: corStatus.withValues(alpha: .12),
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: Text(
                              _statusParada(parada),
                              style: TextStyle(
                                color: corStatus,
                                fontSize: 11,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ),
                        ],
                      ),
                      if ((parada.observacaoCadastro ?? '').isNotEmpty) ...[
                        const SizedBox(height: 10),
                        Container(
                          width: double.infinity,
                          padding: const EdgeInsets.all(10),
                          decoration: BoxDecoration(
                            color: Colors.amber.withValues(alpha: .14),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Icon(
                                Icons.info_outline,
                                color: Colors.orange,
                                size: 18,
                              ),
                              const SizedBox(width: 8),
                              Expanded(
                                child: Text(
                                  parada.observacaoCadastro!,
                                  style: const TextStyle(fontSize: 13),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                      const SizedBox(height: 10),
                      if (parada.latitude != null && parada.longitude != null)
                        Align(
                          alignment: Alignment.centerLeft,
                          child: TextButton.icon(
                            onPressed: () async {
                              final url =
                                  'https://www.google.com/maps/search/?api=1&query=${parada.latitude},${parada.longitude}';

                              await launchUrl(
                                Uri.parse(url),
                                mode: LaunchMode.externalApplication,
                              );
                            },
                            icon: const Icon(Icons.place, size: 18),
                            label: const Text('Abrir ponto no Maps'),
                            style: TextButton.styleFrom(
                              foregroundColor: theme.primaryColor,
                              padding: EdgeInsets.zero,
                            ),
                          ),
                        ),
                      const SizedBox(height: 8),
                      Text(
                        'Este ponto foi realizado?',
                        style: TextStyle(
                          color: Colors.grey[800],
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          Expanded(
                            child: _botaoEntregaParada(
                              theme,
                              parada,
                              valor: true,
                              texto: 'SIM',
                              icone: Icons.check_circle,
                              cor: Colors.green,
                              habilitado: !parada.confirmada && emAndamento,
                            ),
                          ),
                          const SizedBox(width: 10),
                          Expanded(
                            child: _botaoEntregaParada(
                              theme,
                              parada,
                              valor: false,
                              texto: 'NÃO',
                              icone: Icons.cancel,
                              cor: Colors.red,
                              habilitado: !parada.confirmada && emAndamento,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      TextField(
                        controller: parada.controllerObservacao,
                        minLines: 2,
                        maxLines: 3,
                        enabled: !parada.confirmada && emAndamento,
                        decoration: InputDecoration(
                          labelText: 'Observação da execução',
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(16),
                          ),
                          contentPadding: const EdgeInsets.all(16),
                        ),
                      ),
                      if (emAndamento) ...[
                        const SizedBox(height: 12),
                        SizedBox(
                          width: double.infinity,
                          child: ElevatedButton(
                            onPressed: parada.salvando
                                ? null
                                : () async {
                                    if (parada.entregue == null) {
                                      await showAppDialog(
                                        context: context,
                                        titulo: 'Atencao',
                                        mensagem:
                                            'Selecione se a parada foi realizada.',
                                      );
                                      return;
                                    }

                                    await rotasController.confirmarParada(
                                      parada,
                                    );

                                    if (context.mounted) {
                                      await showAppDialog(
                                        context: context,
                                        titulo: 'Ponto da Rota',
                                        mensagem: 'Registro feito com sucesso.',
                                        tipo: AppDialogType.sucesso,
                                      );
                                    }
                                  },
                            style: ElevatedButton.styleFrom(
                              backgroundColor: parada.confirmada
                                  ? Colors.green
                                  : theme.primaryColor,
                              padding: const EdgeInsets.symmetric(vertical: 13),
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(18),
                              ),
                            ),
                            child: parada.salvando
                                ? const SizedBox(
                                    width: 18,
                                    height: 18,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                      color: Colors.white,
                                    ),
                                  )
                                : Text(
                                    parada.confirmada
                                        ? 'Confirmada'
                                        : ativa
                                        ? 'Registrar este ponto'
                                        : 'Salvar ponto',
                                    style: const TextStyle(
                                      fontWeight: FontWeight.bold,
                                      color: Colors.white,
                                    ),
                                  ),
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _botaoEntregaParada(
    ThemeData theme,
    ParadaStore parada, {
    required bool valor,
    required String texto,
    required IconData icone,
    required Color cor,
    required bool habilitado,
  }) {
    final selecionado = parada.entregue == valor;

    return SizedBox(
      height: 58,
      child: OutlinedButton.icon(
        onPressed: habilitado
            ? () => rotasController.atualizarEntrega(parada, valor)
            : null,
        icon: Icon(icone, size: 26),
        label: Text(
          texto,
          style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w800),
        ),
        style: OutlinedButton.styleFrom(
          foregroundColor: selecionado ? Colors.white : cor,
          backgroundColor: selecionado ? cor : cor.withValues(alpha: .08),
          disabledForegroundColor: selecionado ? Colors.white : Colors.grey,
          disabledBackgroundColor: selecionado
              ? cor.withValues(alpha: .70)
              : Colors.grey.withValues(alpha: .08),
          side: BorderSide(
            color: selecionado ? cor : cor.withValues(alpha: .45),
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(18),
          ),
        ),
      ),
    );
  }

  // ignore: unused_element
  Widget _cardParadas(ThemeData theme) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: .12),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(24),
        child: Observer(
          builder: (_) => ExpansionPanelList.radio(
            elevation: 0,
            expandedHeaderPadding: EdgeInsets.zero,
            materialGapSize: 0,
            dividerColor: Colors.transparent,
            expandIconColor: Colors.white,
            animationDuration: const Duration(milliseconds: 200),
            children: [
              for (final parada in rotasController.paradas)
                ExpansionPanelRadio(
                  value: parada.id,
                  canTapOnHeader: true,
                  backgroundColor: theme.primaryColor,

                  headerBuilder: (context, isExpanded) {
                    return Observer(
                      builder: (_) {
                        final corHeader = theme.primaryColor;

                        final corStatus = parada.entregue == null
                            ? Colors.grey
                            : parada.entregue == true
                            ? Colors.green
                            : Colors.red;

                        final iconeStatus = parada.entregue == null
                            ? Icons.radio_button_unchecked
                            : parada.entregue == true
                            ? Icons.check_circle
                            : Icons.cancel;

                        return Container(
                          width: double.infinity,
                          color: corHeader,
                          child: ListTile(
                            contentPadding: const EdgeInsets.symmetric(
                              horizontal: 16,
                            ),
                            leading: Container(
                              decoration: const BoxDecoration(
                                color: Colors.white,
                                shape: BoxShape.circle,
                              ),
                              child: Icon(iconeStatus, color: corStatus),
                            ),
                            title: Text(
                              parada.endereco,
                              style: const TextStyle(
                                fontWeight: FontWeight.w600,
                                color: Colors.white,
                              ),
                            ),
                            subtitle: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                if (parada.observacao.isNotEmpty)
                                  Padding(
                                    padding: const EdgeInsets.only(
                                      top: 4.0,
                                      bottom: 2.0,
                                    ),
                                    child: Text(
                                      'Obs: ${parada.observacao}',
                                      style: TextStyle(
                                        color: Colors.yellow[300],
                                        fontSize: 13,
                                      ),
                                    ),
                                  ),
                                if (!isExpanded)
                                  const Text(
                                    'Toque para abrir detalhes e confirmar',
                                    style: TextStyle(
                                      color: Colors.white70,
                                      fontSize: 12,
                                    ),
                                  ),
                              ],
                            ),
                          ),
                        );
                      },
                    );
                  },

                  body: Container(
                    width: double.infinity,
                    color: Colors.white,
                    padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        if (parada.latitude != null &&
                            parada.longitude != null) ...[
                          const SizedBox(height: 8),

                          SizedBox(
                            width: double.infinity,
                            child: OutlinedButton.icon(
                              onPressed: () async {
                                final url =
                                    'https://www.google.com/maps/search/?api=1&query=${parada.latitude},${parada.longitude}';

                                await launchUrl(
                                  Uri.parse(url),
                                  mode: LaunchMode.externalApplication,
                                );
                              },
                              icon: const Icon(Icons.map),
                              label: const Text('Abrir no Google Maps'),
                              style: OutlinedButton.styleFrom(
                                foregroundColor: theme.primaryColor,
                                backgroundColor: theme.primaryColor.withValues(
                                  alpha: .2,
                                ),
                                side: BorderSide(
                                  color: theme.primaryColor.withOpacity(.4),
                                ),
                                padding: const EdgeInsets.symmetric(
                                  vertical: 14,
                                ),
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(14),
                                ),
                              ),
                            ),
                          ),
                        ],

                        if (parada.link != null && parada.link!.isNotEmpty)
                          Column(
                            children: [
                              const SizedBox(height: 8),
                              SizedBox(
                                width: double.infinity,
                                child: OutlinedButton.icon(
                                  onPressed: () async {
                                    await launchUrl(
                                      Uri.parse(parada.link!),
                                      mode: LaunchMode.externalApplication,
                                    );
                                  },

                                  icon: const Icon(Icons.link),
                                  label: const Text('Abrir link da rota'),
                                  style: OutlinedButton.styleFrom(
                                    foregroundColor: theme.primaryColor,
                                    backgroundColor: theme.primaryColor
                                        .withValues(alpha: .2),
                                    side: BorderSide(
                                      color: theme.primaryColor.withOpacity(.4),
                                    ),
                                    padding: const EdgeInsets.symmetric(
                                      vertical: 14,
                                    ),
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(14),
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),

                        const SizedBox(height: 12),
                        Observer(
                          builder: (_) => DropdownButtonFormField<bool?>(
                            value: parada.entregue,

                            decoration: InputDecoration(
                              labelText: 'Parada realizada?',
                              floatingLabelBehavior:
                                  FloatingLabelBehavior.always,

                              labelStyle: TextStyle(
                                color: parada.confirmada
                                    ? theme.primaryColor.withValues(alpha: .8)
                                    : theme.primaryColor,
                              ),
                              focusedBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(14),
                                borderSide: BorderSide(
                                  color: theme.primaryColor,
                                  width: 2,
                                ),
                              ),
                              border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(14),
                              ),
                              enabledBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(14),
                                borderSide: BorderSide(
                                  color: theme.primaryColor.withValues(
                                    alpha: .5,
                                  ),
                                ),
                              ),
                              disabledBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(14),
                                borderSide: BorderSide(
                                  color: theme.primaryColor.withValues(
                                    alpha: .8,
                                  ),
                                  width: 1.5,
                                ),
                              ),
                            ),
                            style: TextStyle(
                              color: parada.confirmada
                                  ? Colors.black87
                                  : Colors.black,
                              fontWeight: parada.confirmada
                                  ? FontWeight.w600
                                  : FontWeight.normal,
                            ),
                            items: const [
                              DropdownMenuItem(value: true, child: Text('Sim')),
                              DropdownMenuItem(
                                value: false,
                                child: Text('Não'),
                              ),
                            ],
                            onChanged: parada.confirmada
                                ? null
                                : (value) {
                                    rotasController.atualizarEntrega(
                                      parada,
                                      value,
                                    );
                                  },
                          ),
                        ),

                        const SizedBox(height: 12),

                        Observer(
                          builder: (_) => TextField(
                            controller: parada.controllerObservacao,
                            minLines: 2,
                            maxLines: 3,
                            enabled: !parada.confirmada,
                            keyboardType: TextInputType.text,
                            decoration: InputDecoration(
                              labelText: 'Observação',
                              floatingLabelBehavior:
                                  FloatingLabelBehavior.always,

                              labelStyle: TextStyle(color: theme.primaryColor),
                              focusedBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(14),
                                borderSide: BorderSide(
                                  color: theme.primaryColor,
                                  width: 2,
                                ),
                              ),
                              border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(14),
                              ),
                              disabledBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(14),
                                borderSide: BorderSide(
                                  color: theme.primaryColor.withValues(
                                    alpha: .5,
                                  ),
                                ),
                              ),
                            ),
                          ),
                        ),
                        const SizedBox(height: 16),

                        Observer(
                          builder: (_) => SizedBox(
                            width: double.infinity,
                            child: ElevatedButton(
                              onPressed: parada.salvando
                                  ? null
                                  : () async {
                                      if (parada.entregue == null) {
                                        await showAppDialog(
                                          context: context,
                                          titulo: 'Atenção',
                                          mensagem:
                                              'Selecione se a parada foi realizada.',
                                        );
                                        return;
                                      }

                                      await rotasController.confirmarParada(
                                        parada,
                                      );

                                      if (context.mounted) {
                                        await showAppDialog(
                                          context: context,
                                          titulo: 'Ponto da Rota',
                                          mensagem:
                                              'Registro feito com sucesso.',
                                          tipo: AppDialogType.sucesso,
                                        );
                                      }
                                    },
                              style: ElevatedButton.styleFrom(
                                backgroundColor: parada.confirmada
                                    ? Colors.green
                                    : theme.primaryColor,
                                padding: const EdgeInsets.symmetric(
                                  vertical: 14,
                                ),
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(14),
                                ),
                              ),
                              child: parada.salvando
                                  ? const SizedBox(
                                      width: 18,
                                      height: 18,
                                      child: CircularProgressIndicator(
                                        strokeWidth: 2,
                                        color: Colors.white,
                                      ),
                                    )
                                  : Text(
                                      parada.confirmada
                                          ? 'Confirmada'
                                          : 'Salvar',
                                      style: const TextStyle(
                                        fontWeight: FontWeight.bold,
                                        color: Colors.white,
                                      ),
                                    ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _localizacaoInicial(ThemeData theme) {
    return Container(
      padding: const EdgeInsets.all(14),
      width: double.infinity,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: Colors.black.withValues(alpha: .2), blurRadius: 6),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Localização inicial',
            style: TextStyle(fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 8),
          Text(
            'A localização será capturada ao iniciar a rota.',
            style: TextStyle(color: Colors.grey[700]),
          ),
        ],
      ),
    );
  }

  Future<void> solicitarPermissoes() async {
    await Permission.notification.request();

    if (!await Permission.location.isGranted) {
      await Permission.location.request();
    }

    if (!await Permission.locationAlways.isGranted) {
      await Permission.locationAlways.request();
    }
  }
}

class _ManifestoPresenca {
  final List<PresencaPacienteRotaDTO> pacientes;
  final List<PresencaProfissionalRotaDTO> profissionais;

  const _ManifestoPresenca({
    required this.pacientes,
    required this.profissionais,
  });
}

class _PacientePresencaItem {
  final int? pacienteId;
  final String nome;
  final String? cpf;
  final String? telefone;
  bool presente;
  bool possuiAcompanhante;
  bool acompanhantePresente;
  final String? acompanhanteNome;
  final String? acompanhanteCPF;
  final bool novoPaciente;
  final bool incluirNaRota;

  _PacientePresencaItem({
    this.pacienteId,
    required this.nome,
    this.cpf,
    this.telefone,
    this.presente = true,
    this.possuiAcompanhante = false,
    this.acompanhantePresente = false,
    this.acompanhanteNome,
    this.acompanhanteCPF,
    this.novoPaciente = false,
    this.incluirNaRota = true,
  });

  PresencaPacienteRotaDTO toPresenca() {
    return PresencaPacienteRotaDTO(
      pacienteId: pacienteId,
      nome: nome,
      cpf: cpf,
      telefone: telefone,
      presente: presente,
      possuiAcompanhante: possuiAcompanhante,
      acompanhantePresente: acompanhantePresente,
      acompanhanteNome: acompanhanteNome,
      acompanhanteCPF: acompanhanteCPF,
      novoPaciente: novoPaciente,
      incluirNaRota: incluirNaRota,
    );
  }
}

class _TextoVazioPresenca extends StatelessWidget {
  final String texto;

  const _TextoVazioPresenca(this.texto);

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Text(texto, style: const TextStyle(color: Colors.black54)),
    );
  }
}
