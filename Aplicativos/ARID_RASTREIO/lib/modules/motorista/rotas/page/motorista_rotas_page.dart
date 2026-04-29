import 'package:arid_rastreio/core/service/rota_tracking_service.dart';
import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/menu/controller/motorista_menu_controller.dart';
import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_execucao_dto.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_foreground_task/flutter_foreground_task.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
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

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Observer(
      builder: (_) {
        return SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              _header(theme),

              const SizedBox(height: 16),

              rotasController.rotaIniciada
                  ? _rotaAtiva(theme)
                  : _estadoAntesDeIniciar(theme),
            ],
          ),
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

  Widget _estadoAntesDeIniciar(ThemeData theme) {
    final rota = checklistController.rotaSelecionada;
    final veiculo = checklistController.veiculoSelecionado;

    final temChecklistValido =
        rota != null &&
        veiculo != null &&
        checklistController.checklistSnapshots.containsKey(
          '${rota.id}_${veiculo.id}',
        );

    if (rota != null && rotasController.rotaFinalizadaId == rota.id) {
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

                        print('[DEBUG] Chamando rotasController.iniciarRota...');
                        final RotaExecucaoDTO rotaExecucao = await rotasController
                            .iniciarRota(
                              rotaId: checklistController.rotaSelecionada!.id,
                              veiculoId: checklistController.veiculoSelecionado!.id,
                              checklistId: checklistController.ultimaExecucaoId,
                            );
                        print('[DEBUG] rotasController.iniciarRota concluído com sucesso: ID ${rotaExecucao.id}');

                        await FlutterForegroundTask.saveData(
                          key: 'rotaExecucaoId',
                          value: rotaExecucao.id.toString(),
                        );

                        await RotaTrackingService.start();
                        print('[DEBUG] RotaTrackingService iniciado');

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
                locator<MotoristaMenuController>().mudarIndex(0);
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
            'Esta rota já foi concluída.',
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: () {
                locator<MotoristaMenuController>().mudarIndex(0);
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
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          title: const Text('Fazer Pausa', style: TextStyle(fontWeight: FontWeight.bold)),
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
              child: const Text('Cancelar', style: TextStyle(color: Colors.grey)),
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
    return Column(
      children: [
        _cardResumoRota(theme),
        const SizedBox(height: 20),
        _cardParadas(theme),
        
        if (rotasController.rotaAtual?.permitePausa == true) ...[
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
                          mensagem: 'Rota retomada. Registro de localização ativo.',
                          tipo: AppDialogType.sucesso,
                        );
                      }
                    },
                    icon: const Icon(Icons.play_circle_fill, color: Colors.white),
                    label: const Text('Finalizar Pausa (Retomar)', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.white)),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green,
                      padding: const EdgeInsets.symmetric(vertical: 22),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
                      elevation: 4,
                    ),
                  ),
                );
              } else {
                final limiteOk = rotasController.rotaAtual!.quantidadePausasRealizadas < rotasController.rotaAtual!.quantidadePausas;
                return SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: limiteOk 
                        ? () => _dialogoPausa() 
                        : () => showAppDialog(
                              context: context, 
                              titulo: 'Atenção', 
                              mensagem: 'O limite de pausas desta rota já foi atingido!', 
                              tipo: AppDialogType.alerta,
                            ),
                    icon: const Icon(Icons.pause_circle_filled, color: Colors.white),
                    label: Text(
                      limiteOk ? 'Fazer Pausa' : 'Limite de Pausas Atingido', 
                      style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.white)
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: limiteOk ? Colors.orange : Colors.grey,
                      padding: const EdgeInsets.symmetric(vertical: 22),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
                      elevation: 4,
                    ),
                  ),
                );
              }
            },
          ),
        ],

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
    );
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
                                    padding: const EdgeInsets.only(top: 4.0, bottom: 2.0),
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

