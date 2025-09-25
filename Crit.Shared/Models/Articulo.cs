using System;
using System.ComponentModel.DataAnnotations;

public enum NivelPriorizacion
{
    Bajo = 0,
    Medio = 1,
    Alto = 2,
    Critico = 3
}

public class Articulo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50, ErrorMessage = "El código no puede exceder los 50 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación es obligatoria.")]
    [StringLength(200, ErrorMessage = "La ubicación no puede exceder los 200 caracteres.")]
    public string Ubicacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El uso es obligatorio.")]
    [StringLength(300, ErrorMessage = "El uso no puede exceder los 300 caracteres.")]
    public string Uso { get; set; } = string.Empty;

    public NivelPriorizacion NivelPriorizacion { get; set; } = NivelPriorizacion.Medio;

    [DataType(DataType.Date)]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public string? UsuarioQueRegistroId { get; set; }
    public string? UsuarioQueRegistroUserName { get; set; }
}