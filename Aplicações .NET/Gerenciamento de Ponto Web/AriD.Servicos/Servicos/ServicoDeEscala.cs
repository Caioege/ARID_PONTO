using AriD.BibliotecaDeClasses.Entidades;
using AriD.BibliotecaDeClasses.Enumeradores;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class ServicoDeEscala : Servico<Escala>, IServicoDeEscala
    {
        private readonly IRepositorio<Escala> _repositorioEscala;
        private readonly IRepositorio<CicloDaEscala> _repositorioCiclo;
        private readonly IRepositorio<EscalaDoServidor> _repositorioEscalaDoServidor;
        private readonly IRepositorio<LogAuditoriaEscala> _repositorioAuditoriaEscala;
        private readonly IUsuarioAtual _usuarioAtual;

        public ServicoDeEscala(
            IRepositorio<Escala> repositorioEscala,
            IRepositorio<CicloDaEscala> repositorioCiclo,
            IRepositorio<EscalaDoServidor> repositorioEscalaDoServidor,
            IRepositorio<LogAuditoriaEscala> repositorioAuditoriaEscala,
            IUsuarioAtual usuarioAtual)
        : base (repositorioEscala)
        {
            _repositorioEscala = repositorioEscala;
            _repositorioCiclo = repositorioCiclo;
            _repositorioEscalaDoServidor = repositorioEscalaDoServidor;
            _repositorioAuditoriaEscala = repositorioAuditoriaEscala;
            _usuarioAtual = usuarioAtual;
        }

        public void Dispose()
        {
        }

        public CicloDaEscala ObtenhaCiclo(int id)
            => _repositorioCiclo.Obtenha(id);

        public EscalaDoServidor ObtenhaEscalaDoServidor(int id)
            => _repositorioEscalaDoServidor.Obtenha(id);

        private void Auditar(
            int organizacaoId,
            int? escalaId,
            int? unidadeOrganizacionalId,
            string acao,
            string descricao)
        {
            _repositorioAuditoriaEscala.Add(new LogAuditoriaEscala
            {
                OrganizacaoId = organizacaoId,
                EscalaId = escalaId,
                UnidadeOrganizacionalId = unidadeOrganizacionalId,
                UsuarioNome = _usuarioAtual?.Nome ?? "Sistema",
                UsuarioId = _usuarioAtual?.UsuarioId,
                DataHora = DateTime.Now,
                Acao = acao,
                Descricao = descricao
            });

            _repositorioAuditoriaEscala.Commit();
        }

        public override int Adicionar(Escala entidade)
        {
            var id = base.Adicionar(entidade);
            Auditar(entidade.OrganizacaoId, id, entidade.UnidadeOrganizacionalId, "Inclusão de Escala", $"Escala '{entidade.Descricao}' criada.");
            return id;
        }

        public override void Atualizar(Escala entidade, bool commit = true)
        {
            base.Atualizar(entidade, commit);
            Auditar(entidade.OrganizacaoId, entidade.Id, entidade.UnidadeOrganizacionalId, "Alteração de Escala", $"Escala '{entidade.Descricao}' alterada.");
        }

        public void AdicioneOuAltereCiclo(CicloDaEscala cicloDaEscala)
        {
            try
            {
                if (cicloDaEscala.Id == 0)
                {
                    _repositorioCiclo.Add(cicloDaEscala);
                    Auditar(cicloDaEscala.OrganizacaoId, cicloDaEscala.EscalaId, null, "Inclusão de Ciclo", $"Ciclo {cicloDaEscala.Ciclo} adicionado à escala.");
                }
                else
                {
                    _repositorioCiclo.Atualizar(cicloDaEscala);
                    Auditar(cicloDaEscala.OrganizacaoId, cicloDaEscala.EscalaId, null, "Alteração de Ciclo", $"Ciclo {cicloDaEscala.Ciclo} alterado.");
                }

                _repositorioCiclo.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RemoverCiclo(int id)
        {
            try
            {
                var ciclo = _repositorioCiclo.Obtenha(id);
                if (ciclo.Escala.EscalaDoServidor.Any())
                    throw new ApplicationException("Não é possível remover o ciclo quando existem servidores vinculados a escala.");

                _repositorioCiclo.Remover(ciclo);
                Auditar(ciclo.OrganizacaoId, ciclo.EscalaId, null, "Exclusão de Ciclo", $"Ciclo {ciclo.Ciclo} removido da escala.");
                _repositorioCiclo.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AdicioneOuAltereEscalaDoServidor(
            EscalaDoServidor escalaDoServidor, 
            bool ciclica)
        {
            if (ciclica)
            {
                if (escalaDoServidor.DataFim.HasValue && escalaDoServidor.DataFim.Value <= escalaDoServidor.Data)
                    throw new ApplicationException("A data do primeiro ciclo deve ser menor que a data final da escala do servidor.");

                if (escalaDoServidor.Id == 0)
                {
                    _repositorioEscalaDoServidor.Add(escalaDoServidor);
                    Auditar(escalaDoServidor.OrganizacaoId, escalaDoServidor.EscalaId, null, "Inclusão de Servidor", $"Servidor Vínculo {escalaDoServidor.VinculoDeTrabalhoId} adicionado à escala (cíclica) com data limite {escalaDoServidor.DataFim}.");
                }
                else
                {
                    var escalaPersistida = _repositorioEscalaDoServidor.Obtenha(escalaDoServidor.Id);

                    escalaPersistida.Data = escalaDoServidor.Data;
                    escalaPersistida.DataFim = escalaDoServidor.DataFim;

                    _repositorioEscalaDoServidor.Atualizar(escalaPersistida);
                    Auditar(escalaPersistida.OrganizacaoId, escalaPersistida.EscalaId, null, "Alteração de Servidor", $"Ajuste na alocação do servidor Vínculo {escalaPersistida.VinculoDeTrabalhoId} na escala (cíclica).");
                }
            }
            else
            {
                if (escalaDoServidor.Id == 0)
                {
                    _repositorioCiclo.Add(escalaDoServidor.CicloDaEscala);
                    escalaDoServidor.CicloDaEscalaId = escalaDoServidor.CicloDaEscala.Id;
                    _repositorioEscalaDoServidor.Add(escalaDoServidor);
                    Auditar(escalaDoServidor.OrganizacaoId, escalaDoServidor.EscalaId, null, "Inclusão de Servidor", $"Servidor Vínculo {escalaDoServidor.VinculoDeTrabalhoId} adicionado à escala (mensal) a partir de {escalaDoServidor.Data}.");
                }
                else
                {
                    var escalaPersistida = _repositorioEscalaDoServidor.Obtenha(escalaDoServidor.Id);

                    escalaPersistida.CicloDaEscala.Entrada1 = escalaDoServidor.CicloDaEscala.Entrada1;
                    escalaPersistida.CicloDaEscala.Entrada2 = escalaDoServidor.CicloDaEscala.Entrada2;
                    escalaPersistida.CicloDaEscala.Entrada3 = escalaDoServidor.CicloDaEscala.Entrada3;
                    escalaPersistida.CicloDaEscala.Entrada4 = escalaDoServidor.CicloDaEscala.Entrada4;
                    escalaPersistida.CicloDaEscala.Entrada5 = escalaDoServidor.CicloDaEscala.Entrada5;

                    escalaPersistida.CicloDaEscala.Saida1 = escalaDoServidor.CicloDaEscala.Saida1;
                    escalaPersistida.CicloDaEscala.Saida2 = escalaDoServidor.CicloDaEscala.Saida2;
                    escalaPersistida.CicloDaEscala.Saida3 = escalaDoServidor.CicloDaEscala.Saida3;
                    escalaPersistida.CicloDaEscala.Saida4 = escalaDoServidor.CicloDaEscala.Saida4;
                    escalaPersistida.CicloDaEscala.Saida5 = escalaDoServidor.CicloDaEscala.Saida5;

                    _repositorioCiclo.Atualizar(escalaPersistida.CicloDaEscala);
                    Auditar(escalaPersistida.OrganizacaoId, escalaPersistida.EscalaId, null, "Alteração de Servidor", $"Ajuste nos horários da alocação do servidor Vínculo {escalaPersistida.VinculoDeTrabalhoId} na escala (mensal).");
                }
            }

            _repositorioEscalaDoServidor.Commit();
        }

        public void RemoverEscalaServidor(int id)
        {
            try
            {
                var escalaServidor = _repositorioEscalaDoServidor.Obtenha(id);

                if (escalaServidor.Escala.Tipo == eTipoDeEscala.Mensal)
                    _repositorioCiclo.Remover(escalaServidor.CicloDaEscala);

                _repositorioEscalaDoServidor.Remover(escalaServidor);
                Auditar(escalaServidor.OrganizacaoId, escalaServidor.EscalaId, null, "Exclusão de Servidor", $"Servidor Vínculo {escalaServidor.VinculoDeTrabalhoId} removido da escala.");
                _repositorioEscalaDoServidor.Commit();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public (int Total, List<LogAuditoriaEscala> Itens) ObtenhaAuditoriaPaginada(System.Linq.Expressions.Expression<Func<LogAuditoriaEscala, bool>> filtro, int pagina, int quantidadeDeItens)
        {
            return
            (
                _repositorioAuditoriaEscala.TotalDeItens(filtro),
                _repositorioAuditoriaEscala.ObtenhaListaPaginada(filtro, pagina, quantidadeDeItens, c => c.Id, false)
            );
        }

        public void RemoverEscala(int id)
        {
            try
            {
                var escala = Obtenha(id);
                if (escala == null)
                    throw new Exception("Escala não encontrada.");

                foreach (var servidor in escala.EscalaDoServidor)
                    _repositorioEscalaDoServidor.Remover(servidor);

                foreach (var ciclo in escala.Ciclos)
                    _repositorioCiclo.Remover(ciclo);

                _repositorioEscala.Remover(escala);
                Auditar(escala.OrganizacaoId, escala.Id, escala.UnidadeOrganizacionalId, "Exclusão de Escala", $"Escala '{escala.Descricao}' excluída junto com seus ciclos e servidores.");
                _repositorioEscala.Commit();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}