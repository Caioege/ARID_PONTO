import 'package:mobx/mobx.dart';

part 'checklist_item_store.g.dart';

class ChecklistItemStore = ChecklistItemStoreBase with _$ChecklistItemStore;

abstract class ChecklistItemStoreBase with Store {
  final int id;
  final String descricao;

  @observable
  bool checked;

  ChecklistItemStoreBase({
    required this.id,
    required this.descricao,
    this.checked = false,
  });
}

extension ChecklistItemStoreMapper on ChecklistItemStore {
  static ChecklistItemStore fromJson(Map<String, dynamic> json) {
    return ChecklistItemStore(
      id: json['id'],
      descricao: json['descricao'],
      checked: json['checked'] ?? false,
    );
  }
}
