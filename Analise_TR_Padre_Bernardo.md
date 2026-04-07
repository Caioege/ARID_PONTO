# Análise de Conformidade - TR Padre Bernardo

Este documento detalha a análise dos itens descritos no Termo de Referência (TR) "DOC - PADRE BERNADO - Atualizado.docx" em relação ao estado atual do projeto ARID_PONTO.

---

## 1. Itens Atendidos (Funcional)

Os itens abaixo foram identificados no código-fonte e estão operacionais conforme as especificações do TR.

1.  **Hospedagem e Arquitetura Cloud (SaaS)**: "A aplicação deverá rodar integralmente em nuvem pública e privada da empresa, com acesso via web browser... O software de gestão de ponto eletrônico deve ser 100% em nuvem."
    *   *Status*: **Atendido**. O sistema é uma aplicação web hospedada em nuvem, acessível via navegador.
2.  **Cadastros Ilimitados**: "Cadastros ilimitados: Operadores; Secretarias e setores; Servidores Públicos; Grupos de operadores; Lotações e Locais de trabalho; Cargos..."
    *   *Status*: **Atendido**. Não existem travas de licenciamento por quantidade de registros nas entidades `Servidor`, `Organizacao` ou `UnidadeOrganizacional`.
3.  **Log de Auditoria**: "O sistema deve registrar em 'Log de Auditoria' todas as alterações manuais feitas no ponto do servidor (quem alterou, quando alterou e a justificativa)."
    *   *Status*: **Atendido**. Implementado via entidades `LogAuditoriaPonto` e `LogAuditoriaEscala`.
4.  **Geofencing (Cerca Virtual)**: "A marcação de ponto por aplicativo com cerca virtual - o gestor desenha um raio de alcance (ex: 100 metros) ao redor do local de trabalho no mapa do sistema."
    *   *Status*: **Atendido**. Existe o campo `RaioDaCercaVirtualEmMetros` na `UnidadeOrganizacional` e validação no `ServicoDeAplicativo.cs`.
5.  **Relatório de Absenteísmo**: "Exibir resumo através de um relatório de absenteísmo."
    *   *Status*: **Atendido**. Implementado no `ServicoDeRelatorios.cs` (método `ObtenhaRelatorioDeAbsenteismo`).
6.  **Ações em Lote**: "Realizar ajustes nos cadastros dos servidores por demanda, por exemplo ativar o registro no aplicativo para todos os servidores."
    *   *Status*: **Atendido**. Funcionalidade de ações em lote presente no `FolhaDePontoController`.
7.  **Monitoramento de Frotas em Tempo Real**: "Poder acompanhar o veículo em rota em tempo real. Com as informações da viagem e motorista."
    *   *Status*: **Atendido**. Implementado via SignalR/Polling no `RotaController` e visualização em `Monitoramento.cshtml`.
8.  **Campos Médicos para Transporte**: "Cadastro de médicos do município com respectivo número de CRM e especialidade médica... Cadastro do paciente e acompanhante."
    *   *Status*: **Atendido**. Campos `NomePaciente` e `MedicoResponsavel` presentes na entidade `Rota`.
9.  **Comprovação por Foto no Mobile**: "O aplicativo deve capturar obrigatoriamente a foto do servidor no momento do registro."
    *   *Status*: **Atendido**. O `AppController` recepta a imagem e salva na pasta `wwwroot/img/registrosapp`.
10. **Observações no Cadastro do Servidor**: "Registrar uma observação no cadastro do servidor e possibilidade de imprimir relatório listando as observações cadastradas."
    *   *Status*: **Atendido**. A Ficha do Servidor (`RelatorioController.cs`) já imprime todas as observações ativas vinculadas ao cadastro.

---

## 2. Itens em Ajuste / Desenvolvimento

Itens que possuem base técnica iniciada, mas requerem finalização ou ajustes para atender plenamente ao TR.

