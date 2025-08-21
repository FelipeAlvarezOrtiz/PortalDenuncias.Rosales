using PortalDenuncias.Rosales.Interfaces;
using System.Net;
using System.Net.Mail;

namespace PortalDenuncias.Rosales.Infraestructura
{
    public sealed class Mailer : IMailer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Mailer> _logger;

        public Mailer(IConfiguration configuration, ILogger<Mailer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task EnviarEmail(List<string> destinatarios, string asunto, string cuerpo)
        {
            throw new NotImplementedException();
        }

        public Task EnviarEmailAdjuntoBytes(List<string> destinatarios, string asunto, string cuerpo, byte[] adjunto, string fileNameAdjunto)
        {
            throw new NotImplementedException();
        }

        public async Task EnviarEmailConAdjuntos(List<string> destinatarios, string asunto, string cuerpo, List<Tuple<byte[], string>> archivos)
        {
            var correo = _configuration.GetValue<string>("Mailer:Correo");
            var alias = _configuration.GetValue<string>("Mailer:Alias");
            var host = _configuration.GetValue<string>("Mailer:Host") ?? throw new ArgumentNullException("Mail:CMPC");
            var puerto = _configuration.GetValue<int>("Mailer:Puerto");
            var enableSsl = _configuration.GetValue<bool>("Mailer:EnabledSsl");
            const string fromPassword = "pyknanztxsuyxwjl";
            var fromAddress = new MailAddress(correo, alias);
            var subject = asunto;
            var body = cuerpo;
            var mensaje = new MailMessage();
            try
            {
                mensaje.From = fromAddress;
                mensaje.Subject = subject;
                mensaje.Body = body;
                mensaje.IsBodyHtml = true;

                foreach (var destinatario in destinatarios)
                {
                    mensaje.To.Add(new MailAddress(destinatario));
                }

                foreach (var archivo in archivos)
                {
                    mensaje.Attachments.Add(new Attachment(new MemoryStream(archivo.Item1), $"{archivo.Item2}"));
                }

                var smtp = new SmtpClient
                {
                    Host = host,
                    Port = puerto,
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                await smtp.SendMailAsync(mensaje);
                _logger.LogInformation("CORREO ENVIADO.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
