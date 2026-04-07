# AriD - Gerenciamento de Ponto e Controle Operacional

Bem-vindo ao repositório do **AriD Gerenciamento de Ponto**, uma plataforma robusta desenvolvida em **.NET / C#** para o controle eficiente de jornadas de trabalho, horas extras, gestão de frotas, escalas, banco de horas e muito mais.

---

## 🚀 Visão Geral

O sistema provê uma solução completa para gestão de ponto e controle operacional, incluindo:

- Registro e tratamento de ponto (inclusive mobile com geolocalização e liveness).
- Controle de Rotas e Frotas, além da gestão de motoristas.
- Cálculos avançados de horas extras, tolerâncias e saldos de banco de horas (inclusive regras de bônus).
- Exportações para folha de pagamento e relatórios gerenciais diversos.
- Auditoria de dados de espelho de ponto e ausências.

## 🛠️ Tecnologias Principais

- **Linguagem / Framework:** C#, .NET (ASP.NET Core / MVC)
- **Banco de Dados:** MySQL com Entity Framework (via `MySQLDBContext`)
- **Frontend:** HTML, CSS, JavaScript (Gulp / Webpack para gestão de assets)
- **Geração de Documentos:** Utilitários para criação e extração de PDFs.

## 📁 Estrutura do Projeto

Abaixo, apresentamos a organização dos principais diretórios da solução:

```text
c:\src\ARID_PONTO\
├── AriD.GerenciamentoDePonto/       # Projeto Principal (Web application)
│   ├── Controllers/                 # Controladores MVC e APIs
│   ├── Views/                       # Views do sistema (Razor - .cshtml)
│   ├── wwwroot/                     # Assets estáticos (Scripts, CSS, Imagens)
│   └── Program.cs                   # Ponto de entrada da aplicação
│
├── AriD.BibliotecaDeClasses/        # Biblioteca de Classes e Domínio
│   ├── Entidades/                   # Modelos de Domínio (ex: Servidor, Rota, RegistroDePonto)
│   ├── Enumeradores/                # Enums utilizados em toda a aplicação
│   └── ParametrosDeConsulta/        # Filtros de listagem e parâmetros de buscas
│
├── AriD.Servicos/                   # Camada de Regras de Negócio e Persistência
│   ├── Servicos/                    # Regras de Negócio (ex: ServicoRegistroDePonto, ServicoBonus)
│   ├── Repositorios/                # Camada de Acesso a Dados
│   └── DBContext/                   # Contexto principal do Banco de Dados (MySQLDBContext)
│
├── ScriptsSQL/                      # Scripts de Banco de Dados
│   └── *.sql                        # Scripts de migrações, atualizações e criação de banco
│
├── Documentação de Melhorias/       # Documentação Funcional e Tecnológica
│   └── *.md                         # Manuais das features desenvolvidas recentemente
│
├── PdfParser/ & Aplicativos/        # Utilitários e binários complementares
│
├── AGENTS.md                        # Instruções e diretrizes de IA para evolução e CI/CD
└── AriD.GerenciamentoDePonto.sln    # Solution do Visual Studio
```

### Detalhando os Módulos Centrais

- **`AriD.BibliotecaDeClasses`**: Aqui ficam armazenadas todas as **entidades** essenciais do sistema (arquitetura anemica/rica base). Nunca inclua lógicas que precisem acessar o banco aqui.
- **`AriD.Servicos`**: Concentra toda a inteligência e lógica da aplicação. Ao implementar novas regras, cálculos ou validações complexas, este é o local adequado.
- **`AriD.GerenciamentoDePonto`**: Trata apenas do fluxo de apresentação e roteamento de requisições.

## 📚 Documentação Adicional

Este repositório possui uma forte cultura de documentação de novas funcionalidades. Quando precisar entender o funcionamento de uma feature recente, não deixe de verificar a pasta `Documentação de Melhorias/`. Alguns documentos notáveis:
- Controle de Tolerâncias
- Relatórios de Absenteísmo
- Regras de Bônus
- Gestão Mobile e Auditorias

## ⚙️ Diretrizes para Desenvolvimento

1. **Scripts SQL Centralizados:** Qualquer atualização ou migração de banco não deve ser feita solta, e sim documentada e inserida em arquivos como `.sql` na pasta `ScriptsSQL/`.
2. **Atualização da Documentação:** Ao criar funcionalidades densas ou melhorar processos, sempre crie um artefato dentro de `Documentação de Melhorias/`.
3. **Build e Teste de Assets:** A aplicação faz uso de Gulp/Webpack, execute a instalação ou build via Node se mexer em arquivos JS ou SCSS/CSS.

---
> *Desenvolvido com o compromisso de simplificar a governança pública e privada de jornada e frota.*
