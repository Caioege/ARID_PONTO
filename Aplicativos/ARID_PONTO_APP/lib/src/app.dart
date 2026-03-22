import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:app_arid_escolas/src/routes.dart';
import 'package:app_arid_escolas/src/screens/splash_screen.dart';
import 'package:google_fonts/google_fonts.dart';

class AppFrequenciaEscolar extends StatelessWidget {
  const AppFrequenciaEscolar({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Frequência Escolar',
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: const [Locale('pt', 'BR'), Locale('en', 'US')],
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.indigo),
        useMaterial3: true,
        textTheme: GoogleFonts.interTextTheme(),
      ),
      initialRoute: '/',
      routes: {'/': (context) => const SplashScreen(), ...routes},
    );
  }
}
