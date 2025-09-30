using System.ComponentModel.DataAnnotations;

namespace PortalDenuncias.Rosales.Models;
public class PersonaViewModel
{
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [Display(Name = "Cargo")]
    public string? Cargo { get; set; }

    [Display(Name = "Rol en los hechos")]
    public string? Rol { get; set; }
}

public class DenunciaViewModel
{
    [Required]
    public string TipoIdentificacion { get; set; } = "anonima"; // "anonima" o "identificada"

    [Display(Name = "Nombre Completo")]
    public string? DenuncianteNombre { get; set; }

    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [Display(Name = "Correo Electrónico de Contacto")]
    public string? DenuncianteEmail { get; set; }

    [Display(Name = "Teléfono de Contacto (Opcional)")]
    public string? DenuncianteTelefono { get; set; }
    
    [Display(Name = "Relación con la empresa")]
    public string? DenuncianteRelacion { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un tipo de delito.")]
    [Display(Name = "Tipo de presunto delito o falta")]
    public string TipoDelito { get; set; }

    [Required(ErrorMessage = "La descripción del hecho es obligatoria.")]
    [Display(Name = "Descripción detallada del hecho")]
    public string Descripcion { get; set; }

    [Required(ErrorMessage = "Debe indicar la fecha.")]
    [Display(Name = "¿Cuándo supo de esto?")]
    [DataType(DataType.Date)]
    public DateTime FechaConocimiento { get; set; }

    [Required(ErrorMessage = "Debe indicar el lugar de ocurrencia.")]
    [Display(Name = "¿Dónde ocurrió?")]
    public string Lugar { get; set; }

    public string IdentificaPersonasRadio { get; set; }

    public List<PersonaViewModel> Personas { get; set; } = new List<PersonaViewModel>();

    [Required(ErrorMessage = "Debe indicar este dato")]
    [Display(Name = "¿Con qué frecuencia ocurre?")]
    public string Frecuencia { get; set; }

    [Required (ErrorMessage = "Debe indicar si la situación sigue ocurriendo")]
    [Display(Name = "¿La situación sigue ocurriendo?")]
    public string SigueOcurriendo { get; set; }

    [Required (ErrorMessage = "Este dato es requerido")]
    [Display(Name = "¿Reportó esta situación antes?")]
    public string ReportoAntes { get; set; }

    [Display(Name = "Detalle del reporte previo")]
    public string? DetalleReportePrevio { get; set; }

    [Display(Name = "Descripción de la evidencia")]
    public string? DescripcionEvidencia { get; set; }

    // Para recibir los archivos adjuntos
    [Display(Name = "Adjuntar evidencia (opcional)")]
    public List<IFormFile> Archivos { get; set; } = new List<IFormFile>();
}