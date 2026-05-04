using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeEscala : IServico<Escala>, IDisposable
    {
        CicloDaEscala ObtenhaCiclo(int id);
        EscalaDoServidor ObtenhaEscalaDoServidor(int id);

        void AdicioneOuAltereCiclo(CicloDaEscala cicloDaEscala);
        void RemoverCiclo(int id);
        void AdicioneOuAltereEscalaDoServidor(
            EscalaDoServidor escalaDoServidor,
            bool ciclica);
        void RemoverEscalaServidor(int id);

        void RemoverEscala(int id);
        (int Total, System.Collections.Generic.List<LogAuditoriaEscala> Itens) ObtenhaAuditoriaPaginada(System.Linq.Expressions.Expression<Func<LogAuditoriaEscala, bool>> filtro, int pagina, int quantidadeDeItens);
    }
}