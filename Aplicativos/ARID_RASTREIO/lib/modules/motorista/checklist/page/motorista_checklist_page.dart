import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:mobx/mobx.dart';
import '../controller/checklist_controller.dart';
import 'package:arid_rastreio/shared/functions/functions.dart';

class MotoristaChecklistPage extends StatefulWidget {
  const MotoristaChecklistPage({super.key});

  @override
  State<MotoristaChecklistPage> createState() => _MotoristaChecklistPageState();
}

class _MotoristaChecklistPageState extends State<MotoristaChecklistPage> {
  final controller = locator<ChecklistController>();
  final rotasController = locator<MotoristaRotasController>();

  @override
  void initState() {
    super.initState();
    controller.carregarRotas();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Observer(
      builder: (_) {
        if (controller.erroProcessamento != null) {
          WidgetsBinding.instance.addPostFrameCallback((_) async {
            await showAppDialog(
              context: context,
              titulo: 'Atenção',
              mensagem: controller.erroProcessamento!,
              tipo: AppDialogType.alerta,
            );

            controller.erroProcessamento = null;
          });
        }

        if (controller.rotasFuture == null ||
            controller.rotasFuture!.status == FutureStatus.pending) {
          return const Center(child: CircularProgressIndicator());
        }

        final rotas = controller.rotasFuture!.value ?? [];

        return SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              _header(theme),
              const SizedBox(height: 20),
              campoSelecao(
                obrigatorio: true,
                theme: theme,
                titulo: 'Rota',
                valor: controller.rotaSelecionada?.nome,
                icone: Icons.alt_route,
                onTap: rotasController.rotaIniciada
                    ? null
                    : () => abrirSheet(
                        context,
                        theme,
                        titulo: 'Selecione a rota',
                        itens: rotas.map((e) => e.descricao).toList(),
                        selecionadoAtual: controller.rotaSelecionada?.descricao,
                        onSelecionar: (rSelecionada) {
                          final rota = rotas.firstWhere(
                            (r) => r.descricao == rSelecionada,
                          );
                          controller.selecionarRota(rota);
                        },
                      ),
              ),
              const SizedBox(height: 16),
              if (controller.rotaSelecionada != null)
                Observer(
                  builder: (_) {
                    if (controller.veiculosFuture == null ||
                        controller.veiculosFuture!.status ==
                            FutureStatus.pending) {
                      return const Center(child: CircularProgressIndicator());
                    }

                    final veiculos = controller.veiculosFuture!.value ?? [];

                    return campoSelecao(
                      obrigatorio: true,
                      theme: theme,
                      titulo: 'Veículo',
                      valor: controller.veiculoSelecionado?.nome,
                      icone: Icons.directions_car,
                      onTap: rotasController.rotaIniciada
                          ? null
                          : () => abrirSheet(
                              context,
                              theme,
                              titulo: 'Selecione o veículo',
                              itens: veiculos
                                  .map((v) => '${v.nome} • ${v.placa}')
                                  .toList(),
                              selecionadoAtual:
                                  controller.veiculoSelecionado?.nome != null &&
                                      controller.veiculoSelecionado?.placa !=
                                          null
                                  ? '${controller.veiculoSelecionado!.nome} • ${controller.veiculoSelecionado!.placa}'
                                  : null,
                              onSelecionar: (rSelecionada) {
                                final vSelecionado = veiculos.firstWhere(
                                  (v) =>
                                      '${v.nome} • ${v.placa}' == rSelecionada,
                                );
                                controller.selecionarVeiculo(vSelecionado);
                              },
                            ),
                    );
                  },
                ),
              if (controller.veiculoSelecionado != null)
                Container(
                  margin: const EdgeInsets.only(top: 16),
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(16),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: .06),
                        blurRadius: 6,
                      ),
                    ],
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'Progresso do checklist',
                        style: TextStyle(fontWeight: FontWeight.w600),
                      ),
                      const SizedBox(height: 8),
                      ClipRRect(
                        borderRadius: BorderRadius.circular(10),
                        child: Observer(
                          builder: (_) => LinearProgressIndicator(
                            value: controller.progressoChecklist,
                            minHeight: 8,
                            backgroundColor: Colors.grey.withValues(alpha: 0.2),
                            valueColor: AlwaysStoppedAnimation(
                              theme.primaryColor,
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 6),
                      Observer(
                        builder: (_) => Text(
                          '${controller.itensMarcados} de ${controller.totalItens} itens verificados',
                          style: TextStyle(color: Colors.grey[600]),
                        ),
                      ),
                    ],
                  ),
                ),
              if (controller.veiculoSelecionado != null)
                Container(
                  padding: const EdgeInsets.symmetric(vertical: 20),
                  child: Row(
                    children: [
                      const Expanded(child: Divider(thickness: 1)),
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 12),
                        child: Text(
                          'Itens do checklist',
                          style: const TextStyle(fontWeight: FontWeight.w600),
                        ),
                      ),
                      const Expanded(child: Divider(thickness: 1)),
                    ],
                  ),
                ),
              if (controller.veiculoSelecionado != null)
                ...controller.veiculoSelecionado!.checklist.map((item) {
                  return Observer(
                    builder: (_) {
                      final marcado = item.checked;
                      return GestureDetector(
                        onTap: rotasController.rotaIniciada
                            ? null
                            : () => controller.alternarCheck(item),
                        child: Container(
                          margin: const EdgeInsets.only(bottom: 12),
                          padding: const EdgeInsets.all(14),
                          decoration: BoxDecoration(
                            color: marcado
                                ? theme.primaryColor.withValues(alpha: .12)
                                : Colors.white,
                            borderRadius: BorderRadius.circular(16),
                            border: Border.all(
                              color: marcado
                                  ? theme.primaryColor
                                  : Colors.grey.withValues(alpha: .3),
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
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: Text(
                                  item.descricao,
                                  style: TextStyle(
                                    decoration: marcado
                                        ? TextDecoration.lineThrough
                                        : null,
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
              if (controller.veiculoSelecionado != null &&
                  !rotasController.rotaIniciada &&
                  !controller.rotaSelecionada!.rotaFinalizada)
                Padding(
                  padding: const EdgeInsets.only(top: 24, bottom: 32),
                  child: SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: controller.salvando
                          ? null
                          : () async {
                              await controller.salvarChecklist();

                              if (context.mounted &&
                                  controller.erroProcessamento == null) {
                                await showAppDialog(
                                  context: context,
                                  titulo: 'Checklist concluído',
                                  mensagem:
                                      'Checklist salvo com sucesso. Você já pode iniciar a rota.',
                                  tipo: AppDialogType.sucesso,
                                );
                              }
                            },
                      style: ElevatedButton.styleFrom(
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16),
                        ),
                        backgroundColor: theme.primaryColor,
                      ),
                      child: controller.salvando
                          ? const SizedBox(
                              height: 22,
                              width: 22,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                color: Colors.white,
                              ),
                            )
                          : const Text(
                              'Salvar checklist',
                              style: TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.bold,
                                color: Colors.white,
                              ),
                            ),
                    ),
                  ),
                ),
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
              child: const Icon(Icons.checklist, color: Colors.white),
            ),
            const SizedBox(width: 12),
            const Expanded(
              child: Text(
                'Checklist do Veículo',
                style: TextStyle(fontWeight: FontWeight.bold),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

