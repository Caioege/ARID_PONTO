import 'package:arid_rastreio/modules/acompanhante/service/acompanhante_service.dart';
import 'package:arid_rastreio/core/widgets/rastreio_tile_layer.dart';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

class RotaTrajetoPage extends StatefulWidget {
  final int rotaId;
  final String rotaNome;

  const RotaTrajetoPage({
    super.key,
    required this.rotaId,
    required this.rotaNome,
  });

  @override
  State<RotaTrajetoPage> createState() => _RotaTrajetoPageState();
}

class _RotaTrajetoPageState extends State<RotaTrajetoPage> {
  final _service = AcompanhanteService();
  List<LatLng> _pontos = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _carregarTrajeto();
  }

  Future<void> _carregarTrajeto() async {
    setState(() => _loading = true);
    try {
      final dados = await _service.obterTrajeto(widget.rotaId, DateTime.now());
      setState(() {
        _pontos = dados
            .map(
              (p) => LatLng(
                double.parse(p['latitude'].toString()),
                double.parse(p['longitude'].toString()),
              ),
            )
            .toList();
        _loading = false;
      });
    } catch (e) {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Histórico: ${widget.rotaNome}')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _pontos.isEmpty
          ? const Center(child: Text('Nenhum trajeto registrado para hoje.'))
          : FlutterMap(
              options: MapOptions(
                initialCenter: _pontos.first,
                initialZoom: 14.0,
              ),
              children: [
                const RastreioTileLayer(),
                PolylineLayer(
                  polylines: [
                    Polyline(
                      points: _pontos,
                      color: Colors.blue,
                      strokeWidth: 4.0,
                    ),
                  ],
                ),
                MarkerLayer(
                  markers: [
                    Marker(
                      point: _pontos.first,
                      child: const Icon(Icons.location_on, color: Colors.green),
                    ),
                    Marker(
                      point: _pontos.last,
                      child: const Icon(Icons.location_on, color: Colors.red),
                    ),
                  ],
                ),
              ],
            ),
    );
  }
}
