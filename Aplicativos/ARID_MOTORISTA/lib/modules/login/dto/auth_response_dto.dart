import 'usuario_dto.dart';

class AuthResponseDTO {
  final String token;
  final UsuarioDTO usuario;

  AuthResponseDTO({
    required this.token,
    required this.usuario,
  });

  factory AuthResponseDTO.fromJson(Map<String, dynamic> json) {
    final data = json['data'];

    return AuthResponseDTO(
      token: data['token'],
      usuario: UsuarioDTO.fromJson(data['usuarioLogado']),
    );
  }

  Map<String, dynamic> toJson() => {
    'token': token,
    'usuario': usuario.toJson(),
  };
}
