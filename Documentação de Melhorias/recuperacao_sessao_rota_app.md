# Recuperação local da sessão de rota no aplicativo

## O que é
Esta melhoria faz o aplicativo `ARID_RASTREIO` recuperar localmente os dados da rota em andamento ao abrir o app novamente.

O cache local preserva a execução da rota, a rota selecionada, o veículo selecionado, o checklist salvo e os pontos da rota exibidos na tela. Assim, o usuário volta para a tela preenchida como estava antes, sem aguardar primeiro a resposta do servidor.

## Como usar
1. Inicie uma rota normalmente no aplicativo.
2. Feche e abra o aplicativo novamente.
3. O app restaura a aba **Rotas** com rota, veículo, checklist e paradas preenchidos.
4. Depois da restauração visual, o app valida a rota ativa com o servidor em segundo plano quando houver conexão.

## Por que
A recuperação anterior dependia da consulta online ao servidor antes de preencher a tela. Em redes lentas, instáveis ou durante retomada do aplicativo, o motorista via a sessão sem os dados de rota, veículo e checklist preenchidos.

Ao carregar primeiro o cache local, a experiência fica mais rápida e resistente a falhas temporárias de conexão. A validação com o servidor continua existindo, mas deixa de bloquear a abertura da tela.

## Guias de uso
- O cache é salvo quando a rota é iniciada.
- O cache também é atualizado quando a rota é recuperada do servidor e quando há mudanças operacionais relevantes na execução.
- O cache é limpo quando a rota é encerrada ou quando a validação em segundo plano confirma que não existe mais rota ativa no servidor.
- Execuções offline recuperadas do cache não são descartadas apenas porque o servidor ainda não conhece a execução local.
- A validação em segundo plano usa `GET /api/rastreio-app/rotas/em-andamento`.
- O cache não substitui a sincronização oficial da rota; ele apenas hidrata a interface rapidamente.