1.  **Controle de Manutenção e Alertas (Frotas)**: "O sistema deve notificar quando será a próxima manutenção do veículo, por mensagem de push no sistema... Cadastro de controle de manutenção do veículo, km, troca de óleo, etc."
    *   *Ajuste*: A entidade `Veiculo` possui `QuilometragemAtual`, mas faltam campos para `ProximaTrocaOleo`. Além disso, o envio de **Push Notifications** via backend está em fase inicial (App pronto, mas API/Banco precisam de estrutura de tokens).
2.  **Modo Offline (Store and Forward)**: "Deve operar conectado ao servidor (online) e possuir autonomia para registrar ponto mesmo sem internet (offline), sincronizando posteriormente."
    *   *Status*: **Ajuste**. Conforme análise do TR, este requisito refere-se aos **Terminais Faciais** (Hardware) e **Rastreadores GPS**, que devem possuir memória interna para log local. O aplicativo mobile pode operar offline, mas a sincronização depende da lógica do App (Front-end).
3.  **Integração com Folha de Pagamento**: "Integração via API... para sincronização, captura e despacho de dados."
    *   *Ajuste*: Estrutura genérica existente; layouts específicos de Padre Bernardo aguardando definição da empresa de folha.

---

## 3. Itens Não Atendidos / Faltantes

Funcionalidades descritas no TR que não foram encontradas na implementação atual.

1.  **Liveness Check (Anti-Spoofing)**: "Algoritmo de reconhecimento facial com Detecção de Vivacidade (Liveness Check - anti-spoofing), capaz de distinguir um rosto real de uma foto ou vídeo em celular."
    *   *Análise*: Esta funcionalidade é complexa para implementação pura em C#. Recomenda-se o uso de um motor de biometria via API (ex: Azure Face API) para máxima conformidade com o TR. Ver seção 5 para detalhes de custos e complexidade.
2.  **Modo Offline (Store and Forward)**: "Deve operar conectado ao servidor (online) e possuir autonomia para registrar ponto mesmo sem internet (offline), sincronizando posteriormente."
    *   *Motivo*: Não identificada lógica de persistência local no aplicativo mobile para envio posterior em caso de queda de conexão.
3.  **Roteirização Inteligente**: "Módulo de Roteirização Inteligente: Criação de rotas otimizadas para o transporte escolar, com alertas de desvio de trajeto."
    *   *Motivo*: O sistema permite cadastrar paradas manuais, mas não possui motor de otimização de trajeto (estilo Google Routes Optimization).
4.  **Assinatura Eletrônica do Espelho**: "Acerto de ponto por meio de um processo que permita delegar o ajuste mediante justificativa... Consulta do espelho ponto pelos próprios servidores públicos."
    *   *Motivo*: Embora a consulta exista, o fluxo de assinatura digital (conforme Portaria 671) não foi identificado no código.

---

## 5. Análise Técnica: Liveness Check (Biometria Facial)

Para atender ao requisito de anti-spoofing no TR de Padre Bernardo, existem dois caminhos principais:

### Opção A: Azure Face API (Recomendado para o TR)
*   **Complexidade**: **Média**. Requer integração no servidor e uso do SDK mobile para capturar a vivacidade.
*   **Custo**: **Baixo (Pago por Uso)**. Aproximadamente R$ 0,05 por validação.
*   **Vantagem**: Atende 100% ao requisito de "distinguir rosto real de foto/vídeo" com alta precisão jurídica.

### Opção B: Validação Algorítmica no Aplicativo (Nativa)
*   **Complexidade**: **Alta**. Exige desenvolvimento de lógica de desafio (ex: "pisque os olhos") no App.
*   **Custo**: **Zero Recorrente**.
*   **Desvantagem**: Menos segura (pode ser burlada por vídeos) e pode não ser aceita em uma Prova de Conceito (POC) rigorosa que exija detecção de textura de pele/profundidade.

---

## 6. Dúvidas e Decisões Tomadas

> [!IMPORTANT]
> **Push Notifications**: Conforme solicitado, o backend será atualizado para permitir o registro de tokens dos dispositivos (`PushToken`), ativando a infraestrutura de alertas no servidor.

> [!NOTE]
> Os itens de **Service Desk** e **Números de Página** foram ignorados conforme instrução direta do usuário.
