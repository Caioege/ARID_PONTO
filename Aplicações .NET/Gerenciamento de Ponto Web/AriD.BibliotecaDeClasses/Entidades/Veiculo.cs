using AriD.BibliotecaDeClasses.Entidades.Base;
using AriD.BibliotecaDeClasses.Enumeradores;

namespace AriD.BibliotecaDeClasses.Entidades
{
    public class Veiculo : EntidadeOrganizacaoBase
    {
        public string Placa { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public eTipoVeiculo TipoVeiculo { get; set; }
        public eCorVeiculo Cor { get; set; }
        public eTipoCombustivel TipoCombustivel { get; set; }
        public eStatusVeiculo Status { get; set; }
        public int AnoFabricacao { get; set; }
        public int AnoModelo { get; set; }
        public string Renavam { get; set; }
        public string Chassi { get; set; }
        public int QuilometragemAtual { get; set; }
        public DateTime VencimentoLicenciamento { get; set; }
    }
}