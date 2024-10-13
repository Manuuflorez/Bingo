using System.ComponentModel.DataAnnotations;

namespace Bingoo.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;  // Inicializado con un valor por defecto.

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;  // Inicializado con un valor por defecto.
    }
}
