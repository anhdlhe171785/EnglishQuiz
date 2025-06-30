using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222_EnglishQuiz.Models;


namespace PRN222_EnglishQuiz.Controllers
{
    public class UserController : Controller
    {
        private readonly EnglishQuizContext _context;
        
        public UserController(EnglishQuizContext context)
        {
            _context = context;
        }
        public IActionResult Profile()
        {
            int? id = HttpContext.Session.GetInt32("Id");
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View(user);
        }
        [HttpPost]
        public IActionResult UpdateProfile(User user)
        {
            int? id = HttpContext.Session.GetInt32("Id");
            var userselect = _context.Users.FirstOrDefault(u=> u.Id == id);
            if (userselect != null)
            {
                userselect.UserName = user.UserName;
                userselect.PasswordHash = user.PasswordHash;
            }
            _context.Users.Update(userselect);
            _context.SaveChanges();
            return RedirectToAction("Profile");
        }
        public IActionResult ExamHistory()
        {
            int? userid = HttpContext.Session.GetInt32("Id");
            var history = _context.ExamHistories.Include(e => e.Exam)
                                                .Where(x => x.UserId == userid)
                                                .ToList();
            ViewBag.ExamHistory = history;
            return View();
        }
        [HttpPost]
        public IActionResult Detail(int id)
        {

            return View();
        }

    }
}