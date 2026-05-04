import 'dart:async';

import 'package:arid_rastreio/modules/motorista/rotas/dto/rota_chat_mensagem_dto.dart';
import 'package:arid_rastreio/modules/motorista/rotas/service/motorista_rotas_service.dart';
import 'package:flutter/material.dart';

class RotaChatPage extends StatefulWidget {
  final int rotaExecucaoId;
  final String rotaDescricao;

  const RotaChatPage({
    super.key,
    required this.rotaExecucaoId,
    required this.rotaDescricao,
  });

  @override
  State<RotaChatPage> createState() => _RotaChatPageState();
}

class _RotaChatPageState extends State<RotaChatPage> {
  final MotoristaRotasService _service = MotoristaRotasService();
  final TextEditingController _mensagemController = TextEditingController();
  final ScrollController _scrollController = ScrollController();

  RotaChatResumoDTO? _chat;
  bool _carregando = true;
  bool _atualizando = false;
  bool _enviando = false;
  String? _erro;
  Timer? _pollingTimer;

  @override
  void initState() {
    super.initState();
    _carregarChat();
    _pollingTimer = Timer.periodic(const Duration(seconds: 5), (_) {
      _carregarChat(silencioso: true);
    });
  }

  @override
  void dispose() {
    _pollingTimer?.cancel();
    _mensagemController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  Future<void> _carregarChat({bool silencioso = false}) async {
    if (_atualizando) return;

    _atualizando = true;
    if (!silencioso) {
      setState(() {
        _carregando = true;
        _erro = null;
      });
    }

    try {
      final chat = await _service.obterChatRota(widget.rotaExecucaoId);
      if (!mounted) return;
      setState(() {
        _chat = chat;
        _carregando = false;
        _erro = null;
      });
      _rolarParaFim();
    } catch (e) {
      if (!mounted) return;
      if (silencioso) return;
      setState(() {
        _erro = e.toString();
        _carregando = false;
      });
    } finally {
      _atualizando = false;
    }
  }

  Future<void> _enviar() async {
    final texto = _mensagemController.text.trim();
    if (texto.isEmpty || _chat?.finalizada == true || _enviando) return;

    setState(() => _enviando = true);

    try {
      await _service.enviarMensagemChatRota(
        rotaExecucaoId: widget.rotaExecucaoId,
        mensagem: texto,
      );
      _mensagemController.clear();
      await _carregarChat();
    } finally {
      if (mounted) setState(() => _enviando = false);
    }
  }

  void _rolarParaFim() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!_scrollController.hasClients) return;
      _scrollController.jumpTo(_scrollController.position.maxScrollExtent);
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final finalizada = _chat?.finalizada == true;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Chat da rota'),
        actions: [
          IconButton(
            onPressed: _carregarChat,
            icon: const Icon(Icons.refresh),
            tooltip: 'Atualizar',
          ),
        ],
      ),
      body: Column(
        children: [
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(14),
            color: theme.primaryColor.withValues(alpha: .08),
            child: Text(
              widget.rotaDescricao,
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
          ),
          if (finalizada)
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(12),
              color: Colors.amber.withValues(alpha: .18),
              child: const Text(
                'Esta rota foi finalizada. O chat está disponível somente para consulta.',
              ),
            ),
          Expanded(child: _conteudoMensagens(theme)),
          SafeArea(
            top: false,
            child: Padding(
              padding: const EdgeInsets.all(12),
              child: Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _mensagemController,
                      minLines: 1,
                      maxLines: 4,
                      maxLength: 1000,
                      enabled: !finalizada && !_enviando,
                      decoration: const InputDecoration(
                        hintText: 'Digite uma mensagem',
                        border: OutlineInputBorder(),
                        counterText: '',
                      ),
                    ),
                  ),
                  const SizedBox(width: 8),
                  SizedBox(
                    height: 48,
                    child: ElevatedButton(
                      onPressed: finalizada || _enviando ? null : _enviar,
                      child: _enviando
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Icon(Icons.send),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _conteudoMensagens(ThemeData theme) {
    if (_carregando) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_erro != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Text(
            'Não foi possível carregar o chat.\n$_erro',
            textAlign: TextAlign.center,
          ),
        ),
      );
    }

    final mensagens = _chat?.mensagens ?? [];
    if (mensagens.isEmpty) {
      return const Center(child: Text('Nenhuma mensagem enviada nesta rota.'));
    }

    return ListView.builder(
      controller: _scrollController,
      padding: const EdgeInsets.all(12),
      itemCount: mensagens.length,
      itemBuilder: (_, index) {
        final mensagem = mensagens[index];
        final enviadaPeloApp = mensagem.origem == 2;
        return Align(
          alignment: enviadaPeloApp ? Alignment.centerRight : Alignment.centerLeft,
          child: Container(
            constraints: const BoxConstraints(maxWidth: 310),
            margin: const EdgeInsets.only(bottom: 8),
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: enviadaPeloApp
                  ? theme.primaryColor.withValues(alpha: .16)
                  : Colors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: Colors.black.withValues(alpha: .08)),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  mensagem.remetenteNome.isEmpty
                      ? mensagem.origemDescricao
                      : mensagem.remetenteNome,
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(mensagem.mensagem),
                const SizedBox(height: 4),
                Align(
                  alignment: Alignment.centerRight,
                  child: Text(
                    mensagem.dataHoraEnvioFormatada,
                    style: TextStyle(fontSize: 11, color: Colors.grey[700]),
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}
