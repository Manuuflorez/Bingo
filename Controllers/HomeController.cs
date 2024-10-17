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
            var currentUserName = User?.Identity?.Name;

            // Obtener todas las salas y luego filtrar en memoria para evitar problemas de traducción con listas de cadenas
            var rooms = _context.Rooms
                .Where(r => r.ActivePlayers > 0 || r.GameStarted)
                .AsEnumerable()  // Esto fuerza a EF Core a traer los datos antes de filtrar en memoria
                .Where(r => r.ActivePlayers > 0
                            || r.GameStarted
                            || r.Players.Contains(currentUserName)
                            || r.PreviousPlayers.Contains(currentUserName))
                .ToList();

            return View(rooms);
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

        [HttpGet]
        [Authorize]
        public IActionResult PlayGame(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            var currentUserName = User.Identity.Name;

            // Verificar si el usuario ya está en la lista de jugadores o en la lista de jugadores previos
            if (!room.Players.Contains(currentUserName) && (room.GameStarted && room.PreviousPlayers.Contains(currentUserName)))
            {
                room.Players.Add(currentUserName);
                room.ActivePlayers++;
                _context.SaveChanges();
            }
            else if (!room.Players.Contains(currentUserName) && !room.GameStarted)
            {
                room.Players.Add(currentUserName);
                room.ActivePlayers++;
                _context.SaveChanges();
            }

            return View(room);
        }


        [HttpPost]
        public IActionResult CreateRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                // Establecer la fecha de creación y el dueño de la sala
                room.CreatedAt = DateTime.Now;
                room.Owner = User?.Identity?.Name ?? "Anónimo"; // Nombre del usuario o Anónimo si no tiene nombre
                room.GameStarted = false; // Inicialmente, el juego no ha empezado
                room.ActivePlayers = 1; // El dueño de la sala se cuenta como el primer jugador activo
                room.Players = new List<string> { room.Owner }; // Agregar el dueño como jugador en la lista de jugadores

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

        [HttpPost]
        [Authorize]
        public IActionResult LeaveGame(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();  // Devolver 404 si la sala no se encuentra
            }

            var currentUserName = User.Identity.Name;

            // Verificar si el usuario está en la lista de jugadores actuales
            if (room.Players.Contains(currentUserName))
            {
                room.Players.Remove(currentUserName);
                room.ActivePlayers--;

                // Agregar al jugador a la lista de PreviousPlayers si no está ya en ella
                if (!room.PreviousPlayers.Contains(currentUserName))
                {
                    room.PreviousPlayers.Add(currentUserName);
                }

                // Si hay jugadores en la sala, no la eliminamos
                if (room.ActivePlayers > 0)
                {
                    _context.SaveChanges();
                    return RedirectToAction("Index");  // Redirigir al inicio
                }
                else
                {
                    // No hay jugadores restantes, eliminar la sala
                    _context.Rooms.Remove(room);
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");  // Redirigir al inicio después de dejar la sala
        }


        [HttpPost]
        [Authorize]
        public IActionResult StartGame(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            room.GameStarted = true;
            _context.SaveChanges();
            return RedirectToAction("PlayGame", new { id = room.Id });
        }
    }
}

