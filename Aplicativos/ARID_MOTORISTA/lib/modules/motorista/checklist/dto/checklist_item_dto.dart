class ChecklistItemDTO {
  final int id;
  final String descricao;
  bool checked;

  ChecklistItemDTO({
    required this.id,
    required this.descricao,
    this.checked = false,
  });

  factory ChecklistItemDTO.fromJson(Map<String, dynamic> json) {
    return ChecklistItemDTO(
      id: json['id'],
      descricao: json['descricao'],
      checked: json['checked'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {'id': id, 'descricao': descricao, 'checked': checked};
  }
}
