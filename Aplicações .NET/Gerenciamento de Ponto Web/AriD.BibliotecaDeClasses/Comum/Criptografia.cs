using System.Security.Cryptography;
using System.Text;

public static class Criptografia
{
    public static string CriptografarSenha(string senha)
    {
        if (string.IsNullOrEmpty(senha))
        {
            throw new ArgumentException("A senha não pode ser nula ou vazia.");
        }

        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(senha));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
