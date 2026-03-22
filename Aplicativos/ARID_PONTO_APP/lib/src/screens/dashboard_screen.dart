import 'dart:async';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:http/http.dart' as http;
import 'package:app_arid_escolas/src/constants.dart' as Constants;

class DropdownItem {
  final int codigo;
  final String descricao;

  DropdownItem({required this.codigo, required this.descricao});

  factory DropdownItem.fromJson(Map<String, dynamic> json) {
    return DropdownItem(codigo: json['codigo'], descricao: json['descricao']);
  }
}

class DashboardEscolaScreen extends StatefulWidget {
  const DashboardEscolaScreen({super.key});

  @override
  State<DashboardEscolaScreen> createState() => _DashboardEscolaScreenState();
}

class _DashboardEscolaScreenState extends State<DashboardEscolaScreen> {
  // Dados do Header
  String? _redeDeEnsino;
  String? _escolaNome;
  String? _usuarioNome;

  // KPIs
  int _totalAlunosCursando = 0;
  int _alunosPresentes = 0;

  // Filtros
  DateTime _dataSelecionada = DateTime.now();
  // MODIFICADO: Inicializa os filtros com o valor -1 ("Todos")
  int? _turnoSelecionado = -1;
  int? _turmaSelecionada = -1;

  List<DropdownItem> _listaTurnos = [];
  List<DropdownItem> _listaTurmas = [];

  // Controles de Carregamento
  bool _carregandoFiltros = true;
  bool _carregandoKpis = true;

  // Timer para atualização automática
  Timer? _autoRefreshTimer;

  @override
  void initState() {
    super.initState();
    _carregarDadosIniciais();
    _iniciarTimerDeAtualizacao();
  }

  @override
  void dispose() {
    _autoRefreshTimer?.cancel();
    super.dispose();
  }

  void _iniciarTimerDeAtualizacao() {
    _autoRefreshTimer = Timer.periodic(const Duration(seconds: 30), (timer) {
      if (!_carregandoKpis) {
        _carregarKpis();
      }
    });
  }

  Future<void> _carregarDadosIniciais() async {
    await _carregarInfoUsuario();
    if (mounted) {
      await _carregarFiltros();
      await _carregarKpis();
    }
  }

  Future<void> _carregarInfoUsuario() async {
    final prefs = await SharedPreferences.getInstance();
    setState(() {
      _redeDeEnsino = prefs.getString('redeDeEnsinoNome');
      _escolaNome = prefs.getString('escolaNome');
      _usuarioNome = prefs.getString('nome');
    });
  }

