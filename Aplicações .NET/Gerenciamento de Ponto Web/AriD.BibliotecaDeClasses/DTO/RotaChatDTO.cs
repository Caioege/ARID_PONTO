using System;
using System.Collections.Generic;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RotaChatMensagemDTO
    {
        public int Id { get; set; }
        public int RotaExecucaoId { get; set; }
        public int RotaId { get; set; }
        public string RotaDescricao { get; set; } = string.Empty;
        public int Origem { get; set; }
        public string OrigemDescricao { get; set; } = string.Empty;
        public int? UsuarioId { get; set; }
        public int? ServidorId { get; set; }
        public string RemetenteNome { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataHoraEnvio { get; set; }
        public string DataHoraEnvioFormatada => DataHoraEnvio.ToString("dd/MM/yyyy HH:mm");
        public bool LidaNoSistema { get; set; }
        public bool LidaNoAplicativo { get; set; }
    }

    public class RotaChatResumoDTO
    {
        public int RotaExecucaoId { get; set; }
        public int RotaId { get; set; }
        public string RotaDescricao { get; set; } = string.Empty;
        public bool Finalizada { get; set; }
        public int NaoLidasSistema { get; set; }
        public int NaoLidasAplicativo { get; set; }
        public List<RotaChatMensagemDTO> Mensagens { get; set; } = new();
    }

    public class RotaChatEnvioDTO
    {
        public int RotaExecucaoId { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    public class RotaChatPushDestinoDTO
    {
        public int RotaExecucaoId { get; set; }
        public int RotaId { get; set; }
        public string RotaDescricao { get; set; } = string.Empty;
        public string MotoristaNome { get; set; } = string.Empty;
        public string PushToken { get; set; } = string.Empty;
    }

    public class RotaChatNaoLidasDTO
    {
        public int RotaExecucaoId { get; set; }
        public int Quantidade { get; set; }
    }
}
