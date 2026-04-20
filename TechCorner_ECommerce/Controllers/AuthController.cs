//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using TechCorner_ECommerce.Data;
//using TechCorner_ECommerce.Helpers;


//namespace TechCorner_ECommerce.Controllers {
//    public class AuthController : Controller {
//        private readonly AppDbContext _context;

//        public AuthController(AppDbContext context) {
//            _context = context;
//        }

//        // ================= REGISTER =================
//        public IActionResult Register() {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Register(string email, string password) {
//            var userExists = await _context.Users.AnyAsync(x => x.Email == email);
//            if (userExists) {
//                ViewBag.Error = "Email already exists";
//                return View();
//            }

//            var user = new User {
//                Email = email,
//                Password = PasswordHelper.Hash(password),
//                Role = "Customer"
//            };

//            _context.Users.Add(user);
//            await _context.SaveChangesAsync();

//            return RedirectToAction("Login");
//        }

//        // ================= LOGIN =================
//        public IActionResult Login() {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Login(string email, string password) {
//            var hashed = PasswordHelper.Hash(password);

//            var user = await _context.Users
//                .FirstOrDefaultAsync(x => x.Email == email && x.Password == hashed);

//            if (user == null) {
//                ViewBag.Error = "Invalid login";
//                return View();
//            }

//            // Claims
//            var claims = new List<Claim>
//            {
//            new Claim(ClaimTypes.Name, user.Email),
//            new Claim(ClaimTypes.Role, user.Role),
//            new Claim("UserId", user.Id.ToString())
//        };

//            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//            var principal = new ClaimsPrincipal(identity);

//            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

//            return RedirectToAction("Index", "Home");
//        }

//        // ================= LOGOUT =================
//        public async Task<IActionResult> Logout() {
//            await HttpContext.SignOutAsync();
//            return RedirectToAction("Login");
//        }

//        public IActionResult AccessDenied() {
//            return View();
//        }
//    }
//}
