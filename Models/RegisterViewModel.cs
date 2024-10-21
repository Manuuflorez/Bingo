using System.ComponentModel.DataAnnotations;

namespace Bingoo.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;  // Nombre de usuario o nick (debe ser único)

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;  // Correo electrónico (debe ser único)

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres.", MinimumLength = 6)]
        // Solo requiere una longitud mínima de 6 caracteres
        [RegularExpression(@"^.{6,}$", ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;  // Contraseña menos estricta

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;  // Confirmación de contraseña
    }
}
