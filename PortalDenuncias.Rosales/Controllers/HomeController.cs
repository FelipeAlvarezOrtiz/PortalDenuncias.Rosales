using Microsoft.AspNetCore.Mvc;
using PortalDenuncias.Rosales.Models;
using System.Diagnostics;
using System.Text;
using PortalDenuncias.Rosales.Interfaces;

namespace PortalDenuncias.Rosales.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IMailer _mailer;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, IMailer mailer)
        {
            _logger = logger;
            _env = env;
            _mailer = mailer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FormularioDenuncia()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DenunciaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si hay errores, vuelve a mostrar el formulario con los datos y errores.
                return View("FormularioDenuncia",model);
            }

            var codigoSeguimiento = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

            // 2. Preparar y enviar el correo de notificación al administrador
            try
            {
                var cuerpoCorreo = await GenerarCuerpoCorreoAsync(model, codigoSeguimiento);

                // --- Lógica para enviar el correo ---
                // Aquí deberías usar un servicio de correo real (SendGrid, MailKit, etc.)
                var archivosTuples = new List<Tuple<byte[], string>>();
                foreach (var modelArchivo in model.Archivos)
                {
                    var resultExtraccion = await ExtraerArchivoInfoAsync(modelArchivo);
                    if(!resultExtraccion.HasValue)
                        continue;
                    archivosTuples.Add(new Tuple<byte[], string>(resultExtraccion.Value.FileBytes,resultExtraccion.Value.FileName));
                }
                // await _emailSender.SendEmailAsync("admin@tuempresa.com", "Nueva Denuncia Recibida", cuerpoCorreo);
                await _mailer.EnviarEmailConAdjuntos([
                        "me@felipealvarez.dev"
                    ],
                    "Nueva denuncia ingresada",
                    cuerpoCorreo,
                    archivosTuples
                    );
                Console.WriteLine("Correo enviado a admin@tuempresa.com"); // Simulación para depuración
            }
            catch (Exception ex)
            {
                // Manejar el error si el correo no se puede enviar.
                // Es importante que la denuncia se guarde aunque el correo falle.
                _logger.LogError(ex.Message);
            }

            TempData["CodigoExito"] = codigoSeguimiento;

            return RedirectToAction("Exito");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Exito()
        {
            return View();
        }

        private async Task<string> GenerarCuerpoCorreoAsync(DenunciaViewModel model, string codigoSeguimiento)
        {
            // Ruta al template HTML
            var path = Path.Combine(_env.WebRootPath, "templates", "email", "NuevaDenunciaAdmin.html");
            var template = await System.IO.File.ReadAllTextAsync(path);

            // Reemplazar placeholders básicos
            template = template.Replace("{{CODIGO_SEGUIMIENTO}}", codigoSeguimiento)
                               .Replace("{{FECHA_RECEPCION}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                               .Replace("{{TIPO_DELITO}}", model.TipoDelito)
                               .Replace("{{FECHA_CONOCIMIENTO}}", model.FechaConocimiento.ToString("dd/MM/yyyy"))
                               .Replace("{{LUGAR}}", model.Lugar)
                               .Replace("{{DESCRIPCION}}", model.Descripcion)
                               .Replace("{{FRECUENCIA}}", model.Frecuencia)
                               .Replace("{{SIGUE_OCURRIENDO}}", model.SigueOcurriendo)
                               .Replace("{{REPORTO_ANTES}}", model.ReportoAntes)
                               .Replace("{{ARCHIVOS_ADJUNTOS}}", model.Archivos.Any() ? $"Sí ({model.Archivos.Count} archivo/s)" : "No")
                               .Replace("{{URL_PORTAL_GESTION}}", "https://tuportal.com/admin/denuncias/" + codigoSeguimiento); // URL de ejemplo

            // Lógica para secciones condicionales (más limpio y robusto)

            // Sección de Personas Involucradas
            if (model.Personas.Any())
            {
                var personasHtml = new StringBuilder("<h2>Personas Involucradas</h2>");
                foreach (var persona in model.Personas)
                {
                    personasHtml.Append("<div class='details-section' style='margin-bottom:15px;'>");
                    personasHtml.Append($"<p><b>Nombre:</b> {persona.Nombre ?? "No especificado"}<br>");
                    personasHtml.Append($"<b>Cargo:</b> {persona.Cargo ?? "No especificado"}<br>");
                    personasHtml.Append($"<b>Rol:</b> {persona.Rol ?? "No especificado"}</p>");
                    personasHtml.Append("</div>");
                }
                template = template.Replace("{{SECCION_PERSONAS}}", personasHtml.ToString());
            }
            else
            {
                template = template.Replace("{{SECCION_PERSONAS}}", ""); // Si no hay, se elimina el placeholder
            }

            // Sección de Reporte Previo
            if (model.ReportoAntes == "si" && !string.IsNullOrWhiteSpace(model.DetalleReportePrevio))
            {
                var reportePrevioHtml = $"<h2>Detalle del Reporte Previo</h2><div class='details-section'><p>{model.DetalleReportePrevio}</p></div>";
                template = template.Replace("{{SECCION_REPORTE_PREVIO}}", reportePrevioHtml);
            }
            else
            {
                template = template.Replace("{{SECCION_REPORTE_PREVIO}}", "");
            }

            // Sección de Descripción de Evidencia
            if (model.Archivos.Any() && !string.IsNullOrWhiteSpace(model.DescripcionEvidencia))
            {
                string descEvidenciaHtml = $"<h2>Descripción de la Evidencia</h2><div class='details-section'><p>{model.DescripcionEvidencia}</p></div>";
                template = template.Replace("{{SECCION_DESCRIPCION_EVIDENCIA}}", descEvidenciaHtml);
            }
            else
            {
                template = template.Replace("{{SECCION_DESCRIPCION_EVIDENCIA}}", "");
            }

            return template;
        }

        /// <summary>
        /// Extrae los bytes y el nombre de un archivo IFormFile.
        /// </summary>
        /// <param name="archivo">El IFormFile recibido desde el formulario.</param>
        /// <returns>Una tupla con el array de bytes y el nombre completo del archivo.</returns>
        public async Task<(byte[] FileBytes, string FileName)?> ExtraerArchivoInfoAsync(IFormFile? archivo)
        {
            // 1. Validar que el archivo no sea nulo o esté vacío
            if (archivo == null || archivo.Length == 0)
            {
                return null;
            }

            // 2. Extraer el nombre completo directamente de la propiedad FileName
            string nombreCompleto = archivo.FileName;

            // 3. Extraer los bytes del archivo
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                // Copiamos el contenido del archivo a un stream en memoria
                await archivo.CopyToAsync(memoryStream);

                // Convertimos el stream en memoria a un array de bytes
                fileBytes = memoryStream.ToArray();
            }

            return (fileBytes, nombreCompleto);
        }
    }
}
