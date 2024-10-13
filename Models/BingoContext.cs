using Microsoft.EntityFrameworkCore;

namespace Bingoo.Models
{
    public class BingoContext : DbContext
    {
        public BingoContext(DbContextOptions<BingoContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }  // Agregar la entidad Room
    }
}
