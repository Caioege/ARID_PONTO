import 'package:flutter_map/flutter_map.dart';
import 'package:flutter/widgets.dart';

class RastreioTileLayer extends StatelessWidget {
  const RastreioTileLayer({super.key});

  @override
  Widget build(BuildContext context) {
    return TileLayer(
      urlTemplate:
          'https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}.png',
      subdomains: const ['a', 'b', 'c', 'd'],
      userAgentPackageName: 'com.arid.rastreio',
    );
  }
}
