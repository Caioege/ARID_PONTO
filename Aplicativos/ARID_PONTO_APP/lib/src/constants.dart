const String API_URL = "https://frequencia.arid.com.br/api/app";
// const String API_URL = "http://192.168.1.5:7798/api/app";

class Aluno {
  final int alunoTurmaId;
  final String pessoaNome;
  final DateTime entradaNaTurma;
  final DateTime? saidaDaTurma;
  final int situacao;

  Aluno({
    required this.alunoTurmaId,
    required this.pessoaNome,
    required this.entradaNaTurma,
    required this.saidaDaTurma,
    required this.situacao,
  });

  factory Aluno.fromJson(Map<String, dynamic> json) {
    return Aluno(
      alunoTurmaId: json['alunoTurmaId'],
      pessoaNome: json['pessoaNome'],
      entradaNaTurma: DateTime.parse(json['entradaNaTurma']),
      saidaDaTurma: json['saidaDaTurma'] != null
          ? DateTime.parse(json['saidaDaTurma'])
          : null,
      situacao: json['situacao'],
    );
  }
}

class TurmaInfo {
  final int turmaId;
  final int disciplinaId;
  final String turmaDescricao;
  final DateTime inicioTurma;
  final DateTime fimTurma;
  final List<bool> diasDeAula;
  final List<Aluno> alunos;
  final List<ConfiguracaoConteudo> configuracaoConteudo;

  TurmaInfo({
    required this.turmaId,
    required this.disciplinaId,
    required this.turmaDescricao,
    required this.inicioTurma,
    required this.fimTurma,
    required this.diasDeAula,
    required this.alunos,
    required this.configuracaoConteudo,
  });

  factory TurmaInfo.fromMap(Map<String, dynamic> map) {
    List<dynamic> alunosJson = map['alunos'] ?? [];
    List<ConfiguracaoConteudo> configs = [];

    final configData = map['configuracaoConteudo'];
    if (configData is List) {
      configs = configData
          .map((c) => ConfiguracaoConteudo.fromMap(c as Map<String, dynamic>))
          .toList();
    } else if (configData is Map) {
      configs.add(
        ConfiguracaoConteudo.fromMap(configData as Map<String, dynamic>),
      );
    }

    configs.sort((a, b) => a.ordem.compareTo(b.ordem));

    return TurmaInfo(
      turmaId: map['turmaId'],
      disciplinaId: map['disciplinaId'],
      turmaDescricao: map['turmaDescricao'],
      inicioTurma: DateTime.parse(map['inicioTurma']),
      fimTurma: DateTime.parse(map['fimTurma']),
      diasDeAula: [
        map['temAulaSegunda'] ?? false,
        map['temAulaTerca'] ?? false,
        map['temAulaQuarta'] ?? false,
        map['temAulaQuinta'] ?? false,
        map['temAulaSexta'] ?? false,
        map['temAulaSabado'] ?? false,
        map['temAulaDomingo'] ?? false,
      ],
      alunos: alunosJson.map((json) => Aluno.fromJson(json)).toList(),
      configuracaoConteudo: configs,
    );
  }
}

class ConfiguracaoConteudo {
  final int ordem;
  final String titulo;
  final bool obrigatorio;
  final int quantidadeDeCaracteresMaxima;

  ConfiguracaoConteudo({
    required this.ordem,
    required this.titulo,
    required this.obrigatorio,
    required this.quantidadeDeCaracteresMaxima,
  });

  factory ConfiguracaoConteudo.fromMap(Map<String, dynamic> map) {
    return ConfiguracaoConteudo(
      ordem: map['ordem'] ?? 0,
      titulo: map['titulo'] ?? 'Campo sem título',
      obrigatorio: map['obrigatorio'] ?? false,
      quantidadeDeCaracteresMaxima: map['quantidadeDeCaracteresMaxima'] ?? 255,
    );
  }
}
