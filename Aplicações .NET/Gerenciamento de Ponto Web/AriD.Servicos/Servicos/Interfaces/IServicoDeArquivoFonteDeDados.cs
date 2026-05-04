using AriD.BibliotecaDeClasses.DTO;

namespace AriD.Servicos.Servicos.Interfaces
{
    public interface IServicoDeArquivoFonteDeDados
    {
        int ImportarArquivoAFD(
            int equipamentoId,
            int ultimoNsrInformado,
            SessaoDTO sessaoDTO,
            Stream arquivoStream);
    }
}