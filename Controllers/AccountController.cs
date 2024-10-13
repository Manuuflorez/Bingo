using Bingoo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace Bingoo.Controllers
{
    public class AccountController : Controller
    {
        private readonly BingoContext _context;

        public AccountController(BingoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Verificar si el usuario ya está autenticado
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                
                // Verificación si el usuario no es nulo
                if (user != null)
                {
                    // Crear los claims para el usuario
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Name),  // Usar el nombre del usuario
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        // Configurar propiedades como persistencia de cookies si es necesario
                    };

                    // Iniciar sesión del usuario
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    return RedirectToAction("Index", "Home");  // Redirigir al Home después de iniciar sesión
                }

                // Si las credenciales son incorrectas
                ModelState.AddModelError("", "Intento de inicio de sesión inválido");
            }

            // Si el modelo no es válido, volver a la vista con el modelo
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                
                // Verificar si el correo electrónico ya está registrado
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Password = model.Password, // Considera encriptar la contraseña en producción
                        Credits = 1000 // Créditos iniciales
                    };

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    return RedirectToAction("Login");
                }

                // Si el correo ya está registrado
                ModelState.AddModelError("", "El correo electrónico ya está registrado");
            }

            // Si el modelo no es válido, volver a la vista de registro
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            // Cerrar sesión del usuario
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

          [HttpPost]
        public IActionResult CreateRoom(string generatorType, int speed, string markingType, int maxPlayers, string gameRules)
        {
            // Crear una nueva sala con las opciones seleccionadas
            var room = new Room
            {
                GeneratorType = generatorType,
                Speed = speed,
                MarkingType = markingType,
                MaxPlayers = maxPlayers,
                GameRules = gameRules,
                CreatedAt = DateTime.Now,
                Owner = User?.Identity?.Name ?? "Anónimo"  // Asignar el nombre del usuario que creó la sala
            };

            // Guardar la sala en la base de datos
            _context.Rooms.Add(room);
            _context.SaveChanges();

            // Redirigir a la vista de detalles de la sala creada
            return RedirectToAction("RoomDetails", new { room.Id });
        }

        [HttpGet]
        public IActionResult RoomDetails(int id)
        {
            // Buscar la sala por su Id
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            // Pasar la sala a la vista
            return View(room);
        }
    }
}
