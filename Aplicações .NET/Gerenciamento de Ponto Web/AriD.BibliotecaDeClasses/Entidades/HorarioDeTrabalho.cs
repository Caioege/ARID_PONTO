using AriD.BibliotecaDeClasses.Comum;
using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class HorarioDeTrabalho : EntidadeOrganizacaoBase
    {
        [Required, MaxLength(5)]
        public string Sigla { get; set; }

        [Required, MaxLength(100)]
        public string Descricao { get; set; }

        public bool Ativo { get; set; }

        public string SiglaComDescricao => $"[{Sigla}] {Descricao}";

        public virtual List<HorarioDeTrabalhoVigencia> Vigencias { get; set; } = new();

        public HorarioDeTrabalhoVigencia ObtenhaVigenciaDoMes(MesAno mesAno)
            => ObtenhaVigenciaVigente(mesAno.Inicio);

        public HorarioDeTrabalhoVigencia ObtenhaVigenciaVigente(DateTime data)
        {
            if (Vigencias == null || Vigencias.Count == 0)
                throw new ApplicationException("Horário sem vigências carregadas. Carregue HorarioDeTrabalho.Vigencias antes de chamar ObtenhaVigenciaVigente.");

            var dt = data.Date;

            // pega a vigência mais recente com inicio <= data
            var vig = Vigencias
                .Where(v => v.VigenciaInicio.Date <= dt)
                .OrderByDescending(v => v.VigenciaInicio)
                .FirstOrDefault();

            if (vig == null)
                throw new ApplicationException($"Não existe vigência para o horário na data {dt:dd/MM/yyyy}.");

            return vig;
        }
    }
}