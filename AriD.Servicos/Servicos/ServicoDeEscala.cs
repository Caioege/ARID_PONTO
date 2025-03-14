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

        public ServicoDeEscala(
            IRepositorio<Escala> repositorioEscala,
            IRepositorio<CicloDaEscala> repositorioCiclo,
            IRepositorio<EscalaDoServidor> repositorioEscalaDoServidor)
        : base (repositorioEscala)
        {
            _repositorioEscala = repositorioEscala;
            _repositorioCiclo = repositorioCiclo;
            _repositorioEscalaDoServidor = repositorioEscalaDoServidor;
        }

        public void Dispose()
        {
        }

        public CicloDaEscala ObtenhaCiclo(int id)
            => _repositorioCiclo.Obtenha(id);

        public EscalaDoServidor ObtenhaEscalaDoServidor(int id)
            => _repositorioEscalaDoServidor.Obtenha(id);

        public void AdicioneOuAltereCiclo(CicloDaEscala cicloDaEscala)
        {
            try
            {
                if (cicloDaEscala.Id == 0)
                    _repositorioCiclo.Add(cicloDaEscala);
                else
                    _repositorioCiclo.Atualizar(cicloDaEscala);

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
                    _repositorioEscalaDoServidor.Add(escalaDoServidor);
                else
                {
                    var escalaPersistida = _repositorioEscalaDoServidor.Obtenha(escalaDoServidor.Id);

                    escalaPersistida.Data = escalaDoServidor.Data;
                    escalaPersistida.DataFim = escalaDoServidor.DataFim;

                    _repositorioEscalaDoServidor.Atualizar(escalaPersistida);
                }
            }
            else
            {
                if (escalaDoServidor.Id == 0)
                {
                    _repositorioCiclo.Add(escalaDoServidor.CicloDaEscala);
                    escalaDoServidor.CicloDaEscalaId = escalaDoServidor.CicloDaEscala.Id;
                    _repositorioEscalaDoServidor.Add(escalaDoServidor);
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
                _repositorioEscalaDoServidor.Commit();
            }
            catch (Exception)
            {
                throw;
            }
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
                _repositorioEscala.Commit();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}