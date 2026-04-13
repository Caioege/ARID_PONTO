import 'package:arid_rastreio/modules/login/dto/usuario_dto.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class MotoristaPerfilPage extends StatefulWidget {
  final UsuarioDTO usuario;

  const MotoristaPerfilPage({super.key, required this.usuario});

  @override
  State<MotoristaPerfilPage> createState() => _MotoristaPerfilPageState();
}

class _MotoristaPerfilPageState extends State<MotoristaPerfilPage> {
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [colors.primary.withValues(alpha: .08), colors.surface],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 30),
          child: Column(
            children: [
              CircleAvatar(
                radius: 80,
                backgroundColor: colors.primary.withValues(alpha: .15),
                backgroundImage: widget.usuario.foto != null
                    ? NetworkImage(widget.usuario.foto!)
                    : null,
                child: widget.usuario.foto == null
                    ? Icon(Icons.person, size: 80, color: colors.primary)
                    : null,
              ),
              const SizedBox(height: 20),
              Text(
                widget.usuario.nome,
                style: theme.textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 30),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  color: colors.surface,
                  borderRadius: BorderRadius.circular(20),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: .05),
                      blurRadius: 12,
                      offset: const Offset(0, 6),
                    ),
                  ],
                ),
                child: Column(
                  children: [
                    _infoTile(
                      context,
                      icon: Icons.badge_outlined,
                      label: 'CPF',
                      value: widget.usuario.cpf,
                    ),
                    _divider(),
                    _infoTile(
                      context,
                      icon: Icons.cake_outlined,
                      label: 'Data de Nascimento',
                      value: widget.usuario.dataNascimento != null
                          ? DateFormat(
                              'dd/MM/yyyy',
                            ).format(widget.usuario.dataNascimento!)
                          : null,
                    ),
                    _divider(),
                    _infoTile(
                      context,
                      icon: Icons.email_outlined,
                      label: 'E-mail',
                      value: widget.usuario.email,
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _infoTile(
    BuildContext context, {
    required IconData icon,
    required String label,
    required String? value,
  }) {
    final colors = Theme.of(context).colorScheme;

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 14),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: colors.primary.withValues(alpha: .1),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(icon, color: colors.primary, size: 20),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: TextStyle(
                    fontSize: 12,
                    color: colors.onSurfaceVariant,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  value ?? 'Não informado',
                  style: const TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _divider() => const Divider(height: 1);
}

