using Bingoo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Bingoo.Controllers
{
    public class AccountController : Controller
    {
        private readonly BingoContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher; 

          public AccountController(BingoContext context, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    // Verificar la contraseña encriptada
                    var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                    if (passwordVerificationResult == PasswordVerificationResult.Success)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Name),
                            new Claim(ClaimTypes.Email, user.Email)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Credenciales inválidas.");
            }

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
                // Verificar si el nombre de usuario o el correo ya están registrados
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email || u.Name == model.Name);
                
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Credits = 1000 // Créditos iniciales
                    };

                    // Encriptar la contraseña antes de guardarla
                    user.Password = _passwordHasher.HashPassword(user, model.Password);

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    return RedirectToAction("Login");
                }

                // Si el correo o el nombre de usuario ya están registrados
                if (_context.Users.Any(u => u.Name == model.Name))
                {
                    ModelState.AddModelError("Name", "El nombre de usuario ya está en uso.");
                }
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya está registrado.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
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
