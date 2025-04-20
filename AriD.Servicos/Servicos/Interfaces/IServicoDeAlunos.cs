using AriD.BibliotecaDeClasses.DTO;
using AriD.BibliotecaDeClasses.Entidades;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeAlunos : IServico<Aluno>
    {
        List<CodigoDescricaoDTO> ObtenhaListaDeAlunosDisponiveisParaAlocacaoNaTurma(int turmaId);
        (int AnoLetivoMaisAntigo, int AnoLetivoMaisNovo) ObtenhaAnosLetivosDaRede(int redeDeEnsinoId);
        void AlocarAlunosNaTurma(int turmaId, DateTime entrada, List<int> alunosIds);
        void RemoverVinculoDeAluno(int alunoTurmaId);

        List<AlunoDiarioDTO> ListaDeAlunosParaDiario(
            int turmaId,
            DateTime inicio,
            DateTime fim);

        int? ObtenhaEscolaIdDoAluno(int alunoId);
        int ObtenhaMatriculaColetor(int escolaId);
    }
}