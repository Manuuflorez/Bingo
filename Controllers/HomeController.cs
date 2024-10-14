using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bingoo.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bingoo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BingoContext _context;

        public HomeController(ILogger<HomeController> logger, BingoContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Obtener todas las salas activas (con al menos 1 jugador)
            var activeRooms = _context.Rooms.Where(r => r.ActivePlayers > 0).ToList();
            return View(activeRooms);
        }

        [HttpGet]
        public IActionResult CreateRoom()
        {
            // Si el usuario no está autenticado, redirigir al login
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            // Devolver la vista para la creación de sala
            return View();
        }

        [HttpPost]
        public IActionResult CreateRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                // Establecer la fecha de creación y el dueño de la sala
                room.CreatedAt = DateTime.Now;
                room.Owner = User?.Identity?.Name ?? "Anónimo";  // Nombre del usuario o Anónimo si no tiene nombre

                // Guardar la sala en la base de datos
                _context.Rooms.Add(room);
                _context.SaveChanges();

                // Redirigir a los detalles de la sala creada
                return RedirectToAction("PlayGame", new { id = room.Id });
            }

            // Si hay errores en el modelo, devolver la vista con el formulario de creación
            return View(room);
        }

        [HttpGet]
        public IActionResult RoomDetails(int id)
        {
            // Buscar la sala en la base de datos
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Authorize]
        public IActionResult PlayGame(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            // Incrementar el número de jugadores activos
            room.ActivePlayers++;
            _context.SaveChanges();

            // Pasar las características de la sala a la vista del juego
            return View(room);
        }

        [HttpPost]
        [Authorize]  // Asegúrate de que el usuario esté autenticado
        public IActionResult LeaveGame(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();  // Devolver 404 si la sala no se encuentra
            }

            // Lógica para decrementar los jugadores activos o eliminar la sala
            room.ActivePlayers--;

            // Si no hay jugadores activos, eliminar la sala
            if (room.ActivePlayers <= 0)
            {
                _context.Rooms.Remove(room);
            }

            _context.SaveChanges();

            return RedirectToAction("Index");  // Redirigir al inicio después de dejar la sala
        }


    }
}
