using System;
using System.ComponentModel.DataAnnotations;

public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un número positivo.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(50, ErrorMessage = "La categoría no puede exceder los 50 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    // Propiedades para el DTO (lo que envías al cliente)
    public string? UsuarioQueRegistroId { get; set; }
    public string? UsuarioQueRegistroUserName { get; set; }
}