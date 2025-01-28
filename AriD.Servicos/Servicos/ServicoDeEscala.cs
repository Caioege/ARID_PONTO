using AriD.BibliotecaDeClasses.Entidades;
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


    }
}