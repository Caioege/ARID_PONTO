import 'package:arid_rastreio/modules/acompanhante/home/rota_monitoramento_page.dart';
import 'package:arid_rastreio/modules/acompanhante/home/rota_trajeto_page.dart';
import 'package:arid_rastreio/ioc/service_locator.dart';
import 'package:arid_rastreio/modules/acompanhante/service/acompanhante_service.dart';
import 'package:arid_rastreio/shared/layout/app_bar/base_app_bar.dart';
import 'package:arid_rastreio/shared/layout/drawer/page/base_drawer.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class AcompanhanteHomePage extends StatefulWidget {
  const AcompanhanteHomePage({super.key});

  @override
  State<AcompanhanteHomePage> createState() => _AcompanhanteHomePageState();
}

class _AcompanhanteHomePageState extends State<AcompanhanteHomePage> {
  final _service = AcompanhanteService();
  final GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>();
  
  List<Map<String, dynamic>> _rotas = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _carregarRotas();
  }

  Future<void> _carregarRotas() async {
    setState(() => _loading = true);
    try {
      final rotas = await _service.obterMinhasRotas();
      setState(() {
        _rotas = rotas;
        _loading = false;
      });
    } catch (e) {
      setState(() => _loading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erro ao carregar rotas: $e')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      key: _scaffoldKey,
      appBar: BaseAppBar(scaffoldKey: _scaffoldKey),
      drawer: BaseDrawer(),
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [
              theme.primaryColor.withValues(alpha: 0.05),
              Colors.white,
            ],
          ),
        ),
        child: RefreshIndicator(
          onRefresh: _carregarRotas,
          child: _loading 
            ? const Center(child: CircularProgressIndicator())
            : _rotas.isEmpty 
              ? _buildEmptyState(theme)
              : _buildRotaList(theme),
        ),
      ),
    );
  }

  Widget _buildEmptyState(ThemeData theme) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.route_outlined, size: 80, color: theme.primaryColor.withValues(alpha: 0.3)),
          const SizedBox(height: 16),
          const Text(
            'Nenhuma rota vinculada',
            style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          const Text('Você não possui rotas ativas ou agendadas.'),
        ],
      ),
    );
  }

  Widget _buildRotaList(ThemeData theme) {
    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: _rotas.length,
      itemBuilder: (context, index) {
        final rota = _rotas[index];
        return Card(
          elevation: 4,
          margin: const EdgeInsets.bottom(16),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          child: Column(
            children: [
              ListTile(
                contentPadding: const EdgeInsets.all(16),
                leading: CircleAvatar(
                  backgroundColor: theme.primaryColor.withValues(alpha: 0.1),
                  child: Icon(Icons.departure_board, color: theme.primaryColor),
                ),
                title: Text(
                  rota['nome'] ?? 'Rota sem Nome',
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
                subtitle: Text(rota['descricao'] ?? ''),
                trailing: const Icon(Icons.chevron_right),
              ),
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                child: Row(
                  children: [
                    Expanded(
                      child: OutlinedButton.icon(
                        onPressed: () {
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (context) => RotaTrajetoPage(
                                rotaId: rota['id'],
                                rotaNome: rota['nome'],
                              ),
                            ),
                          );
                        },
                        icon: const Icon(Icons.history, size: 18),
                        label: const Text('Histórico'),
                        style: OutlinedButton.styleFrom(
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                        ),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: ElevatedButton.icon(
                        onPressed: () {
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (context) => RotaMonitoramentoPage(
                                rotaId: rota['id'],
                                rotaNome: rota['nome'],
                              ),
                            ),
                          );
                        },
                        icon: const Icon(Icons.gps_fixed, size: 18),
                        label: const Text('Monitorar'),
                        style: ElevatedButton.styleFrom(
                          backgroundColor: theme.primaryColor,
                          foregroundColor: Colors.white,
                          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}
