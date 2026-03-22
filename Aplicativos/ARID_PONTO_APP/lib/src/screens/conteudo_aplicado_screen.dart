import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:connectivity_plus/connectivity_plus.dart';
import '../constants.dart' as Constants;

class ConteudoAplicadoScreen extends StatefulWidget {
  const ConteudoAplicadoScreen({super.key});

  @override
  State<ConteudoAplicadoScreen> createState() => _ConteudoAplicadoScreenState();
}

class _ConteudoAplicadoScreenState extends State<ConteudoAplicadoScreen> {
  Constants.TurmaInfo? _turmaInfo;
  DateTime? _dataSelecionada;
  bool _carregandoTela = true;
  bool _carregandoConteudo = false;
  bool _salvando = false;

  final _formKey = GlobalKey<FormState>();
  late List<TextEditingController> _controllers;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_turmaInfo == null) {
      final routeArgs =
          ModalRoute.of(context)!.settings.arguments as Map<String, dynamic>;
      setState(() {
        _turmaInfo = Constants.TurmaInfo.fromMap(routeArgs);
        _controllers = List.generate(
          _turmaInfo!.configuracaoConteudo.length,
          (_) => TextEditingController(),
        );
        _carregandoTela = false;
      });
    }
  }

  @override
  void dispose() {
    for (var controller in _controllers) {
      controller.dispose();
    }
    super.dispose();
  }

  DateTime _encontrarDataInicialValida() {
    DateTime dataInicial = _dataSelecionada ?? DateTime.now();
    final hoje = DateTime.now();
    final fimDaTurma = _turmaInfo!.fimTurma;
    final ultimaDataPermitida = hoje.isBefore(fimDaTurma) ? hoje : fimDaTurma;

    if (dataInicial.isAfter(ultimaDataPermitida)) {
      dataInicial = ultimaDataPermitida;
    }

    for (int i = 0; i < 90; i++) {
      DateTime diaAnalisado = dataInicial.subtract(Duration(days: i));
      if (diaAnalisado.isBefore(_turmaInfo!.inicioTurma)) break;
      if (_turmaInfo!.diasDeAula[diaAnalisado.weekday - 1]) return diaAnalisado;
    }
    return _turmaInfo!.inicioTurma;
  }

  Future<void> _selecionarData(BuildContext context) async {
    final DateTime dataInicialValida = _encontrarDataInicialValida();
    final DateTime hoje = DateTime.now();
    final DateTime fimDaTurma = _turmaInfo!.fimTurma;
    final DateTime ultimaDataPermitida = hoje.isBefore(fimDaTurma)
        ? hoje
        : fimDaTurma;

    final DateTime? data = await showDatePicker(
      context: context,
      initialDate: dataInicialValida,
      firstDate: _turmaInfo!.inicioTurma,
      lastDate: ultimaDataPermitida,
      selectableDayPredicate: (DateTime day) {
        if (day.isAfter(ultimaDataPermitida)) return false;
        return _turmaInfo!.diasDeAula[day.weekday - 1];
      },
    );

    if (data != null && data != _dataSelecionada) {
      setState(() => _dataSelecionada = data);
      _carregarConteudo();
    }
  }

  Future<void> _carregarConteudo() async {
    if (_dataSelecionada == null || _turmaInfo == null) return;
    setState(() => _carregandoConteudo = true);

    final isOnline = !(await Connectivity().checkConnectivity()).contains(
      ConnectivityResult.none,
    );
    List<dynamic> conteudoCarregado = [];

    try {
      if (isOnline) {
        conteudoCarregado = await _carregarConteudoDaApi();
      } else {
        conteudoCarregado = await _carregarConteudoLocal();
      }

      for (int i = 0; i < _controllers.length; i++) {
        var campoEncontrado = conteudoCarregado.firstWhere(
          (c) => c['ordem'] == _turmaInfo!.configuracaoConteudo[i].ordem,
          orElse: () => null,
        );
        _controllers[i].text = campoEncontrado?['valor'] ?? '';
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Erro ao carregar conteúdo: $e'),
          backgroundColor: Colors.red,
        ),
      );
    } finally {
      setState(() => _carregandoConteudo = false);
    }
  }

  Future<List<dynamic>> _carregarConteudoDaApi() async {
    final dataFormatada = DateFormat('yyyy-MM-dd').format(_dataSelecionada!);
    final uri = Uri.parse(
      '${Constants.API_URL}/conteudo/${_turmaInfo!.turmaId}/${_turmaInfo!.disciplinaId}/$dataFormatada',
    );
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');
    final response = await http.get(
      uri,
      headers: {'Authorization': 'Bearer $token'},
    );

    if (response.statusCode == 200) {
      final Map<String, dynamic> dados = jsonDecode(response.body);
      return dados['conteudos'] ?? [];
    }
    if (response.statusCode == 404) return [];
    throw Exception(
      'Falha ao carregar conteúdo da API: ${response.statusCode}',
    );
  }

  Future<List<dynamic>> _carregarConteudoLocal() async {
    final prefs = await SharedPreferences.getInstance();
    final String? registrosJson = prefs.getString('conteudos_pendentes');
    if (registrosJson == null) return [];

    final List<dynamic> todosRegistros = jsonDecode(registrosJson);
    final dataFormatada = DateFormat('yyyy-MM-dd').format(_dataSelecionada!);

    final registroDoDia = todosRegistros.firstWhere((reg) {
      final dataRegistro = DateTime.parse(reg['data']);
      return reg['turmaId'] == _turmaInfo!.turmaId &&
          reg['disciplinaId'] == _turmaInfo!.disciplinaId &&
          DateFormat('yyyy-MM-dd').format(dataRegistro) == dataFormatada;
    }, orElse: () => null);

    return registroDoDia?['conteudos'] ?? [];
  }

  Future<void> _salvarConteudo() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    setState(() => _salvando = true);
    final scaffoldMessenger = ScaffoldMessenger.of(context);

    final List<Map<String, dynamic>> payloadConteudos = [];
    for (int i = 0; i < _controllers.length; i++) {
      payloadConteudos.add({
        'ordem': _turmaInfo!.configuracaoConteudo[i].ordem,
        'titulo': _turmaInfo!.configuracaoConteudo[i].titulo,
        'valor': _controllers[i].text,
      });
    }

    final Map<String, dynamic> registroCompleto = {
      'turmaId': _turmaInfo!.turmaId,
      'disciplinaId': _turmaInfo!.disciplinaId,
      'data': _dataSelecionada!.toIso8601String(),
      'conteudos': payloadConteudos,
    };

    final isOnline = !(await Connectivity().checkConnectivity()).contains(
      ConnectivityResult.none,
    );
    try {
      if (isOnline) {
        await _enviarConteudoParaApi(registroCompleto);
        scaffoldMessenger.showSnackBar(
          const SnackBar(
            content: Text('Conteúdo salvo com sucesso!'),
            backgroundColor: Colors.green,
          ),
        );
      } else {
        await _salvarConteudoLocalmente(registroCompleto);
        scaffoldMessenger.showSnackBar(
          const SnackBar(
            content: Text('Offline. Conteúdo salvo para enviar depois.'),
            backgroundColor: Colors.orange,
          ),
        );
      }
      Navigator.pop(context);
    } catch (e) {
      scaffoldMessenger.showSnackBar(
        SnackBar(
          content: Text('Erro ao salvar: $e'),
          backgroundColor: Colors.red,
        ),
      );
    } finally {
      setState(() => _salvando = false);
    }
  }

  Future<void> _enviarConteudoParaApi(Map<String, dynamic> payload) async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');
    final apiUrl = Constants.API_URL;
    final response = await http.post(
      Uri.parse('$apiUrl/conteudo/registrar'),
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
      body: jsonEncode(payload),
    );
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Falha ao enviar dados para o servidor.');
    }
  }

  Future<void> _salvarConteudoLocalmente(Map<String, dynamic> registro) async {
    final prefs = await SharedPreferences.getInstance();
    final String? registrosJson = prefs.getString('conteudos_pendentes');
    List<dynamic> todosRegistros = registrosJson != null
        ? jsonDecode(registrosJson)
        : [];

    final dataFormatada = DateFormat('yyyy-MM-dd').format(_dataSelecionada!);
    todosRegistros.removeWhere((reg) {
      final dataRegistro = DateTime.parse(reg['data']);
      return reg['turmaId'] == _turmaInfo!.turmaId &&
          reg['disciplinaId'] == _turmaInfo!.disciplinaId &&
          DateFormat('yyyy-MM-dd').format(dataRegistro) == dataFormatada;
    });

    todosRegistros.add(registro);
    await prefs.setString('conteudos_pendentes', jsonEncode(todosRegistros));
  }

  @override
  Widget build(BuildContext context) {
    if (_carregandoTela || _turmaInfo == null) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }
    return Scaffold(
      backgroundColor: Colors.grey.shade100,
      body: SafeArea(
        child: Column(
          children: [
            _header(),
            _buildDatePicker(),
            if (_dataSelecionada != null) Expanded(child: _buildBody()),
          ],
        ),
      ),
      bottomNavigationBar: (_dataSelecionada == null)
          ? null
          : _buildBottomBar(),
    );
  }

  Widget _header() {
    return Container(
      width: double.infinity,
      decoration: const BoxDecoration(
        color: Color.fromARGB(255, 0, 53, 77),
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(40),
          bottomRight: Radius.circular(40),
        ),
      ),
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 24),
      child: Row(
        children: [
          IconButton(
            icon: const Icon(Icons.arrow_back, color: Colors.white),
            onPressed: () => Navigator.pop(context),
          ),
          const SizedBox(width: 8),
          const Icon(Icons.edit_note, color: Colors.white, size: 28),
          const SizedBox(width: 12),
          const Text(
            'Conteúdo Aplicado',
            style: TextStyle(
              color: Colors.white,
              fontSize: 20,
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDatePicker() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: OutlinedButton.icon(
        icon: const Icon(Icons.calendar_today),
        label: Text(
          _dataSelecionada == null
              ? 'SELECIONAR DATA DA AULA'
              : 'Aula de: ${DateFormat('dd/MM/yyyy').format(_dataSelecionada!)}',
        ),
        style: OutlinedButton.styleFrom(
          padding: const EdgeInsets.symmetric(vertical: 16),
          minimumSize: const Size(double.infinity, 50),
          foregroundColor: const Color.fromARGB(255, 0, 53, 77),
          side: const BorderSide(color: Color.fromARGB(255, 0, 53, 77)),
        ),
        onPressed: () => _selecionarData(context),
      ),
    );
  }

  Widget _buildBody() {
    if (_carregandoConteudo) {
      return const Center(child: CircularProgressIndicator());
    }
    return Form(
      key: _formKey,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: _turmaInfo!.configuracaoConteudo.length,
        itemBuilder: (context, index) {
          final config = _turmaInfo!.configuracaoConteudo[index];
          return Padding(
            padding: const EdgeInsets.only(bottom: 20.0),
            child: TextFormField(
              controller: _controllers[index],
              decoration: InputDecoration(
                labelText: config.titulo,
                labelStyle: const TextStyle(fontWeight: FontWeight.bold),
                border: const OutlineInputBorder(
                  borderRadius: BorderRadius.all(Radius.circular(12)),
                ),
                alignLabelWithHint: true,
                helperText: config.obrigatorio ? '* Campo obrigatório' : '',
              ),
              maxLength: config.quantidadeDeCaracteresMaxima > 0
                  ? config.quantidadeDeCaracteresMaxima
                  : null,
              keyboardType: TextInputType.multiline,
              minLines: 3,
              maxLines: null,
              validator: (value) {
                if (config.obrigatorio &&
                    (value == null || value.trim().isEmpty)) {
                  return 'Este campo é obrigatório.';
                }
                return null;
              },
            ),
          );
        },
      ),
    );
  }

  Widget _buildBottomBar() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: ElevatedButton.icon(
        onPressed: _salvando ? null : _salvarConteudo,
        icon: _salvando
            ? const SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(
                  color: Colors.white,
                  strokeWidth: 3.0,
                ),
              )
            : const Icon(Icons.save),
        label: const Text('SALVAR CONTEÚDO'),
        style: ElevatedButton.styleFrom(
          backgroundColor: const Color.fromARGB(255, 0, 53, 77),
          foregroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(vertical: 16),
        ),
      ),
    );
  }
}
