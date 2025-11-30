using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Helpers;
using System.Net.Mail;
using System.Net;
using System.Text;
using AriD.Servicos.Servicos.Interfaces;

namespace AriD.Servicos.Servicos
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost = "ponto.arid.com.br";
        private readonly int _smtpPort = 587;
        private readonly string _remetenteEmail = "comunicacao@ponto.arid.com.br";
        private readonly string _remetenteSenha = "aridtecnologia2021";

        public async Task EnviarComprovantePontoAsync(Servidor servidor, int nsr, DateTime dataHora)
        {
            if (servidor == null)
                throw new ApplicationException("Servidor não encontrado.");

            if (string.IsNullOrEmpty(servidor.Email)) 
                return;

            string pdfBase64ComHeader = ComprovantePdfHelper.GerarComprovantePortaria671(
                servidor.OrganizacaoId,
                servidor.Organizacao.Nome,
                string.Empty,
                servidor.Nome,
                servidor.Pessoa.Cpf,
                nsr,
                dataHora
            );

            var base64Puro = pdfBase64ComHeader.Split(',')[1];
            byte[] pdfBytes = Convert.FromBase64String(base64Puro);
            using var pdfStream = new MemoryStream(pdfBytes);

            string assunto = $"Confirmação de Ponto - {dataHora:dd/MM/yyyy}";
            string nomeArquivo = $"Comprovante_Ponto_{dataHora:yyyyMMdd_HHmm}.pdf";

            var sbCorpo = new StringBuilder();
            sbCorpo.AppendLine("<html><body>");
            sbCorpo.AppendLine($"<p>Olá, <strong>{servidor.Nome.ToUpper()}</strong>!</p>");
            sbCorpo.AppendLine("<p>Recebemos seu registro de ponto com sucesso.</p>");
            sbCorpo.AppendLine("<ul>");
            sbCorpo.AppendLine($"<li><strong>Data:</strong> {dataHora:dd/MM/yyyy}</li>");
            sbCorpo.AppendLine($"<li><strong>Hora:</strong> {dataHora:HH:mm}</li>");
            sbCorpo.AppendLine("</ul>");
            sbCorpo.AppendLine("<p>O comprovante oficial, atendendo à Portaria 671, está anexo a esta mensagem.</p>");
            sbCorpo.AppendLine("<br>");
            sbCorpo.AppendLine($"<p>Atenciosamente,<br><strong>{servidor.Organizacao.Nome}</strong></p>");
            sbCorpo.AppendLine("<hr>");
            sbCorpo.AppendLine("<p style='font-size: 12px; color: #666;'><em>Este é um e-mail automático. Por favor, não responda a esta mensagem.</em></p>");
            sbCorpo.AppendLine("</body></html>");

            using (var mensagem = new MailMessage())
            {
                mensagem.From = new MailAddress(_remetenteEmail, "AriD Tecnologia - Comprovante de Ponto");
                mensagem.To.Add(servidor.Email);
                mensagem.Subject = assunto;
                mensagem.Body = sbCorpo.ToString();
                mensagem.IsBodyHtml = true;

                mensagem.Attachments.Add(new Attachment(pdfStream, nomeArquivo, "application/pdf"));

                using (var smtp = new SmtpClient(_smtpHost, _smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(_remetenteEmail, _remetenteSenha);
                    smtp.EnableSsl = true;

                    try
                    {
                        await smtp.SendMailAsync(mensagem);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException($"Erro ao enviar e-mail: {ex.Message}");
                    }
                }
            }
        }
    }
}