  void _exibirErro(String mensagem) {
    if (!mounted) return;
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Erro'),
        content: Text(mensagem),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  Future<void> _carregarFiltros() async {
    setState(() => _carregandoFiltros = true);

    try {
      final prefs = await SharedPreferences.getInstance();
      final escolaId = prefs.getInt('escolaId');
      final token = prefs.getString('token') ?? '';

      final apiUrl = Constants.API_URL;
      final uri = Uri.parse('$apiUrl/dados-escola/$escolaId');
      final resposta = await http.get(
        uri,
        headers: {'Authorization': 'Bearer $token'},
      );

      if (resposta.statusCode == 200) {
        final dados = json.decode(resposta.body);
        final List<dynamic> turnosJson = dados['turnos'];
        final List<dynamic> turmasJson = dados['turmas'];

        final turnos = turnosJson
            .map((json) => DropdownItem.fromJson(json))
            .toList();
        turnos.insert(0, DropdownItem(codigo: -1, descricao: 'Todos'));

        final turmas = turmasJson
            .map((json) => DropdownItem.fromJson(json))
            .toList();
        turmas.insert(0, DropdownItem(codigo: -1, descricao: 'Todas'));

        setState(() {
          _listaTurnos = turnos;
          _listaTurmas = turmas;
        });
      }
    } catch (e) {
    } finally {
      if (mounted) {
        setState(() => _carregandoFiltros = false);
      }
    }
  }

  Future<void> _carregarKpis() async {
    setState(() => _carregandoKpis = true);

    try {
      final prefs = await SharedPreferences.getInstance();
      final escolaId = prefs.getInt('escolaId') ?? 0;
      final token = prefs.getString('token') ?? '';

      final dataFormatada = DateFormat('yyyy-MM-dd').format(_dataSelecionada);

      final queryParams = <String, String>{};
      if (_turnoSelecionado != null && _turnoSelecionado != -1) {
        queryParams['turno'] = _turnoSelecionado.toString();
      }
      if (_turmaSelecionada != null && _turmaSelecionada != -1) {
        queryParams['turmaId'] = _turmaSelecionada.toString();
      }

      final apiUrl = Constants.API_URL;
      final uri = Uri.parse(
        '$apiUrl/dados-dashboard-escola/$escolaId/$dataFormatada',
      ).replace(queryParameters: queryParams);

      final resposta = await http.get(
        uri,
        headers: {'Authorization': 'Bearer $token'},
      );

      if (resposta.statusCode == 200) {
        final dados = json.decode(resposta.body);
        setState(() {
          _totalAlunosCursando = dados['totalDeAlunosCursando'];
          _alunosPresentes = dados['totalDeAlunosPresente'];
        });
      } else {
        throw Exception(
          'Falha ao carregar dados do dashboard: ${resposta.body}',
        );
      }
    } catch (e) {
      _exibirErro(
        'Não foi possível carregar os dados do dashboard. Tente novamente.',
      );
    } finally {
      if (mounted) {
        setState(() => _carregandoKpis = false);
      }
    }
  }

  void _confirmarSair(BuildContext context) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text(
          'Deseja sair?',
          style: TextStyle(
            color: Color.fromARGB(255, 0, 53, 77),
            fontWeight: FontWeight.bold,
          ),
        ),
        content: const Text('Você realmente deseja sair do aplicativo?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text(
              'Cancelar',
              style: TextStyle(color: Colors.grey, fontWeight: FontWeight.bold),
            ),
          ),
          TextButton(
            onPressed: () async {
              final prefs = await SharedPreferences.getInstance();
              await prefs.clear();

              if (context.mounted) {
                Navigator.pushNamedAndRemoveUntil(
                  context,
                  '/login',
                  (route) => false,
                );
              }
            },
            child: const Text(
              'Sair',
              style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _selecionarData(BuildContext context) async {
    final DateTime? data = await showDatePicker(
      context: context,
      initialDate: _dataSelecionada,
      firstDate: DateTime(2020),
      lastDate: DateTime.now(),
    );

    if (data != null && data != _dataSelecionada) {
      setState(() {
        _dataSelecionada = data;
      });
      _carregarKpis();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey.shade100,
      body: SafeArea(
        child: Column(
          children: [
            _header(context),
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  children: [
                    _buildFiltros(),
                    const SizedBox(height: 24),
                    _buildKpis(),
                  ],
                ),
              ),
            ),
            _footer(context),
          ],
        ),
      ),
    );
  }

  Widget _footer(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Image.asset('assets/images/logo-sistema.png', width: 60),
          InkWell(
            onTap: () => _confirmarSair(context),
            borderRadius: BorderRadius.circular(50),
            child: Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: const Color.fromARGB(255, 0, 53, 77),
                borderRadius: BorderRadius.circular(50),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.2),
                    blurRadius: 4,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: const Icon(Icons.logout, color: Colors.white, size: 18),
            ),
          ),
        ],
      ),
    );
  }

  Widget _header(BuildContext context) {
    return Container(
      width: double.infinity,
      decoration: const BoxDecoration(
        color: Color.fromARGB(255, 0, 53, 77),
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(40),
          bottomRight: Radius.circular(40),
        ),
      ),
      padding: const EdgeInsets.fromLTRB(24, 24, 16, 24),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  _redeDeEnsino ?? '',
                  style: const TextStyle(
                    color: Color.fromARGB(150, 255, 255, 255),
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  _escolaNome ?? 'Carregando...',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    const Icon(
                      Icons.person_outline,
                      color: Colors.white70,
                      size: 20,
                    ),
                    const SizedBox(width: 8),
                    Text(
                      _usuarioNome ?? 'Carregando...',
                      style: const TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          IconButton(
            icon: const Icon(Icons.refresh, color: Colors.white),
            onPressed: _carregandoKpis ? null : _carregarKpis,
            tooltip: 'Atualizar Dados',
          ),
        ],
      ),
    );
  }

  /// **MODIFICADO**: Atualiza a lógica de limpeza para resetar para -1 ("Todos").
  Widget _buildFiltros() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          "Filtrar por data",
          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
        ),
        const SizedBox(height: 8),
        InkWell(
          onTap: () => _selecionarData(context),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 16),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: Colors.grey.shade300),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(DateFormat('dd/MM/yyyy').format(_dataSelecionada)),
                const Icon(Icons.calendar_today),
              ],
            ),
          ),
        ),
        const SizedBox(height: 16),
        if (_carregandoFiltros)
          const Center(child: CircularProgressIndicator())
        else
          Column(
            children: [
              _buildDropdown(
                hint: 'Turno',
                value: _turnoSelecionado,
                items: _listaTurnos,
                onChanged: (novoCodigo) {
                  setState(() {
                    _turmaSelecionada = -1; // Reseta a turma para "Todas"
                    _turnoSelecionado = novoCodigo;
                  });
                  _carregarKpis();
                },
                onClear: () {
                  setState(() {
                    _turnoSelecionado = -1; // Limpa para "Todos"
                    _turmaSelecionada = -1; // Limpa também a turma
                  });
                  _carregarKpis();
                },
              ),
              const SizedBox(height: 16),
              _buildDropdown(
                hint: 'Turma',
                value: _turmaSelecionada,
                items: _listaTurmas,
                onChanged: (novoCodigo) {
                  setState(() {
                    _turmaSelecionada = novoCodigo;
                  });
                  _carregarKpis();
                },
                onClear: () {
                  setState(() {
                    _turmaSelecionada = -1; // Limpa para "Todas"
                  });
                  _carregarKpis();
                },
              ),
            ],
          ),
      ],
    );
  }

  /// **MODIFICADO**: O ícone de limpar só aparece se o valor for diferente de -1.
  Widget _buildDropdown({
    required String hint,
    required int? value,
    required List<DropdownItem> items,
    required ValueChanged<int?> onChanged,
    required VoidCallback onClear,
  }) {
    return DropdownButtonFormField<int>(
      value: value,
      isExpanded: true,
      decoration: InputDecoration(
        labelText: hint,
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
        filled: true,
        fillColor: Colors.white,
        contentPadding: const EdgeInsets.symmetric(
          horizontal: 12,
          vertical: 16,
        ),
        suffixIcon: value != null && value != -1
            ? IconButton(
                icon: const Icon(Icons.clear),
                onPressed: onClear,
                tooltip: 'Limpar filtro',
              )
            : null,
      ),
      items: items.map((DropdownItem item) {
        return DropdownMenuItem<int>(
          value: item.codigo,
          child: Text(item.descricao, overflow: TextOverflow.ellipsis),
        );
      }).toList(),
      onChanged: onChanged,
    );
  }

  Widget _buildKpis() {
    if (_carregandoKpis) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(32.0),
          child: CircularProgressIndicator(),
        ),
      );
    }
    return Row(
      children: [
        Expanded(
          child: _kpiCard(
            titulo: 'Alunos Cursando',
            valor: _totalAlunosCursando.toString(),
            icone: Icons.school,
            cor: Colors.blue.shade700,
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: _kpiCard(
            titulo: 'Alunos Presentes',
            valor: _alunosPresentes.toString(),
            icone: Icons.check_circle,
            cor: Colors.green.shade700,
          ),
        ),
      ],
    );
  }

  Widget _kpiCard({
    required String titulo,
    required String valor,
    required IconData icone,
    required Color cor,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.1),
            blurRadius: 8,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icone, color: cor, size: 32),
          const SizedBox(height: 12),
          Text(
            valor,
            style: TextStyle(
              fontSize: 28,
              fontWeight: FontWeight.bold,
              color: cor,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            titulo,
            style: TextStyle(color: Colors.grey.shade600, fontSize: 14),
          ),
        ],
      ),
    );
  }
}
