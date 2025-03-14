using AriD.BibliotecaDeClasses.Atributos;
using System.ComponentModel;
using System.Reflection;

namespace AriD.Servicos.Extensao
{
    public static class ExtensaoDeEnum
    {
        public static string DescricaoDoEnumerador(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type type = value.GetType();

            string name = Enum.GetName(type, value);

            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
                    if (attribute != null)
                    {
                        return attribute.Description;
                    }
                }
            }

            return name ?? string.Empty;
        }

        public static string SiglaDaSemanaDoEnumerador(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type type = value.GetType();

            string name = Enum.GetName(type, value);

            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    SiglaDiaDaSemanaAttribute attribute = field.GetCustomAttribute<SiglaDiaDaSemanaAttribute>();
                    if (attribute != null)
                    {
                        return attribute.Sigla;
                    }
                }
            }

            return string.Empty;
        }
    }
}
