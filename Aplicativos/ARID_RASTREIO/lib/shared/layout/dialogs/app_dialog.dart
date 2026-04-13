import 'package:flutter/material.dart';
import 'package:lottie/lottie.dart';

enum AppDialogType { sucesso, alerta, interrogacao }

Future<bool?> showAppDialog({
  required BuildContext context,
  required String titulo,
  required String mensagem,
  AppDialogType tipo = AppDialogType.sucesso,
  String textoBotao = 'OK',
}) async {
  final theme = Theme.of(context);

  IconData icon;
  Color cor;
  String? lottieAsset;

  switch (tipo) {
    case AppDialogType.sucesso:
      cor = Colors.green;
      lottieAsset = 'assets/json/sucesso.json';
      icon = Icons.check_circle;
      break;

    case AppDialogType.alerta:
      cor = Colors.red;
      lottieAsset = 'assets/json/alerta.json';
      icon = Icons.error;
      break;

    case AppDialogType.interrogacao:
      cor = Colors.orange;
      lottieAsset = 'assets/json/interrogacao.json';
      icon = Icons.question_mark;
      break;
  }

  return showDialog(
    context: context,
    barrierDismissible: false,

    builder: (_) => Dialog(
      backgroundColor: Colors.white,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (lottieAsset != null)
              SizedBox(
                height: 120,
                child: Lottie.asset(lottieAsset, repeat: false),
              )
            else
              Icon(icon, size: 80, color: cor),
            const SizedBox(height: 16),
            Text(
              titulo,
              textAlign: TextAlign.center,
              style: theme.textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              mensagem,
              textAlign: TextAlign.center,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: Colors.grey[700],
              ),
            ),
            const SizedBox(height: 24),
            if (tipo == AppDialogType.interrogacao)
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton(
                      onPressed: () => Navigator.pop(context, false),
                      child: const Text('Cancelar'),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: ElevatedButton(
                      onPressed: () => Navigator.pop(context, true),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: theme.primaryColor,
                      ),
                      child: const Text(
                        'Confirmar',
                        style: TextStyle(color: Colors.white),
                      ),
                    ),
                  ),
                ],
              )
            else
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () => Navigator.pop(context, true),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: theme.primaryColor,
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(14),
                    ),
                  ),
                  child: Text(
                    textoBotao,
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 15,
                      color: Colors.white,
                    ),
                  ),
                ),
              ),
          ],
        ),
      ),
    ),
  );
}
