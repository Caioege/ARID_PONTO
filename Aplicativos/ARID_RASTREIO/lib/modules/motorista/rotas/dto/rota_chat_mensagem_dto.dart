class RotaChatMensagemDTO {
  final int id;
  final int rotaExecucaoId;
  final int origem;
  final String origemDescricao;
  final String remetenteNome;
  final String mensagem;
  final String dataHoraEnvioFormatada;

  RotaChatMensagemDTO({
    required this.id,
    required this.rotaExecucaoId,
    required this.origem,
    required this.origemDescricao,
    required this.remetenteNome,
    required this.mensagem,
    required this.dataHoraEnvioFormatada,
  });

  factory RotaChatMensagemDTO.fromJson(Map<String, dynamic> json) {
    return RotaChatMensagemDTO(
      id: json['id'] ?? 0,
      rotaExecucaoId: json['rotaExecucaoId'] ?? 0,
      origem: json['origem'] ?? 0,
      origemDescricao: json['origemDescricao'] ?? '',
      remetenteNome: json['remetenteNome'] ?? '',
      mensagem: json['mensagem'] ?? '',
      dataHoraEnvioFormatada: json['dataHoraEnvioFormatada'] ?? '',
    );
  }
}

class RotaChatResumoDTO {
  final int rotaExecucaoId;
  final int rotaId;
  final String rotaDescricao;
  final bool finalizada;
  final int naoLidasSistema;
  final int naoLidasAplicativo;
  final List<RotaChatMensagemDTO> mensagens;

  RotaChatResumoDTO({
    required this.rotaExecucaoId,
    required this.rotaId,
    required this.rotaDescricao,
    required this.finalizada,
    this.naoLidasSistema = 0,
    this.naoLidasAplicativo = 0,
    required this.mensagens,
  });

  factory RotaChatResumoDTO.fromJson(Map<String, dynamic> json) {
    return RotaChatResumoDTO(
      rotaExecucaoId: json['rotaExecucaoId'] ?? 0,
      rotaId: json['rotaId'] ?? 0,
      rotaDescricao: json['rotaDescricao'] ?? '',
      finalizada: json['finalizada'] ?? false,
      naoLidasSistema: json['naoLidasSistema'] ?? 0,
      naoLidasAplicativo: json['naoLidasAplicativo'] ?? 0,
      mensagens: ((json['mensagens'] ?? []) as List)
          .map((e) => RotaChatMensagemDTO.fromJson(Map<String, dynamic>.from(e)))
          .toList(),
    );
  }
}
