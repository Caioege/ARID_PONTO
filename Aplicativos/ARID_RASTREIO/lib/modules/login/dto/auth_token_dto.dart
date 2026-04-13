class AuthTokenDTO {
  final String token;

  AuthTokenDTO({required this.token});

  factory AuthTokenDTO.fromJson(Map<String, dynamic> json) {
    final data = json['data'];

    return AuthTokenDTO(token: data['token']);
  }
}
