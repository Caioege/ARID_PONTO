import 'dart:convert';

class ValidacaoServer implements Exception {
  ValidacaoServer({required this.mensagem, this.sucesso = false});

  factory ValidacaoServer.fromMap(Map<String, dynamic> map) => ValidacaoServer(
    sucesso: map['sucesso'] ?? false,
    mensagem: map['mensagem'] ?? map['message'] ?? 'Erro inesperado',
  );

  factory ValidacaoServer.fromJson(String source) =>
      ValidacaoServer.fromMap(json.decode(source));

  bool sucesso;
  String mensagem;

  factory ValidacaoServer.erroGenerico() =>
      ValidacaoServer(sucesso: false, mensagem: 'Tente novamente');

  factory ValidacaoServer.erroConexao() => ValidacaoServer(
    sucesso: false,
    mensagem:
        'Verifique se seu Wi-Fi está conectado ou se seu pacote de dados está ativo!',
  );

  @override
  String toString() => mensagem;
}
