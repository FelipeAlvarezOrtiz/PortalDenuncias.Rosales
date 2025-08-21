namespace PortalDenuncias.Rosales.Interfaces
{
    public interface IMailer
    {
        Task EnviarEmail(List<string> destinatarios, string asunto, string cuerpo);
        Task EnviarEmailAdjuntoBytes(List<string> destinatarios, string asunto, string cuerpo, byte[] adjunto, string fileNameAdjunto);
        Task EnviarEmailConAdjuntos(List<string> destinatarios, string asunto, string cuerpo, List<Tuple<byte[],string>> archivos);
    }
}
