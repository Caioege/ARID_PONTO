using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Security.Cryptography;
using System.Text;

namespace AriD.Servicos.Helpers
{
    public static class ComprovantePdfHelper
    {
        public static string GerarComprovantePortaria671(
            int organizacaoId,
            string razaoSocial,
            string cnpj,
            string nomeColaborador,
            string cpfColaborador,
            long nsr,
            DateTime dataRegistro)
        {
            var logoPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "brasoes", $"{organizacaoId}.png");

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);

            pdf.SetDefaultPageSize(PageSize.A6);

            using var document = new Document(pdf);
            document.SetMargins(10, 10, 10, 10);

            Color corTexto = ColorConstants.BLACK;
            Color corLabels = ColorConstants.DARK_GRAY;

            document.Add(new Paragraph("Comprovante de Registro de Ponto do Trabalhador")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetBold()
                .SetFontColor(corTexto));

            document.Add(new LineSeparator(new SolidLine(1f)).SetMarginTop(5).SetMarginBottom(5));

            if (System.IO.File.Exists(logoPath))
            {
                var imgData = ImageDataFactory.Create(logoPath);
                var img = new Image(imgData).SetHeight(30).SetHorizontalAlignment(HorizontalAlignment.CENTER);
                document.Add(img);
            }

            document.Add(new Paragraph("EMPREGADOR").SetFontSize(7).SetBold());
            document.Add(new Paragraph($"{razaoSocial}").SetFontSize(9));
            document.Add(new Paragraph($"CNPJ: {cnpj}").SetFontSize(9));

            document.Add(new Paragraph("\n"));

            document.Add(new Paragraph("TRABALHADOR").SetFontSize(7).SetBold());
            document.Add(new Paragraph($"Nome: {nomeColaborador}").SetFontSize(9));
            document.Add(new Paragraph($"CPF: {cpfColaborador}").SetFontSize(9));

            document.Add(new Paragraph("\n"));

            var containerRegistro = new Div()
                .SetBorder(new DashedBorder(ColorConstants.GRAY, 1))
                .SetPadding(5)
                .SetKeepTogether(true);

            containerRegistro.Add(new Paragraph("MARCAÇÃO DE PONTO").SetFontSize(8).SetTextAlignment(TextAlignment.CENTER));

            containerRegistro.Add(new Paragraph($"NSR: {nsr.ToString("D9")}")
                .SetFontSize(10).SetBold().SetTextAlignment(TextAlignment.CENTER));

            containerRegistro.Add(new Paragraph($"{dataRegistro:dd/MM/yyyy}   {dataRegistro:HH:mm}")
                .SetFontSize(14).SetBold().SetTextAlignment(TextAlignment.CENTER));

            containerRegistro.Add(new Paragraph("Fuso Horário: -03:00 (Brasília)")
                .SetFontSize(7).SetTextAlignment(TextAlignment.CENTER));

            document.Add(containerRegistro);

            string dadosParaHash = $"{nsr}{cnpj}{cpfColaborador}{dataRegistro:yyyyMMddHHmm}";
            string hash = GerarHashSHA256(dadosParaHash);

            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph("CÓDIGO DE INTEGRIDADE (HASH SHA-256):")
                .SetFontSize(6).SetBold().SetTextAlignment(TextAlignment.LEFT));

            document.Add(new Paragraph(hash)
                .SetFontSize(6)
                .SetFontColor(ColorConstants.DARK_GRAY)
                .SetTextAlignment(TextAlignment.LEFT));

            document.Add(new Paragraph("Sistema de Registro Eletrônico de Ponto")
                .SetFontSize(6).SetItalic().SetTextAlignment(TextAlignment.CENTER).SetMarginTop(10));

            document.Close();

            byte[] bytes = stream.ToArray();
            return $"data:application/pdf;base64,{Convert.ToBase64String(bytes)}";
        }

        private static string GerarHashSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}