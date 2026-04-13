import 'package:flutter/material.dart';

class SuccessAnimation extends StatefulWidget {
  final VoidCallback onComplete;
  final Offset buttonPosition;
  final Size buttonSize;

  const SuccessAnimation({
    super.key,
    required this.onComplete,
    required this.buttonPosition,
    required this.buttonSize,
  });

  @override
  State<SuccessAnimation> createState() => _SuccessAnimationState();
}

class _SuccessAnimationState extends State<SuccessAnimation>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<double> _buttonExpandAnimation;
  late final Animation<double> _checkFadeAnimation;
  late final Animation<double> _checkScaleAnimation;
  late final Animation<double> _checkDrawAnimation;
  late final Animation<double> _finalExpandAnimation;
  late final Animation<double> _contentFadeAnimation;

  @override
  void initState() {
    super.initState();

    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 3000),
    );

    _buttonExpandAnimation = Tween(begin: 1.0, end: 13.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.0, 0.35, curve: Curves.easeInOut),
      ),
    );

    _checkFadeAnimation = Tween(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.25, 0.35, curve: Curves.easeOut),
      ),
    );

    _checkScaleAnimation =
        TweenSequence<double>([
          TweenSequenceItem(
            tween: Tween(
              begin: 0.0,
              end: 1.3,
            ).chain(CurveTween(curve: Curves.elasticOut)),
            weight: 70,
          ),
          TweenSequenceItem(
            tween: Tween(
              begin: 1.3,
              end: 1.0,
            ).chain(CurveTween(curve: Curves.easeOut)),
            weight: 30,
          ),
        ]).animate(
          CurvedAnimation(
            parent: _controller,
            curve: const Interval(0.25, 0.55, curve: Curves.linear),
          ),
        );

    _checkDrawAnimation = Tween(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.30, 0.50, curve: Curves.easeOut),
      ),
    );

    _finalExpandAnimation = Tween(begin: 15.0, end: 50.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.60, 1.0, curve: Curves.easeIn),
      ),
    );

    _contentFadeAnimation = Tween(begin: 1.0, end: 0.0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: const Interval(0.65, 0.85, curve: Curves.easeOut),
      ),
    );

    _controller.forward().then((_) {
      if (mounted) {
        widget.onComplete();
      }
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, child) {
        final expandScale = _controller.value < 0.60
            ? _buttonExpandAnimation.value
            : _finalExpandAnimation.value;

        final buttonCenterX =
            widget.buttonPosition.dx + (widget.buttonSize.width / 2);
        final buttonCenterY =
            widget.buttonPosition.dy + (widget.buttonSize.height / 2);

        return Stack(
          children: [
            // Círculo expansível que começa no botão
            Positioned(
              left:
                  buttonCenterX - (expandScale * widget.buttonSize.height / 2),
              top: buttonCenterY - (expandScale * widget.buttonSize.height / 2),
              child: Container(
                width: expandScale * widget.buttonSize.height,
                height: expandScale * widget.buttonSize.height,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  gradient: LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: [
                      Theme.of(context).colorScheme.primary,
                      Theme.of(
                        context,
                      ).colorScheme.primary.withValues(alpha: 0.8),
                    ],
                  ),
                  boxShadow: [
                    BoxShadow(
                      color: Theme.of(
                        context,
                      ).colorScheme.primary.withValues(alpha: 0.3),
                      blurRadius: 40,
                      spreadRadius: 10,
                    ),
                  ],
                ),
              ),
            ),

            // Check mark no centro da tela
            if (_controller.value >= 0.25 && _controller.value < 0.65)
              Positioned.fill(
                child: Opacity(
                  opacity:
                      _checkFadeAnimation.value * _contentFadeAnimation.value,
                  child: Center(
                    child: Transform.scale(
                      scale: _checkScaleAnimation.value,
                      child: Container(
                        width: 100,
                        height: 100,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: Colors.white,
                          boxShadow: [
                            BoxShadow(
                              color: Colors.black.withValues(alpha: 0.2),
                              blurRadius: 20,
                              spreadRadius: 5,
                            ),
                          ],
                        ),
                        child: CustomPaint(
                          painter: _CheckMarkPainter(
                            _checkDrawAnimation.value,
                            Theme.of(context).colorScheme.primary,
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
              ),
          ],
        );
      },
    );
  }
}

class _CheckMarkPainter extends CustomPainter {
  final double progress;
  final Color color;

  _CheckMarkPainter(this.progress, this.color);

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..strokeWidth = 6
      ..strokeCap = StrokeCap.round
      ..style = PaintingStyle.stroke;

    final path = Path();

    final startX = size.width * 0.25;
    final startY = size.height * 0.5;
    final midX = size.width * 0.45;
    final midY = size.height * 0.7;
    final endX = size.width * 0.75;
    final endY = size.height * 0.3;

    path.moveTo(startX, startY);

    if (progress <= 0.5) {
      final currentProgress = progress / 0.5;
      path.lineTo(
        startX + (midX - startX) * currentProgress,
        startY + (midY - startY) * currentProgress,
      );
    } else {
      path.lineTo(midX, midY);
      final currentProgress = (progress - 0.5) / 0.5;
      path.lineTo(
        midX + (endX - midX) * currentProgress,
        midY + (endY - midY) * currentProgress,
      );
    }

    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(_CheckMarkPainter oldDelegate) =>
      oldDelegate.progress != progress;
}
