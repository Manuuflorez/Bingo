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
                .AsEnumerable()
                .Where(r => r.ActivePlayers > 0
                            || r.GameStarted
                            || (r.Players != null && currentUserName != null && r.Players.Contains(currentUserName))
                            || (r.PreviousPlayers != null && currentUserName != null && r.PreviousPlayers.Contains(currentUserName)))
                .ToList();

            return View(rooms);
        }

        [HttpGet]
        public IActionResult CreateRoom()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

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

            var currentUserName = User.Identity?.Name;

            if (currentUserName == null)
            {
                return RedirectToAction("Index");
            }

            // Permitir siempre al creador de la sala acceder a su propia sala
            if (room.Owner == currentUserName)
            {
                if (!room.Players.Contains(currentUserName))
                {
                    room.Players.Add(currentUserName);
                    room.ActivePlayers = room.Players.Count;
                    _context.SaveChanges();
                }

                return View(room);
            }

            // Verificar si el número máximo de jugadores ya se ha alcanzado
            if (room.ActivePlayers >= room.MaxPlayers)
            {
                TempData["ErrorMessage"] = "La sala ya ha alcanzado el número máximo de jugadores.";
                return RedirectToAction("Index");
            }

            // Verificar si el usuario ya está en la lista de jugadores o en la lista de jugadores previos
            if (!room.Players.Contains(currentUserName) && (room.GameStarted && room.PreviousPlayers.Contains(currentUserName)))
            {
                room.Players.Add(currentUserName);
                room.ActivePlayers = room.Players.Count;
                _context.SaveChanges();
            }
            else if (!room.Players.Contains(currentUserName) && !room.GameStarted)
            {
                room.Players.Add(currentUserName);
                room.ActivePlayers = room.Players.Count;
                _context.SaveChanges();
            }

            return View(room);
        }


        [HttpPost]
        public IActionResult CreateRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                room.CreatedAt = DateTime.Now;
                room.Owner = User?.Identity?.Name ?? "Anónimo";
                room.GameStarted = false;
                room.ActivePlayers = 1;
                room.Players = new List<string> { room.Owner };

                _context.Rooms.Add(room);
                _context.SaveChanges();

                return RedirectToAction("PlayGame", new { id = room.Id });
            }

            return View(room);
        }

        [HttpGet]
        public IActionResult RoomDetails(int id)
        {
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
                return NotFound();
            }

            var currentUserName = User.Identity?.Name;

            if (currentUserName == null)
            {
                return RedirectToAction("Index");
            }

            if (room.Players.Contains(currentUserName))
            {
                room.Players.Remove(currentUserName);
                room.ActivePlayers--;

                if (!room.PreviousPlayers.Contains(currentUserName))
                {
                    room.PreviousPlayers.Add(currentUserName);
                }

                if (room.ActivePlayers > 0)
                {
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    _context.Rooms.Remove(room);
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
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
