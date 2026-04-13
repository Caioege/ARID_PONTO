import 'package:flutter/material.dart';
import 'app_routes.dart';
import 'app_theme.dart';

class AppWidget extends StatelessWidget {
  const AppWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'AriD Rastreio',
      theme: AppTheme.light,
      routerConfig: AppRoutes.router,
      debugShowCheckedModeBanner: false,
    );
  }
}

