using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222_EnglishQuiz.Models;

namespace PRN222_EnglishQuiz.Controllers
{
    public class AdminController : Controller
    {
        private readonly EnglishQuizContext _context;
        public AdminController(EnglishQuizContext context)
        {
            _context = context;
        }
        public IActionResult DashBoard()
        {
            var users = _context.Users.ToList();
            var exams = _context.Exams.ToList();
            var questions = _context.Questions.ToList();

            ViewBag.Users = users;
            ViewBag.Exams = exams;
            ViewBag.Questions = questions;
            return View();
        }
        public IActionResult UserManagement()
        {
            var users = _context.Users.ToList();
            ViewBag.Users = users;
            return View();
        }
        [HttpPost]
        public IActionResult AddUser(User user)
        {
            if (user != null)
            {
                TempData["Success"] = "Add User Successfulyy.";
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            return RedirectToAction("UserManagement");
        }
        [HttpPost]
        public IActionResult UpdateUser(User user)
        {
            var selectuser = _context.Users.FirstOrDefault(x => x.Id == user.Id);
            if (user != null)
            {
                selectuser.UserName = user.UserName;
            }
            _context.Users.Update(selectuser);
            _context.SaveChanges();
            return RedirectToAction("UserManagement");
        }
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            int? adminid = HttpContext.Session.GetInt32("Id");
            var selectuser = _context.Users.FirstOrDefault(x=>x.Id == id);
            if (selectuser.Id == adminid)
            {
                TempData["Error"] = "Bạn không thể xóa chính mình";
                return RedirectToAction("UserManagement");
            }
            if (selectuser.Role == "Admin")
            {
                TempData["Error"] = "Không thể xóa tài khoản Admin";
                return RedirectToAction("UserManagement");
            }
            _context.Users.Remove(selectuser);
            _context.SaveChanges();

            TempData["Success"] = "Xóa người dùng thành công.";
            return RedirectToAction("UserManagement");
        }
        public IActionResult ExamManagement()
        {
            var exams = _context.Exams.ToList();
            ViewBag.Exams = exams;
            return View();
        }
        public IActionResult QuestionManagement()
        {
            var questions = _context.Questions.Include(e=>e.Exam)
                                              .ToList();
            var exams = _context.Exams.ToList();

            ViewBag.Questions = questions;
            ViewBag.Exams = exams;
            return View();
        }
        public IActionResult QuestionDetail(int id)
        {
            var question = _context.Questions.FirstOrDefault(x => x.Id == id);
            ViewBag.Exams = _context.Exams.ToList();
            ViewBag.QuestionDetail = question;
            return View();
        }

    }
}
