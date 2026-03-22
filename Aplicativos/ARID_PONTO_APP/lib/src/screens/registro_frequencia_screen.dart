import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:connectivity_plus/connectivity_plus.dart';
import '../constants.dart' as Constants;
import 'dashboard_professor_screen.dart';

class RegistroFrequenciaScreen extends StatefulWidget {
  const RegistroFrequenciaScreen({super.key});

  @override
  State<RegistroFrequenciaScreen> createState() =>
      _RegistroFrequenciaScreenState();
}

class _RegistroFrequenciaScreenState extends State<RegistroFrequenciaScreen> {
  Constants.TurmaInfo? _turmaInfo;
  DateTime? _dataSelecionada;
  Map<int, bool?> _frequencia = {};
  bool _carregandoTela = true;
  bool _carregandoFrequencia = false;
  bool _salvando = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_turmaInfo == null) {
      final routeArgs =
          ModalRoute.of(context)!.settings.arguments as Map<String, dynamic>;
      setState(() {
        _turmaInfo = Constants.TurmaInfo.fromMap(routeArgs);
        _carregandoTela = false;
      });
    }
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
        if (day.isBefore(_turmaInfo!.inicioTurma) ||
            day.isAfter(_turmaInfo!.fimTurma)) {
          return false;
        }
        return _turmaInfo!.diasDeAula[day.weekday - 1];
      },
    );

    if (data != null && data != _dataSelecionada) {
      setState(() {
        _dataSelecionada = data;
        _frequencia.clear();
      });
      _carregarFrequencias();
    }
  }

  DateTime _encontrarDataInicialValida() {
    DateTime dataInicial = _dataSelecionada ?? DateTime.now();

    // Limita a data para não ficar depois do fim da turma
    if (dataInicial.isAfter(_turmaInfo!.fimTurma)) {
      dataInicial = _turmaInfo!.fimTurma;
    }

    // Procura para trás (até 90 dias) pelo último dia de aula válido
    for (int i = 0; i < 90; i++) {
      DateTime diaAnalisado = dataInicial.subtract(Duration(days: i));

      // Verifica se está dentro do período da turma
      if (diaAnalisado.isBefore(_turmaInfo!.inicioTurma)) {
        break; // Para a busca se chegar antes do início da turma
      }

      // Verifica se é um dia de aula permitido
      bool diaDaSemanaValido = _turmaInfo!.diasDeAula[diaAnalisado.weekday - 1];
      if (diaDaSemanaValido) {
        return diaAnalisado; // Encontrou um dia válido!
      }
    }

    // Se não encontrar nada, retorna o início da turma como fallback
    return _turmaInfo!.inicioTurma;
  }

  Future<void> _carregarFrequencias() async {
    if (_dataSelecionada == null || _turmaInfo == null) return;

    setState(() => _carregandoFrequencia = true);

    final connectivityResult = await Connectivity().checkConnectivity();
    final isOnline = !connectivityResult.contains(ConnectivityResult.none);
    Map<int, bool?> frequenciaCarregada = {};

    try {
      if (isOnline) {
        frequenciaCarregada = await _carregarFrequenciasDaApi();
      } else {
        frequenciaCarregada = await _carregarFrequenciasLocais();
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Erro ao carregar frequências: $e'),
          backgroundColor: Colors.red,
        ),
      );
    }

    setState(() {
      if (frequenciaCarregada.isEmpty) {
        for (var aluno in _turmaInfo!.alunos) {
          frequenciaCarregada[aluno.alunoTurmaId] = null;
        }
      }
      _frequencia = frequenciaCarregada;
      _carregandoFrequencia = false;
    });
  }

  Future<Map<int, bool?>> _carregarFrequenciasDaApi() async {
    final dataFormatada = DateFormat('yyyy-MM-dd').format(_dataSelecionada!);
    final uri = Uri.parse(
      '${Constants.API_URL}/frequencia/${_turmaInfo!.turmaId}/${_turmaInfo!.disciplinaId}/$dataFormatada',
    );

    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');

    final response = await http.get(
      uri,
      headers: {'Authorization': 'Bearer $token'},
    );

    if (response.statusCode == 200) {
      final List<dynamic> dados = jsonDecode(response.body);
      return {for (var item in dados) item['alunoTurmaId']: item['presente']};
    }
    if (response.statusCode == 404) return {};

    throw Exception('Falha ao carregar dados da API: ${response.statusCode}');
  }

  Future<Map<int, bool?>> _carregarFrequenciasLocais() async {
    final prefs = await SharedPreferences.getInstance();
    final String? registrosJson = prefs.getString('frequencias_pendentes');
    if (registrosJson == null) return {};

    final List<dynamic> todosRegistros = jsonDecode(registrosJson);
    final dataFormatada = DateFormat('yyyy-MM-dd').format(_dataSelecionada!);

    final registrosDoDia = todosRegistros.where((reg) {
      final dataRegistro = DateTime.parse(reg['data']);
      return reg['turmaId'] == _turmaInfo!.turmaId &&
          reg['disciplinaId'] == _turmaInfo!.disciplinaId &&
          DateFormat('yyyy-MM-dd').format(dataRegistro) == dataFormatada;
    });

    return {
      for (var item in registrosDoDia) item['alunoTurmaId']: item['presente'],
    };
  }

  Future<void> _salvarFrequencia() async {
    setState(() => _salvando = true);
    final scaffoldMessenger = ScaffoldMessenger.of(context);

    final List<Map<String, dynamic>> payload = _frequencia.entries
        .where((entry) => entry.value != null)
        .map((entry) {
          return {
            'alunoTurmaId': entry.key,
            'turmaId': _turmaInfo!.turmaId,
            'disciplinaId': _turmaInfo!.disciplinaId,
            'presente': entry.value,
            'data': _dataSelecionada!.toIso8601String(),
          };
        })
        .toList();

    final isOnline = !(await Connectivity().checkConnectivity()).contains(
      ConnectivityResult.none,
    );

    try {
      if (isOnline) {
        await _enviarParaApi(payload);
        scaffoldMessenger.showSnackBar(
          const SnackBar(
            content: Text('Frequência enviada com sucesso!'),
            backgroundColor: Colors.green,
          ),
        );
      } else {
        await _salvarLocalmente(payload);
        scaffoldMessenger.showSnackBar(
          const SnackBar(
            content: Text('Offline. Frequência salva para enviar depois.'),
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

  Future<void> _enviarParaApi(List<Map<String, dynamic>> payload) async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');
    final apiUrl = Constants.API_URL;

    final response = await http.post(
      Uri.parse('$apiUrl/frequencia/registrar-lote'),
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

  Future<void> _salvarLocalmente(List<Map<String, dynamic>> payload) async {
    final prefs = await SharedPreferences.getInstance();
    final String? registrosJson = prefs.getString('frequencias_pendentes');
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

    todosRegistros.addAll(payload);

    await prefs.setString('frequencias_pendentes', jsonEncode(todosRegistros));
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
      bottomNavigationBar: (_dataSelecionada == null || _frequencia.isEmpty)
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
          const Icon(Icons.check_circle_outline, color: Colors.white, size: 28),
          const SizedBox(width: 12),
          const Text(
            'Registrar Frequência',
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
    if (_carregandoFrequencia) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_frequencia.isEmpty) {
      return const Center(child: Text('Nenhum aluno para exibir.'));
    }

    final dataDaAula = _dataSelecionada!;
    final dataDaAulaSemHora = DateTime(
      dataDaAula.year,
      dataDaAula.month,
      dataDaAula.day,
    );

    final alunosFiltrados = _turmaInfo!.alunos.where((aluno) {
      final dataEntradaSemHora = DateTime(
        aluno.entradaNaTurma.year,
        aluno.entradaNaTurma.month,
        aluno.entradaNaTurma.day,
      );

      final aposEntrada = !dataDaAulaSemHora.isBefore(dataEntradaSemHora);

      bool antesSaida = true;
      if (aluno.saidaDaTurma != null) {
        final dataSaidaSemHora = DateTime(
          aluno.saidaDaTurma!.year,
          aluno.saidaDaTurma!.month,
          aluno.saidaDaTurma!.day,
        );
        antesSaida = !dataDaAulaSemHora.isAfter(dataSaidaSemHora);
      }

      return aposEntrada && antesSaida;
    }).toList();

    if (alunosFiltrados.isEmpty) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(16.0),
          child: Text(
            'Nenhum aluno ativo encontrado para esta data.',
            textAlign: TextAlign.center,
          ),
        ),
      );
    }

    return ListView.builder(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      itemCount: alunosFiltrados.length,
      itemBuilder: (context, index) {
        final aluno = alunosFiltrados[index];
        final status = _frequencia[aluno.alunoTurmaId];

        return CheckboxListTile(
          title: Text(aluno.pessoaNome),
          value: status,
          tristate: true,
          onChanged: (bool? newValue) {
            setState(() {
              if (status == null) {
                _frequencia[aluno.alunoTurmaId] = true;
              } else if (status == true) {
                _frequencia[aluno.alunoTurmaId] = false;
              } else {
                _frequencia[aluno.alunoTurmaId] = null;
              }
            });
          },
          secondary: Icon(
            status == true
                ? Icons.check_circle
                : (status == false
                      ? Icons.cancel
                      : Icons.remove_circle_outline),
            color: status == true
                ? Colors.green
                : (status == false ? Colors.red : Colors.grey),
          ),
        );
      },
    );
  }

  Widget _buildBottomBar() {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: ElevatedButton.icon(
        onPressed: _salvando ? null : _salvarFrequencia,
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
        label: const Text('SALVAR FREQUÊNCIA'),
        style: ElevatedButton.styleFrom(
          backgroundColor: const Color.fromARGB(255, 0, 53, 77),
          foregroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(vertical: 16),
        ),
      ),
    );
  }
}
