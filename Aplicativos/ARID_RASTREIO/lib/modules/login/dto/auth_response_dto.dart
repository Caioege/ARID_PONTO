import 'usuario_dto.dart';

class AuthResponseDTO {
  final String token;
  final UsuarioDTO usuario;

  AuthResponseDTO({
    required this.token,
    required this.usuario,
  });

  factory AuthResponseDTO.fromJson(Map<String, dynamic> json) {
    return AuthResponseDTO(
      token: json['token'],
      usuario: UsuarioDTO.fromJson(json['usuario']),
    );
  }

  Map<String, dynamic> toJson() => {
    'token': token,
    'usuario': usuario.toJson(),
  };
}
