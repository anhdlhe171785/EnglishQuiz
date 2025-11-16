using Microsoft.AspNetCore.Mvc;
using PRN222_EnglishQuiz.Models;

namespace PRN222_EnglishQuiz.Controllers
{
    public class HomeController : Controller
    {
        private readonly EnglishQuizContext _context;

        public HomeController(EnglishQuizContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("Id") != null)
            {
                return RedirectToAction("Index", "Exam");
            }

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            var checkuser = _context.Users
                .FirstOrDefault(u => u.Email == user.Email && u.PasswordHash == user.PasswordHash);

            if (checkuser != null)
            {
                HttpContext.Session.SetInt32("Id", checkuser.Id);
                HttpContext.Session.SetString("UserName", checkuser.UserName ?? checkuser.Email);
                HttpContext.Session.SetString("Role", checkuser.Role);

                TempData["LoginSuccess"] = "Login successful!";

                return checkuser.Role == "Admin"
                    ? RedirectToAction("DashBoard", "Admin")
                    : RedirectToAction("Index", "Exam");
            }

            ViewBag.LoginError = "Email or Password is incorrect!";
            return View(user);
        }
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(User user, string ConfirmPassword)
        {
            if (user.PasswordHash != ConfirmPassword)
            {
                ViewData["ConfirmError"] = "Re-entered password does not match";
                return View(user);
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists in the system");
                return View(user);
            }

            user.Role = "User";
            user.UserName = "";

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["SignupSuccess"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email, string phone, string newPassword, string confirmPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Phone == phone);
            if (user == null)
            {
                ViewBag.Error = "Email or phone number is incorrect!";
                return View();
            }
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "New password must have at least 6 characters!";
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Confirm Password is wrong!";
                return View();
            }

            user.PasswordHash = newPassword;
            _context.SaveChanges();

            TempData["Success"] = "Password changed successfully! Please log in again.";
            return View();
        }
    }
}
