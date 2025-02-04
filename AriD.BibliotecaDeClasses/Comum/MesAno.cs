namespace AriD.BibliotecaDeClasses.Comum
{
    public class MesAno
    {
        public MesAno(string mesAno)
        {
            if (mesAno == null)
                throw new ArgumentNullException(nameof(mesAno));

            var valorDividido = mesAno.Split('-');
            Ano = int.Parse(valorDividido[0]);
            Mes = int.Parse(valorDividido[1]);
        }

        public int Mes { get; set; }
        public int Ano { get; set; }

        public DateTime Inicio => new DateTime(Ano, Mes, 01);
        public DateTime Fim => new DateTime(Ano, Mes, DateTime.DaysInMonth(Ano, Mes));

        public override string ToString() => $"{Mes.ToString().PadLeft(2, '0')}/{Ano}";
    }
}