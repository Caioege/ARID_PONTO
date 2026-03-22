import 'package:dio/dio.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

String extrairMensagemErro(Object error) {
  if (error is DioException) {
    final data = error.response?.data;

    if (data is Map && data['message'] is String) {
      return data['message'];
    }

    if (data is Map && data['erro'] is String) {
      return data['erro'];
    }

    return 'Não foi possível comunicar com o servidor.';
  }

  return 'Ocorreu um erro inesperado. Tente novamente.';
}

int compararNomesNaturais(String a, String b) {
  final regex = RegExp(r'(\D+)(\d+)?');

  final matchA = regex.firstMatch(a.trim());
  final matchB = regex.firstMatch(b.trim());

  if (matchA == null || matchB == null) {
    return a.compareTo(b);
  }

  final textoA = matchA.group(1)!.trim().toLowerCase();
  final textoB = matchB.group(1)!.trim().toLowerCase();

  // compara o texto (Aluno)
  final textoCompare = textoA.compareTo(textoB);
  if (textoCompare != 0) return textoCompare;

  // compara o número (1, 2, 10...)
  final numeroA = int.tryParse(matchA.group(2) ?? '');
  final numeroB = int.tryParse(matchB.group(2) ?? '');

  if (numeroA == null || numeroB == null) return 0;

  return numeroA.compareTo(numeroB);
}

Widget campoSelecao({
  required ThemeData theme,
  required String titulo,
  required String? valor,
  required IconData icone,
  VoidCallback? onTap,
  bool obrigatorio = false,
}) {
  return Material(
    color: Colors.white,
    borderRadius: BorderRadius.circular(14),
    elevation: 2,
    shadowColor: Colors.black.withValues(alpha: .62),
    child: InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(14),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
        child: Row(
          children: [
            Icon(icone, color: theme.primaryColor),
            const SizedBox(width: 10),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Text(
                        titulo,
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: valor == null
                              ? theme.primaryColor
                              : Colors.black87,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                      if (obrigatorio) ...[
                        const SizedBox(width: 4),
                        Container(
                          width: 6,
                          height: 6,
                          decoration: BoxDecoration(
                            color: Colors.red,
                            shape: BoxShape.circle,
                          ),
                        ),
                      ],
                    ],
                  ),
                  const SizedBox(height: 2),
                  Text(
                    valor ?? 'Toque para selecionar',
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: valor == null ? Colors.grey : theme.primaryColor,
                    ),
                  ),
                ],
              ),
            ),
            if (onTap != null)
              Icon(Icons.keyboard_arrow_down, color: theme.primaryColor),
          ],
        ),
      ),
    ),
  );
}

