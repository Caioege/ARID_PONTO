using System;

namespace AriD.BibliotecaDeClasses.Enumeradores
{
    [Flags]
    public enum eFlagDiaSemana
    {
        Nenhum = 0,
        Domingo = 1,
        Segunda = 2,
        Terca = 4,
        Quarta = 8,
        Quinta = 16,
        Sexta = 32,
        Sabado = 64
    }
}
