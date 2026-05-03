import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:mobx/mobx.dart';
import '../controller/checklist_controller.dart';

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
              const SizedBox(height: 12),
              _etapasPreparacao(theme),
              const SizedBox(height: 16),
              _listaRotasInline(theme, rotas),
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

                    return _listaVeiculosInline(theme, veiculos);
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

  Widget _etapasPreparacao(ThemeData theme) {
    final rotaOk = controller.rotaSelecionada != null;
    final veiculoOk = controller.veiculoSelecionado != null;
    final checklistOk = controller.checklistSalvo;

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: theme.primaryColor.withValues(alpha: .12)),
      ),
      child: Row(
        children: [
          _etapaChip(theme, 'Rota', rotaOk, true),
          _linhaEtapa(theme, rotaOk),
          _etapaChip(theme, 'Veiculo', veiculoOk, rotaOk),
          _linhaEtapa(theme, veiculoOk),
          _etapaChip(theme, 'Checklist', checklistOk, veiculoOk),
        ],
      ),
    );
  }

  Widget _linhaEtapa(ThemeData theme, bool ativa) {
    return Expanded(
      child: Container(
        height: 2,
        margin: const EdgeInsets.symmetric(horizontal: 6),
        color: ativa ? theme.primaryColor : Colors.grey.withValues(alpha: .25),
      ),
    );
  }

  Widget _etapaChip(ThemeData theme, String texto, bool concluida, bool ativa) {
    final color = concluida
        ? Colors.green
        : ativa
        ? theme.primaryColor
        : Colors.grey;

    return Column(
      children: [
        Container(
          width: 30,
          height: 30,
          decoration: BoxDecoration(
            color: color.withValues(alpha: concluida ? .16 : .10),
            shape: BoxShape.circle,
          ),
          child: Icon(
            concluida ? Icons.check : Icons.circle,
            color: color,
            size: concluida ? 18 : 10,
          ),
        ),
        const SizedBox(height: 4),
        Text(texto, style: TextStyle(color: color, fontSize: 11)),
      ],
    );
  }

  Widget _listaRotasInline(ThemeData theme, List<RotaChecklistDTO> rotas) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: Colors.black.withValues(alpha: .06), blurRadius: 8),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.alt_route, color: theme.primaryColor),
              const SizedBox(width: 8),
              const Expanded(
                child: Text(
                  'Escolha a rota',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          for (final rota in rotas)
            Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: InkWell(
                borderRadius: BorderRadius.circular(12),
                onTap: rotasController.rotaIniciada
                    ? null
                    : () => controller.selecionarRota(rota),
                child: Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: controller.rotaSelecionada?.id == rota.id
                        ? theme.primaryColor.withValues(alpha: .10)
                        : Colors.grey.withValues(alpha: .05),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(
                      color: controller.rotaSelecionada?.id == rota.id
                          ? theme.primaryColor
                          : Colors.grey.withValues(alpha: .18),
                    ),
                  ),
                  child: Row(
                    children: [
                      Expanded(
                        child: Text(
                          rota.descricao,
                          style: const TextStyle(fontWeight: FontWeight.w600),
                        ),
                      ),
                      if (controller.rotaSelecionada?.id == rota.id)
                        Icon(Icons.check_circle, color: theme.primaryColor),
                    ],
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _listaVeiculosInline(
    ThemeData theme,
    List<VeiculoChecklistDTO> veiculos,
  ) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.directions_car, color: theme.primaryColor),
              const SizedBox(width: 8),
              const Expanded(
                child: Text(
                  'Escolha o veiculo',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          for (final veiculo in veiculos)
            Padding(
              padding: const EdgeInsets.only(bottom: 8),
              child: OutlinedButton(
                onPressed: rotasController.rotaIniciada
                    ? null
                    : () => controller.selecionarVeiculo(veiculo),
                style: OutlinedButton.styleFrom(
                  alignment: Alignment.centerLeft,
                  foregroundColor:
                      controller.veiculoSelecionado?.id == veiculo.id
                      ? theme.primaryColor
                      : Colors.black87,
                  backgroundColor:
                      controller.veiculoSelecionado?.id == veiculo.id
                      ? theme.primaryColor.withValues(alpha: .08)
                      : Colors.white,
                  padding: const EdgeInsets.all(12),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: Row(
                  children: [
                    Expanded(child: Text('${veiculo.nome} - ${veiculo.placa}')),
                    if (controller.veiculoSelecionado?.id == veiculo.id)
                      const Icon(Icons.check_circle),
                  ],
                ),
              ),
            ),
        ],
      ),
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
