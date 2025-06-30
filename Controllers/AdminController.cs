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
