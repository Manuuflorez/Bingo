using System.ComponentModel.DataAnnotations;

namespace Bingoo.Models
{
    public class ApplicationUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;  // Campo de nombre o nick

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public int Credits { get; set; } = 1000;  // Créditos iniciales
    }
}
