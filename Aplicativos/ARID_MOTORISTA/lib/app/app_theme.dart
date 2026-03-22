import 'package:flutter/material.dart';

class AppTheme {
  static ThemeData get light {
    return ThemeData(
      useMaterial3: true,
      colorSchemeSeed: const Color(0xFF0D1B2A),
      brightness: Brightness.light,
      appBarTheme: const AppBarTheme(
        centerTitle: true,
      ),
    );
  }
}
