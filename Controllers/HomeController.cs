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

                TempData["LoginSuccess"] = "Đăng nhập thành công!";

                return checkuser.Role == "Admin"
                    ? RedirectToAction("DashBoard", "Admin")
                    : RedirectToAction("Index", "Exam");
            }

            ViewBag.LoginError = "Email hoặc mật khẩu sai!";
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
                ViewData["ConfirmError"] = "Mt khu nhp li không khớp";
                return View(user);
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đ tn ti trong h thng");
                return View(user);
            }

            user.Role = "User";
            user.UserName = user.Email;

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["SignupSuccess"] = "Đng k thnh cng! Vui lng đng nhp.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
