using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class ResultadoExportacaoFolhaPagamentoDTO
    {
        public string NomeArquivoExportacao { get; set; } = "";
        public byte[] BytesExportacao { get; set; } = Array.Empty<byte>();

        public int TotalVinculosConsiderados { get; set; }
        public int TotalExportaveis { get; set; }        // 100% fechados (passaram no filtro)
        public int TotalExportadosComEventos { get; set; } // realmente geraram ao menos 1 linha
        public int TotalIgnorados { get; set; }

        public List<ExportacaoResumoColaboradorDTO> ExportadosResumo { get; set; } = new();
        public List<ExportacaoIgnoradoDTO> Ignorados { get; set; } = new();
    }

    public class ExportacaoResumoColaboradorDTO
    {
        public int VinculoId { get; set; }
        public string Matricula { get; set; } = "";
        public string NomeServidor { get; set; } = "";

        // Resumo agrupado por código da folha (HE50, FALTA etc.)
        public List<ResumoCodigoDTO> ResumoPorCodigo { get; set; } = new();

        // Eventos que existiam mas não foram exportados por falta de código mapeado
        public List<string> EventosSemCodigo { get; set; } = new();
    }

    public class ResumoCodigoDTO
    {
        public string Codigo { get; set; } = "";
        public int Minutos { get; set; }
    }

    public class ExportacaoIgnoradoDTO
    {
        public int VinculoId { get; set; }
        public string Matricula { get; set; } = "";
        public string NomeServidor { get; set; } = "";
        public string Motivo { get; set; } = "";
    }
}
