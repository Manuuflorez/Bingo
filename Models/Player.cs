namespace Bingoo.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // Relación con Room
        public int RoomId { get; set; }
        public required Room Room { get; set; }  // Relación con la sala a la que pertenece
    }
}
