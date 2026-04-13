# 🚍 AriD Rastreio

Aplicativo mobile desenvolvido em Flutter para gestão operacional de motoristas, contemplando:

- ✔️ Autenticação
- ✔️ Checklist de saída
- ✔️ Execução de rotas
- ✔️ Confirmação de paradas
- ✔️ Rastreamento de localização em background
- ✔️ Encerramento de rota
- ✔️ Suporte a modo mock para desenvolvimento offline

---

## 📱 Objetivo do Projeto

O **AriD Rastreio** tem como objetivo permitir que motoristas:

1. Realizem checklist obrigatório antes de iniciar uma rota.
2. Iniciem uma execução de rota.
3. Confirmem paradas durante o percurso.
4. Tenham sua localização enviada automaticamente em segundo plano.
5. Finalizem a rota com registro das informações operacionais.

O aplicativo foi estruturado com foco em:

- Arquitetura organizada
- Separação de responsabilidades
- Uso de MobX para gerenciamento de estado
- Serviço em foreground para rastreamento contínuo

---

## 🧱 Arquitetura

O projeto está organizado em camadas:

```
lib/
 ├── core/
 │    ├── http/
 │    ├── auth/
 │    └── service/
 │
 ├── modules/
 │    ├── login/
 │    ├── motorista/
 │    │     ├── checklist/
 │    │     ├── rotas/
 │    │     ├── menu/
 │    │     └── splash/
 │
 ├── shared/
 │
 └── ioc/
```

### 📌 Padrões utilizados

- Service Layer para chamadas HTTP
- Controller com MobX
- Injeção de dependência via GetIt
- DTOs para comunicação com API
- Foreground Service para rastreamento

---

## 🔐 Autenticação

O login é realizado via API, retornando token para autenticação das demais requisições.

O token é armazenado localmente via `SessionManager`.

---

## 📋 Checklist

Antes de iniciar uma rota é obrigatório:

- Selecionar rota
- Selecionar veículo
- Salvar checklist

Somente após isso o botão de iniciar rota é habilitado.

---

## 🚦 Execução de Rota

Fluxo:

1. Motorista inicia rota
2. API retorna um `RotaExecucaoDTO`
3. ID da execução é salvo no Foreground Task
4. Serviço de localização é iniciado
5. Posições são enviadas automaticamente para a API

---

## 📍 Rastreamento em Background

O app utiliza:

- `flutter_foreground_task`
- `geolocator`

A cada atualização válida de localização:

- Valida precisão
- Valida velocidade
- Valida tempo da posição
- Envia para endpoint `/rotas/salvar-ponto`

Funciona mesmo com o app em segundo plano.

---

## 🛑 Encerramento de Rota

Ao encerrar:

- Envia todas as paradas com status
- Finaliza execução
- Interrompe serviço de rastreamento

---

## 🧪 Modo Mock

Para desenvolvimento offline:

No arquivo `.env`:

```
USE_FAKE_LOGIN=true
```

Isso permite:

- Simular login
- Simular rota
- Simular envio de localização
- Desenvolver sem depender da API

---

## 🛠 Tecnologias Utilizadas

- Flutter
- MobX
- Dio
- GetIt
- Geolocator
- Flutter Foreground Task
- Flutter DotEnv

---

## 🚀 Como rodar o projeto

```bash
flutter pub get
flutter run
```

---

## 🔒 Permissões Necessárias

Android:

- ACCESS_FINE_LOCATION
- ACCESS_COARSE_LOCATION
- ACCESS_BACKGROUND_LOCATION
- FOREGROUND_SERVICE
- POST_NOTIFICATIONS
