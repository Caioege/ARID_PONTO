import 'package:arid_rastreio/modules/acompanhante/home/rota_monitoramento_controller.dart';
import 'package:arid_rastreio/core/widgets/rastreio_tile_layer.dart';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:flutter_mobx/flutter_mobx.dart';

class RotaMonitoramentoPage extends StatefulWidget {
  final int rotaId;
  final String rotaNome;

  const RotaMonitoramentoPage({
    super.key,
    required this.rotaId,
    required this.rotaNome,
  });

  @override
  State<RotaMonitoramentoPage> createState() => _RotaMonitoramentoPageState();
}

class _RotaMonitoramentoPageState extends State<RotaMonitoramentoPage> {
  final controller = RotaMonitoramentoController();

  @override
  void initState() {
    super.initState();
    controller.iniciarMonitoramento(widget.rotaId);
  }

  @override
  void dispose() {
    controller.pararMonitoramento();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.rotaNome),
        backgroundColor: theme.primaryColor,
        foregroundColor: Colors.white,
      ),
      body: Observer(
        builder: (_) {
          if (controller.carregando && controller.latitude == null) {
            return const Center(child: CircularProgressIndicator());
          }

          if (controller.latitude == null || controller.longitude == null) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.location_off, size: 60, color: Colors.grey),
                  const SizedBox(height: 16),
                  Text(
                    controller.mensagemErro ?? 'Localização não disponível.',
                  ),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () =>
                        controller.iniciarMonitoramento(widget.rotaId),
                    child: const Text('Tentar novamente'),
                  ),
                ],
              ),
            );
          }

          final position = LatLng(controller.latitude!, controller.longitude!);

          return Stack(
            children: [
              FlutterMap(
                options: MapOptions(initialCenter: position, initialZoom: 15.0),
                children: [
                  const RastreioTileLayer(),
                  MarkerLayer(
                    markers: [
                      Marker(
                        point: position,
                        width: 80,
                        height: 80,
                        child: Column(
                          children: [
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 8,
                                vertical: 4,
                              ),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(8),
                                boxShadow: [
                                  BoxShadow(
                                    color: Colors.black26,
                                    blurRadius: 4,
                                  ),
                                ],
                              ),
                              child: const Text(
                                'Veículo',
                                style: TextStyle(
                                  fontSize: 10,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            const Icon(
                              Icons.directions_bus,
                              color: Colors.blue,
                              size: 40,
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ],
              ),
              Positioned(
                bottom: 16,
                right: 16,
                left: 16,
                child: Card(
                  elevation: 8,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Row(
                      children: [
                        const Icon(Icons.update, color: Colors.green),
                        const SizedBox(width: 12),
                        const Expanded(
                          child: Text(
                            'Monitoramento ativo. Atualizado a cada 30 segundos.',
                            style: TextStyle(
                              fontSize: 12,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ),
                        TextButton(
                          onPressed: () =>
                              controller.iniciarMonitoramento(widget.rotaId),
                          child: const Text('Atualizar'),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
