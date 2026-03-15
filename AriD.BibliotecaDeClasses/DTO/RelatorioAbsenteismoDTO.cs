using System;

namespace AriD.BibliotecaDeClasses.DTO
{
    public class RelatorioAbsenteismoDTO
    {
        public int VinculoDeTrabalhoId { get; set; }
        public string Matricula { get; set; }
        public string NomeServidor { get; set; }
        public string Departamento { get; set; }
        public string Funcao { get; set; }
        public DateTime Data { get; set; }
        
        // Pode ser "Falta Injustificada", "Atraso", "Falta Parcial"
        public string TipoAusencia { get; set; }
        
        // Em formato hh:mm
        public string TotalAtrasoOuFalta { get; set; }

        public TimeSpan? HorasNegativas { get; set; }
    }
}
