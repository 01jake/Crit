using System;
using System.ComponentModel.DataAnnotations;

public enum EstatusQueja
{
    Pendiente = 0,
    Atendida = 1,
    Cerrada = 2
}

public enum PrioridadQueja
{
    Baja = 0,
    Media = 1,
    Alta = 2
}

public class Queja
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string NombreCliente { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "El número de afiliación no puede exceder los 50 caracteres.")]
    public string NumeroAfiliacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede exceder los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción de la queja es obligatoria.")]
    [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres.")]
    public string DescripcionQueja { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(100, ErrorMessage = "La categoría no puede exceder los 100 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Now;

    public EstatusQueja Estatus { get; set; } = EstatusQueja.Pendiente;

    public PrioridadQueja Prioridad { get; set; } = PrioridadQueja.Media;

    // Propiedades para el DTO (lo que envías al cliente)
    public string? ClienteId { get; set; }
    public string? ClienteUserName { get; set; }
}