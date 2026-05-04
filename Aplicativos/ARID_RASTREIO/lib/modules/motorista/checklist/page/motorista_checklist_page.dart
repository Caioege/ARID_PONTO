import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/motorista/checklist/controller/checklist_controller.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/rota_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/checklist/dto/veiculo_checklist_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/controller/motorista_rotas_controller.dart';
import 'package:arid_rastreio/shared/layout/dialogs/app_dialog.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:mobx/mobx.dart';

class MotoristaChecklistPage extends StatefulWidget {
  const MotoristaChecklistPage({super.key});

  @override
  State<MotoristaChecklistPage> createState() => _MotoristaChecklistPageState();
}

class _MotoristaChecklistPageState extends State<MotoristaChecklistPage> {
  final controller = locator<ChecklistController>();
  final rotasController = locator<MotoristaRotasController>();

  bool _rotaAberta = true;
  bool _veiculoAberto = false;
  bool _checklistAberto = false;

  @override
  void initState() {
    super.initState();
    controller.carregarRotas();
    _sincronizarEtapasComEstadoAtual();
  }

  void _sincronizarEtapasComEstadoAtual() {
    _rotaAberta = controller.rotaSelecionada == null;
    _veiculoAberto =
        controller.rotaSelecionada != null &&
        controller.veiculoSelecionado == null;
    _checklistAberto =
        controller.rotaSelecionada != null &&
        controller.veiculoSelecionado != null &&
        !controller.checklistSalvo;
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
              _secaoRota(theme, rotas),
              const SizedBox(height: 12),
              _secaoVeiculo(theme),
              const SizedBox(height: 12),
              _secaoChecklist(theme),
              const SizedBox(height: 28),
            ],
          ),
        );
      },
    );
  }

  Widget _secaoRota(ThemeData theme, List<RotaChecklistDTO> rotas) {
    final selecionada = controller.rotaSelecionada == null
        ? null
        : rotas
              .where((r) => r.id == controller.rotaSelecionada!.id)
              .firstOrNull;

    return _secaoCollapse(
      theme: theme,
      titulo: 'Rota',
      subtitulo: controller.rotaSelecionada == null
          ? 'Selecione a rota'
          : controller.rotaSelecionada!.rotaFinalizada
          ? '${controller.rotaSelecionada!.descricao} - finalizada hoje'
          : controller.rotaSelecionada!.descricao,
      icone: Icons.alt_route,
      concluida: controller.rotaSelecionada != null,
      aberta: _rotaAberta,
      onToggle: () => setState(() => _rotaAberta = !_rotaAberta),
      child: DropdownButtonFormField<RotaChecklistDTO>(
        initialValue: selecionada,
        isExpanded: true,
        decoration: const InputDecoration(
          labelText: 'Rota',
          border: OutlineInputBorder(),
        ),
        hint: const Text('Selecione a rota'),
        items: rotas
            .map(
              (rota) => DropdownMenuItem<RotaChecklistDTO>(
                value: rota,
                child: Text(
                  rota.rotaFinalizada
                      ? '${rota.descricao} (finalizada hoje)'
                      : rota.descricao,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            )
            .toList(),
        onChanged: rotasController.rotaIniciada
            ? null
            : (rota) async {
                if (rota == null) return;
                await controller.selecionarRota(rota);
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

  Widget _secaoVeiculo(ThemeData theme) {
    if (controller.rotaSelecionada == null) {
      return _secaoBloqueada(
        theme,
        titulo: 'Veículo',
        subtitulo: 'Selecione uma rota para carregar os veículos.',
        icone: Icons.directions_car,
      );
    }

    return Observer(
      builder: (_) {
        if (controller.veiculosFuture == null ||
            controller.veiculosFuture!.status == FutureStatus.pending) {
          return _secaoCollapse(
            theme: theme,
            titulo: 'Veículo',
            subtitulo: 'Carregando veículos...',
            icone: Icons.directions_car,
            concluida: false,
            aberta: true,
            onToggle: () {},
            child: const Padding(
              padding: EdgeInsets.symmetric(vertical: 12),
              child: Center(child: CircularProgressIndicator()),
            ),
          );
        }

        final veiculos = controller.veiculosFuture!.value ?? [];
        final selecionado = controller.veiculoSelecionado == null
            ? null
            : veiculos
                  .where((v) => v.id == controller.veiculoSelecionado!.id)
                  .firstOrNull;

        return _secaoCollapse(
          theme: theme,
          titulo: 'Veículo',
          subtitulo: controller.veiculoSelecionado == null
              ? 'Selecione o veículo'
              : '${controller.veiculoSelecionado!.nome} - ${controller.veiculoSelecionado!.placa}',
          icone: Icons.directions_car,
          concluida: controller.veiculoSelecionado != null,
          aberta: _veiculoAberto,
          onToggle: () => setState(() => _veiculoAberto = !_veiculoAberto),
          child: DropdownButtonFormField<VeiculoChecklistDTO>(
            initialValue: selecionado,
            isExpanded: true,
            decoration: const InputDecoration(
              labelText: 'Veículo',
              border: OutlineInputBorder(),
            ),
            hint: const Text('Selecione o veículo'),
            items: veiculos
                .map(
                  (veiculo) => DropdownMenuItem<VeiculoChecklistDTO>(
                    value: veiculo,
                    child: Text(
                      '${veiculo.nome} - ${veiculo.placa}',
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                )
                .toList(),
            onChanged: rotasController.rotaIniciada
                ? null
                : (veiculo) {
                    if (veiculo == null) return;
                    controller.selecionarVeiculo(veiculo);
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

  Widget _secaoChecklist(ThemeData theme) {
    if (controller.veiculoSelecionado == null) {
      return _secaoBloqueada(
        theme,
        titulo: 'Checklist',
        subtitulo: 'Selecione um veículo para exibir os itens.',
        icone: Icons.checklist,
      );
    }

    return _secaoCollapse(
      theme: theme,
      titulo: 'Checklist',
      subtitulo:
          '${controller.itensMarcados} de ${controller.totalItens} itens verificados',
      icone: Icons.checklist,
      concluida: controller.checklistSalvo,
      aberta: _checklistAberto,
      onToggle: () => setState(() => _checklistAberto = !_checklistAberto),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _progressoChecklist(theme),
          const SizedBox(height: 14),
          ...controller.veiculoSelecionado!.checklist.map((item) {
            return Observer(
              builder: (_) {
                final marcado = item.checked;
                return InkWell(
                  borderRadius: BorderRadius.circular(12),
                  onTap: rotasController.rotaIniciada
                      ? null
                      : () => controller.alternarCheck(item),
                  child: Container(
                    margin: const EdgeInsets.only(bottom: 10),
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: marcado
                          ? theme.primaryColor.withValues(alpha: .10)
                          : Colors.grey.withValues(alpha: .04),
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(
                        color: marcado
                            ? theme.primaryColor
                            : Colors.grey.withValues(alpha: .25),
                      ),
                    ),
                    child: Row(
                      children: [
                        Icon(
                          marcado
                              ? Icons.check_circle
                              : Icons.radio_button_unchecked,
                          color: marcado ? theme.primaryColor : Colors.grey,
                        ),
                        const SizedBox(width: 10),
                        Expanded(child: Text(item.descricao)),
                      ],
                    ),
                  ),
                );
              },
            );
          }),
          if (!rotasController.rotaIniciada &&
              !controller.rotaSelecionada!.rotaFinalizada)
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: controller.salvando
                    ? null
                    : () async {
                        await controller.salvarChecklist();

                        if (!mounted || controller.erroProcessamento != null) {
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
                  padding: const EdgeInsets.symmetric(vertical: 15),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
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
        ],
      ),
    );
  }

  Widget _progressoChecklist(ThemeData theme) {
    return Observer(
      builder: (_) => Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Progresso do checklist',
            style: TextStyle(fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 8),
          ClipRRect(
            borderRadius: BorderRadius.circular(10),
            child: LinearProgressIndicator(
              value: controller.progressoChecklist,
              minHeight: 8,
              backgroundColor: Colors.grey.withValues(alpha: 0.2),
              valueColor: AlwaysStoppedAnimation(theme.primaryColor),
            ),
          ),
        ],
      ),
    );
  }

  Widget _secaoBloqueada(
    ThemeData theme, {
    required String titulo,
    required String subtitulo,
    required IconData icone,
  }) {
    return _secaoCollapse(
      theme: theme,
      titulo: titulo,
      subtitulo: subtitulo,
      icone: icone,
      concluida: false,
      aberta: false,
      onToggle: () {},
      child: const SizedBox.shrink(),
    );
  }

  Widget _secaoCollapse({
    required ThemeData theme,
    required String titulo,
    required String subtitulo,
    required IconData icone,
    required bool concluida,
    required bool aberta,
    required VoidCallback onToggle,
    required Widget child,
  }) {
    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: theme.primaryColor.withValues(alpha: .10)),
        boxShadow: [
          BoxShadow(color: Colors.black.withValues(alpha: .05), blurRadius: 8),
        ],
      ),
      child: Column(
        children: [
          InkWell(
            borderRadius: BorderRadius.circular(16),
            onTap: onToggle,
            child: Padding(
              padding: const EdgeInsets.all(14),
              child: Row(
                children: [
                  CircleAvatar(
                    radius: 18,
                    backgroundColor: concluida
                        ? Colors.green.withValues(alpha: .12)
                        : theme.primaryColor.withValues(alpha: .10),
                    child: Icon(
                      concluida ? Icons.check : icone,
                      color: concluida ? Colors.green : theme.primaryColor,
                      size: 20,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          titulo,
                          style: const TextStyle(fontWeight: FontWeight.bold),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          subtitulo,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: Colors.grey[700],
                            fontSize: 12,
                          ),
                        ),
                      ],
                    ),
                  ),
                  Icon(
                    aberta
                        ? Icons.keyboard_arrow_up
                        : Icons.keyboard_arrow_down,
                    color: Colors.grey[700],
                  ),
                ],
              ),
            ),
          ),
          AnimatedCrossFade(
            firstChild: const SizedBox(width: double.infinity),
            secondChild: Padding(
              padding: const EdgeInsets.fromLTRB(14, 0, 14, 14),
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
          _etapaChip(theme, 'Veículo', veiculoOk, rotaOk),
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
