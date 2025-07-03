using Microsoft.AspNetCore.Mvc;
using PRN222_EnglishQuiz.Models;
using System.Diagnostics;

namespace PRN222_EnglishQuiz.Controllers
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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

        public IActionResult Privacy()
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
            var checkuser = _context.Users.FirstOrDefault(u => u.Email == user.Email && u.PasswordHash == user.PasswordHash);

            if (checkuser != null)
            {
                // Save session
                HttpContext.Session.SetInt32("Id", checkuser.Id);
                HttpContext.Session.SetString("UserName", checkuser.UserName);
                HttpContext.Session.SetString("Role", checkuser.Role);

                ViewBag.LoginSuccess = true;
                return View();
            }
            ViewBag.LoginError = "Email or Password was wrong!";
            return View(user);
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
