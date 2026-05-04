using AriD.BibliotecaDeClasses.Entidades;
using AriD.Servicos.Helpers;
using System.Net.Mail;
using System.Net;
using System.Text;
using AriD.Servicos.Servicos.Interfaces;
using AriD.BibliotecaDeClasses.Configuracoes;
using Microsoft.Extensions.Options;
using SendGrid;
using AriD.BibliotecaDeClasses.DTO;
using SendGrid.Helpers.Mail;

namespace AriD.Servicos.Servicos
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfig _config;

        public EmailService(IOptions<EmailConfig> config)
        {
            _config = config.Value;
        }

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

            if (_config.UseSendGrid && !string.IsNullOrEmpty(_config.SendGridApiKey))
            {
                await EnviarViaSendGrid(servidor.Email, assunto, sbCorpo.ToString(), pdfBytes, nomeArquivo);
            }
            else
            {
                await EnviarViaSmtp(servidor.Email, assunto, sbCorpo.ToString(), pdfStream, nomeArquivo);
            }
        }

        public async Task EnviarNotificacaoConectividadeAsync(string emailDestinatario, string nomeEntidade, List<EquipamentoConectividadeInfo> equipamentos)
        {
            if (string.IsNullOrEmpty(emailDestinatario)) return;

            string assunto = $"[ALERTA] Problemas de Conectividade - {nomeEntidade} - {DateTime.Now:dd/MM/yyyy HH:mm}";

            var sbCorpo = new StringBuilder();
            sbCorpo.AppendLine("<!DOCTYPE html>");
            sbCorpo.AppendLine("<html lang='pt-br'>");
            sbCorpo.AppendLine("<head><meta charset='UTF-8'><style>");
            sbCorpo.AppendLine("body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; line-height: 1.6; margin: 0; padding: 0; background-color: #f4f7f9; }");
            sbCorpo.AppendLine(".container { max-width: 650px; margin: 20px auto; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); border: 1px solid #e1e8ed; }");
            sbCorpo.AppendLine(".header { background-color: #12192c; padding: 30px; text-align: center; color: #fff; }");
            sbCorpo.AppendLine(".header img { max-height: 60px; margin-bottom: 15px; }");
            sbCorpo.AppendLine(".header h1 { margin: 0; font-size: 22px; font-weight: 600; letter-spacing: 0.5px; }");
            sbCorpo.AppendLine(".content { padding: 40px; }");
            sbCorpo.AppendLine(".alert-box { background-color: #fff4f4; border-left: 4px solid #d9534f; padding: 15px; margin-bottom: 25px; border-radius: 4px; }");
            sbCorpo.AppendLine(".alert-box p { margin: 0; color: #a94442; font-weight: 500; }");
            sbCorpo.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; font-size: 14px; }");
            sbCorpo.AppendLine("th { background-color: #f8f9fa; text-align: left; padding: 12px; border-bottom: 2px solid #dee2e6; color: #495057; text-transform: uppercase; font-size: 12px; }");
            sbCorpo.AppendLine("td { padding: 12px; border-bottom: 1px solid #eee; color: #333; }");
            sbCorpo.AppendLine(".footer { background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #eee; }");
            sbCorpo.AppendLine(".device-name { font-weight: 600; color: #12192c; }");
            sbCorpo.AppendLine(".status-tag { display: inline-block; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; background-color: #ffdada; color: #c9302c; }");
            sbCorpo.AppendLine("</style></head><body>");

            sbCorpo.AppendLine("<div class='container'>");
            sbCorpo.AppendLine("  <div class='header'>");
            sbCorpo.AppendLine("    <img src='https://ponto.arid.com.br/img/logo-vertical.png' alt='AriD Ponto'>");
            sbCorpo.AppendLine("    <h1>Monitoramento de Equipamentos</h1>");
            sbCorpo.AppendLine("  </div>");
            
            sbCorpo.AppendLine("  <div class='content'>");
            sbCorpo.AppendLine($"    <p>Olá, <strong>{nomeEntidade}</strong>,</p>");
            sbCorpo.AppendLine("    <div class='alert-box'>");
            sbCorpo.AppendLine("      <p>Identificamos que os seguintes equipamentos estão sem comunicação com o sistema há algum tempo.</p>");
            sbCorpo.AppendLine("    </div>");

            sbCorpo.AppendLine("    <table>");
            sbCorpo.AppendLine("      <thead><tr><th>Equipamento</th><th>Nº Série</th><th>Unidade</th><th>Último Registro</th></tr></thead>");
            sbCorpo.AppendLine("      <tbody>");

            foreach (var eq in equipamentos)
            {
                sbCorpo.AppendLine("        <tr>");
                sbCorpo.AppendLine($"          <td class='device-name'>{eq.Descricao}</td>");
                sbCorpo.AppendLine($"          <td>{eq.NumeroSerie}</td>");
                sbCorpo.AppendLine($"          <td>{eq.UnidadeNome}</td>");
                sbCorpo.AppendLine($"          <td><span class='status-tag'>{eq.DataHoraUltimoRegistro:dd/MM/yyyy HH:mm}</span></td>");
                sbCorpo.AppendLine("        </tr>");
            }

            sbCorpo.AppendLine("      </tbody>");
            sbCorpo.AppendLine("    </table>");
            
            sbCorpo.AppendLine("    <p style='margin-top: 30px;'>Por favor, verifique a conexão de rede e o status físico dos equipamentos listados.</p>");
            sbCorpo.AppendLine("  </div>");

            sbCorpo.AppendLine("  <div class='footer'>");
            sbCorpo.AppendLine("    <p>© " + DateTime.Now.Year + " AriD Tecnologia - Sistema de Ponto Facial</p>");
            sbCorpo.AppendLine("    <p>Este é um e-mail automático. Por favor, não responda.</p>");
            sbCorpo.AppendLine("  </div>");
            sbCorpo.AppendLine("</div>");
            sbCorpo.AppendLine("</body></html>");

            if (_config.UseSendGrid && !string.IsNullOrEmpty(_config.SendGridApiKey))
            {
                await EnviarViaSendGrid(emailDestinatario, assunto, sbCorpo.ToString());
            }
            else
            {
                await EnviarViaSmtp(emailDestinatario, assunto, sbCorpo.ToString());
            }
        }

        private async Task EnviarViaSmtp(string destinatario, string assunto, string corpo, Stream? anexoStream = null, string? nomeAnexo = null)
        {
            using (var mensagem = new MailMessage())
            {
                mensagem.From = new MailAddress(_config.RemetenteEmail, "AriD Tecnologia - Sistema de Ponto");
                mensagem.To.Add(destinatario);
                mensagem.Subject = assunto;
                mensagem.Body = corpo;
                mensagem.IsBodyHtml = true;

                if (anexoStream != null && !string.IsNullOrEmpty(nomeAnexo))
                {
                    mensagem.Attachments.Add(new System.Net.Mail.Attachment(anexoStream, nomeAnexo, "application/pdf"));
                }

                using (var smtp = new SmtpClient(_config.SmtpHost, _config.SmtpPort))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_config.RemetenteEmail, _config.RemetenteSenha);
                    smtp.EnableSsl = true;

                    // Forçar TLS 1.2 para maior compatibilidade se necessário
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                    try
                    {
                        await smtp.SendMailAsync(mensagem);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException($"Erro ao enviar e-mail via SMTP: {ex.Message}");
                    }
                }
            }
        }

        private async Task EnviarViaSendGrid(string destinatario, string assunto, string corpo, byte[]? anexoBytes = null, string? nomeAnexo = null)
        {
            var client = new SendGridClient(_config.SendGridApiKey);
            var from = new EmailAddress(_config.RemetenteEmail, "AriD Tecnologia - Sistema de Ponto");
            var to = new EmailAddress(destinatario);
            var msg = MailHelper.CreateSingleEmail(from, to, assunto, "", corpo);

            if (anexoBytes != null && !string.IsNullOrEmpty(nomeAnexo))
            {
                msg.AddAttachment(nomeAnexo, Convert.ToBase64String(anexoBytes), "application/pdf");
            }

            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                throw new ApplicationException($"Erro ao enviar e-mail via SendGrid: {response.StatusCode} - {responseBody}");
            }
        }
    }
}