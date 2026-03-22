# Módulo Gestão Mobile

## O que é?
O **Gestão Mobile** é um módulo do sistema focado no gerenciamento e acompanhamento das operações de campo realizadas pelos motoristas da organização através do aplicativo mobile associado. Ele permite o cadastro de rotas, definição de veículos e motoristas responsáveis, estabelece pontos de parada (entregas ou coletas) e acompanha ativamente a localização geográfica em tempo real destes profissionais.

## Como usar?
A funcionalidade é controlada via permissões modulares. Siga os passos abaixo para utilizá-la na aplicação:
1. **Ativação**: O módulo não é visível por padrão. Para exibi-lo, um administrador deve editar a Organização correspondente e marcar a opção "Ativar Gestão Mobile?".
2. **Permissões**: Para acessar os menus, o usuário deverá ter recebido as permissões específicas do módulo (Visualizar, CadastrarOuAlterar, Excluir) em seu Perfil de Acesso, vinculadas às telas de Motorista, Veículo e Rotas.
3. **Manutenção de Cadastros**:
   - Utilize a tela de **Motoristas** para parear servidores/funcionários como motoristas ativos da plataforma.
   - Cadastre os **Veículos** que farão parte da frota, gerenciando os status (Disponível, Em Manutenção, etc.).
4. **Gerenciamento de Rotas**:
   - Na respectiva tela, crie a rota definindo o motorista, veículo, e adicione os **Pontos de Parada** contendo endereço e coordenada geográfica.
5. **Integração com o APP**: Ao usar o aplicativo e aceitar a rota, localizações geográficas contínuas (GPS)  serão enviadas para a API (via POST `/api/rota-app/receber-localizacao`) e persistidas internamente para histórico e plotagem em mapas.

## Por que foi desenvolvido?
Para trazer visibilidade e previsibilidade as operações de frota e entregas/visitas externas que ocorrem fora do escritório. Anteriormente, apenas o ponto do servidor era registrado, não havendo garantia de qual itinerário foi cumprido ou da localização atual. Com este módulo, a organização unifica Departamento Pessoal com Logística Básica num só lugar.

## Guias de Uso (Desenvolvimento e API)
- **Criação de Entidades**: Modificações no módulo giram em torno das tabelas `Rota`, `ParadaRota`, e `LocalizacaoRota`. O motor principal de persistência utiliza o repositório `IServico<T>`.
- **API Mobile**: 
  - A API para o aplicativo reside no Controller `RotaAppController`.
  - Envie pacotes para a rota de localização no formato JSON, utilizando a interface definida em `PostLocalizacaoRotaDTO` (`RotaId`, `Latitude`, `Longitude`, `DataHora`).
- **Scripts de Banco**: Mudanças estruturais na mecânica de Rotas devem ser mapeadas nos scripts SQL presentes dentro de `/ScriptsSQL/`.
