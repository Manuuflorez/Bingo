using System;

namespace Bingoo.Models
{
    public class Room
    {
        public int Id { get; set; }

        public string GeneratorType { get; set; } = string.Empty;  // Inicializar con un valor predeterminado
        public int Speed { get; set; }
        public string MarkingType { get; set; } = string.Empty;  // Inicializar con un valor predeterminado
        public int MaxPlayers { get; set; }
        public string GameRules { get; set; } = string.Empty;  // Inicializar con un valor predeterminado
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // Inicializar con la fecha y hora actual
        public string Owner { get; set; } = string.Empty;  // Inicializar con un valor predeterminado
    }
}
