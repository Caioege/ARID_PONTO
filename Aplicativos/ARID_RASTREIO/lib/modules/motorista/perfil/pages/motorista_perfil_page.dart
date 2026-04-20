import 'package:arid_rastreio/modules/login/dto/usuario_dto.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'dart:convert';

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
                backgroundImage: widget.usuario.foto != null && widget.usuario.foto!.isNotEmpty
                    ? MemoryImage(base64Decode(widget.usuario.foto!))
                    : null,
                child: widget.usuario.foto == null || widget.usuario.foto!.isEmpty
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
              if (widget.usuario.tipoAcesso?.toLowerCase() == 'motorista' && widget.usuario.numeroCnh != null && widget.usuario.numeroCnh!.isNotEmpty)
                _buildCnhCard(context, colors),
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

  Widget _buildCnhCard(BuildContext context, ColorScheme colors) {
    if (widget.usuario.numeroCnh == null) return const SizedBox.shrink();

    // Lógica de Vencimento
    int diasAteVencimento = 0;
    Color statusColor = Colors.green;
    String statusText = 'Válida';
    IconData statusIcon = Icons.check_circle;

    if (widget.usuario.validadeCnh != null) {
      final hoje = DateTime.now();
      final vencimento = widget.usuario.validadeCnh!;
      diasAteVencimento = vencimento.difference(hoje).inDays;

      if (diasAteVencimento < 0) {
        statusColor = Colors.red;
        statusText = 'Vencida há ${diasAteVencimento.abs()} dias';
        statusIcon = Icons.cancel;
      } else if (diasAteVencimento <= 30) {
        statusColor = Colors.orange;
        statusText = 'Vence em $diasAteVencimento dias';
        statusIcon = Icons.warning;
      } else {
        statusColor = Colors.green;
        statusText = 'Válida (vence em $diasAteVencimento dias)';
        statusIcon = Icons.check_circle;
      }
    }

    return Container(
      margin: const EdgeInsets.only(top: 20),
      width: double.infinity,
      decoration: BoxDecoration(
        color: colors.surface,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: statusColor.withValues(alpha: .3), width: 2),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: .05),
            blurRadius: 12,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            decoration: BoxDecoration(
              color: statusColor.withValues(alpha: .1),
              borderRadius: const BorderRadius.vertical(top: Radius.circular(18)),
            ),
            child: Row(
              children: [
                Icon(statusIcon, color: statusColor, size: 24),
                const SizedBox(width: 10),
                Expanded(
                  child: Text(
                    'Situação da CNH: $statusText',
                    style: TextStyle(
                      color: statusColor,
                      fontWeight: FontWeight.bold,
                      fontSize: 14,
                    ),
                  ),
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              children: [
                _infoTile(
                  context,
                  icon: Icons.drive_eta_outlined,
                  label: 'Número do Registro',
                  value: widget.usuario.numeroCnh,
                ),
                _divider(),
                Row(
                  children: [
                    Expanded(
                      child: _infoTile(
                        context,
                        icon: Icons.date_range_outlined,
                        label: '1º Emissão',
                        value: widget.usuario.emissaoCnh != null
                            ? DateFormat('dd/MM/yyyy').format(widget.usuario.emissaoCnh!)
                            : '---',
                      ),
                    ),
                    Expanded(
                      child: _infoTile(
                        context,
                        icon: Icons.event_busy_outlined,
                        label: 'Vencimento',
                        value: widget.usuario.validadeCnh != null
                            ? DateFormat('dd/MM/yyyy').format(widget.usuario.validadeCnh!)
                            : '---',
                      ),
                    ),
                  ],
                ),
                _divider(),
                _infoTile(
                  context,
                  icon: Icons.category_outlined,
                  label: 'Categoria',
                  value: widget.usuario.categoriaCnh,
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

