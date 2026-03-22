import 'package:flutter/material.dart';

class LoginForm extends StatefulWidget {
  final TextEditingController emailController;
  final TextEditingController passwordController;
  final bool hasError;

  const LoginForm({
    super.key,
    required this.emailController,
    required this.passwordController,
    required this.hasError,
  });

  @override
  State<LoginForm> createState() => _LoginFormState();
}

class _LoginFormState extends State<LoginForm>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<double> _userOpacity;
  late final Animation<Offset> _userOffset;
  late final Animation<double> _passwordOpacity;
  late final Animation<Offset> _passwordOffset;

  bool _obscurePassword = true;
  bool _userFieldFocused = false;
  bool _passwordFieldFocused = false;

  final FocusNode _userFocusNode = FocusNode();
  final FocusNode _passwordFocusNode = FocusNode();

  @override
  void initState() {
    super.initState();

    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 900),
    );

    _userOpacity = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.2, 0.5, curve: Curves.easeOut),
    );

    _userOffset = Tween(begin: const Offset(0, 0.3), end: Offset.zero).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.2, 0.5, curve: Curves.easeOut),
      ),
    );

    _passwordOpacity = CurvedAnimation(
      parent: _controller,
      curve: const Interval(0.4, 0.7, curve: Curves.easeOut),
    );

    _passwordOffset = Tween(begin: const Offset(0, 0.3), end: Offset.zero)
        .animate(
          CurvedAnimation(
            parent: _controller,
            curve: const Interval(0.4, 0.7, curve: Curves.easeOut),
          ),
        );

    _userFocusNode.addListener(() {
      setState(() => _userFieldFocused = _userFocusNode.hasFocus);
    });

    _passwordFocusNode.addListener(() {
      setState(() => _passwordFieldFocused = _passwordFocusNode.hasFocus);
    });

    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    _userFocusNode.dispose();
    _passwordFocusNode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        FadeTransition(
          opacity: _userOpacity,
          child: SlideTransition(
            position: _userOffset,
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(16),
                color: Theme.of(context).colorScheme.surface,
                border: Border.all(
                  color: _userFieldFocused
                      ? Theme.of(context).colorScheme.primary
                      : widget.hasError
                      ? Theme.of(context).colorScheme.error
                      : Theme.of(
                          context,
                        ).colorScheme.outline.withValues(alpha: 0.2),
                  width: _userFieldFocused ? 2 : 1,
                ),
                boxShadow: _userFieldFocused
                    ? [
                        BoxShadow(
                          color: Theme.of(
                            context,
                          ).colorScheme.primary.withValues(alpha: 0.1),
                          blurRadius: 8,
                          offset: const Offset(0, 4),
                        ),
                      ]
                    : null,
              ),
              child: TextField(
                controller: widget.emailController,
                focusNode: _userFocusNode,
                style: Theme.of(context).textTheme.bodyLarge,
                decoration: InputDecoration(
                  labelText: 'Usuário',
                  prefixIcon: Icon(
                    Icons.person,
                    color: _userFieldFocused
                        ? Theme.of(context).colorScheme.primary
                        : Theme.of(
                            context,
                          ).colorScheme.onSurface.withValues(alpha: 0.6),
                  ),
                  border: InputBorder.none,
                  contentPadding: const EdgeInsets.symmetric(
                    horizontal: 10,
                    vertical: 10,
                  ),
                  labelStyle: TextStyle(
                    color: _userFieldFocused
                        ? Theme.of(context).colorScheme.primary
                        : Theme.of(
                            context,
                          ).colorScheme.onSurface.withValues(alpha: 0.6),
                  ),
                ),
              ),
            ),
          ),
        ),
        const SizedBox(height: 12),
        FadeTransition(
          opacity: _passwordOpacity,
          child: SlideTransition(
            position: _passwordOffset,
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(16),
                color: Theme.of(context).colorScheme.surface,
                border: Border.all(
                  color: _passwordFieldFocused
                      ? Theme.of(context).colorScheme.primary
                      : widget.hasError
                      ? Theme.of(context).colorScheme.error
                      : Theme.of(
                          context,
                        ).colorScheme.outline.withValues(alpha: 0.2),
                  width: _passwordFieldFocused ? 2 : 1,
                ),
                boxShadow: _passwordFieldFocused
                    ? [
                        BoxShadow(
                          color: Theme.of(
                            context,
                          ).colorScheme.primary.withValues(alpha: 0.1),
                          blurRadius: 8,
                          offset: const Offset(0, 4),
                        ),
                      ]
                    : null,
              ),
              child: TextField(
                controller: widget.passwordController,
                focusNode: _passwordFocusNode,
                obscureText: _obscurePassword,
                style: Theme.of(context).textTheme.bodyLarge,
                decoration: InputDecoration(
                  labelText: 'Senha',
                  prefixIcon: Icon(
                    Icons.lock,
                    color: _passwordFieldFocused
                        ? Theme.of(context).colorScheme.primary
                        : Theme.of(
                            context,
                          ).colorScheme.onSurface.withValues(alpha: 0.6),
                  ),
                  suffixIcon: IconButton(
                    icon: Icon(
                      _obscurePassword
                          ? Icons.visibility_off_rounded
                          : Icons.visibility_rounded,
                      color: Theme.of(
                        context,
                      ).colorScheme.onSurface.withValues(alpha: 0.6),
                    ),
                    onPressed: () =>
                        setState(() => _obscurePassword = !_obscurePassword),
                  ),
                  border: InputBorder.none,
                  contentPadding: const EdgeInsets.symmetric(
                    horizontal: 10,
                    vertical: 10,
                  ),
                  labelStyle: TextStyle(
                    color: _passwordFieldFocused
                        ? Theme.of(context).colorScheme.primary
                        : Theme.of(
                            context,
                          ).colorScheme.onSurface.withValues(alpha: 0.6),
                  ),
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }
}