void abrirSheet(
  BuildContext context,
  ThemeData theme, {
  required String titulo,
  required List<String> itens,
  required Function(String?) onSelecionar,
  String? selecionadoAtual,
}) {
  showModalBottomSheet(
    context: context,
    backgroundColor: Colors.transparent,
    builder: (_) {
      return Container(
        height: MediaQuery.of(context).size.height * 0.5,
        decoration: BoxDecoration(
          color: Colors.grey.shade50,
          borderRadius: const BorderRadius.vertical(top: Radius.circular(28)),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: .25),
              blurRadius: 30,
            ),
          ],
        ),
        child: Column(
          children: [
            const SizedBox(height: 12),
            Container(
              width: 44,
              height: 5,
              decoration: BoxDecoration(
                color: Colors.grey[400],
                borderRadius: BorderRadius.circular(10),
              ),
            ),
            const SizedBox(height: 18),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Row(
                children: [
                  Expanded(
                    child: Divider(thickness: 1.5, color: Colors.grey.shade300),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    titulo,
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                      color: theme.primaryColor,
                      fontSize: 17,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Divider(thickness: 1.5, color: Colors.grey.shade300),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),
            Expanded(
              child: ListView(
                padding: const EdgeInsets.symmetric(horizontal: 20),
                children: itens.map((item) {
                  final selecionado = item == selecionadoAtual;

                  return GestureDetector(
                    onTap: () {
                      if (selecionado) {
                        onSelecionar(null);
                      } else {
                        onSelecionar(item);
                      }
                      Navigator.pop(context);
                    },
                    child: Container(
                      margin: const EdgeInsets.only(bottom: 14),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(14),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withValues(
                              alpha: selecionado ? .10 : .08,
                            ),
                            blurRadius: 5,
                            offset: const Offset(0, 2),
                          ),
                        ],
                      ),
                      child: AnimatedContainer(
                        duration: const Duration(milliseconds: 220),
                        padding: const EdgeInsets.symmetric(
                          horizontal: 18,
                          vertical: 16,
                        ),
                        decoration: BoxDecoration(
                          color: selecionado
                              ? theme.primaryColor.withValues(alpha: .25)
                              : Colors.white,
                          borderRadius: BorderRadius.circular(14),
                        ),
                        child: Row(
                          children: [
                            Expanded(
                              child: Text(
                                item,
                                style: theme.textTheme.titleSmall?.copyWith(
                                  fontWeight: FontWeight.w600,
                                  color: selecionado
                                      ? theme.primaryColor
                                      : Colors.black,
                                ),
                              ),
                            ),
                            if (selecionado)
                              Icon(
                                Icons.check_circle,
                                color: theme.primaryColor,
                              ),
                          ],
                        ),
                      ),
                    ),
                  );
                }).toList(),
              ),
            ),
            const SizedBox(height: 16),
          ],
        ),
      );
    },
  );
}

void abrirPickerData(BuildContext context, ThemeData theme) {
  DateTime dataTemp = DateTime.now();

  showModalBottomSheet(
    context: context,
    backgroundColor: Colors.transparent,
    builder: (_) {
      return Container(
        height: 350,
        decoration: BoxDecoration(
          color: Colors.grey.shade50,
          borderRadius: const BorderRadius.vertical(top: Radius.circular(28)),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: .25),
              blurRadius: 30,
            ),
          ],
        ),
        child: Column(
          children: [
            const SizedBox(height: 12),
            Container(
              width: 44,
              height: 5,
              decoration: BoxDecoration(
                color: Colors.grey[400],
                borderRadius: BorderRadius.circular(10),
              ),
            ),
            const SizedBox(height: 18),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Row(
                children: [
                  Expanded(
                    child: Divider(thickness: 1.5, color: Colors.grey.shade300),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    'Data',
                    style: theme.textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                      color: theme.primaryColor,
                      fontSize: 17,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Divider(thickness: 1.5, color: Colors.grey.shade300),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 10),
            Container(
              decoration: BoxDecoration(
                color: Colors.grey.shade50,
                borderRadius: BorderRadius.circular(18),
              ),
              child: SizedBox(
                height: 170,
                child: CupertinoTheme(
                  data: CupertinoThemeData(
                    brightness: Brightness.light,
                    primaryColor: theme.primaryColor,
                    textTheme: CupertinoTextThemeData(
                      dateTimePickerTextStyle: TextStyle(
                        fontSize: 16,
                        color: Colors.black87,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  child: CupertinoDatePicker(
                    mode: CupertinoDatePickerMode.date,
                    showDayOfWeek: true,
                    initialDateTime: dataTemp,
                    minimumDate: DateTime(2000),
                    maximumDate: DateTime.now(),
                    onDateTimeChanged: (DateTime novaData) {
                      dataTemp = novaData;
                    },
                  ),
                ),
              ),
            ),
            SizedBox(width: 10),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 10, 20, 0),
              child: SizedBox(
                width: double.infinity,
                height: 46,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: theme.primaryColor,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16),
                    ),
                    elevation: 2,
                  ),
                  onPressed: () {
                    Navigator.pop(context, dataTemp);
                  },
                  child: const Text(
                    'Ok',
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                      color: Colors.white,
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      );
    },
  );
}
