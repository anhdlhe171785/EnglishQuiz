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
            var userselect = _context.Users.FirstOrDefault(u => u.Id == id);
            if (userselect != null)
            {
                userselect.UserName = user.UserName;
                userselect.PasswordHash = user.PasswordHash;
                userselect.Address = user.Address;
                userselect.Phone = user.Phone;

                _context.Users.Update(userselect);
                _context.SaveChanges();
                HttpContext.Session.SetString("UserName", user.UserName);

                if (userselect.Role == "Admin")
                {
                    return RedirectToAction("DashBoard", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Exam");
                }
            }

            return RedirectToAction("Profile");
        }
        public IActionResult CancelRedirect(string role)
        {
            if (role == "Admin")
            {
                return RedirectToAction("DashBoard", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Exam");
            }
        }

        public IActionResult ExamHistory(int? examId, string sortOrder, DateTime? examDate)
        {
            int? userid = HttpContext.Session.GetInt32("Id");

            var histories = _context.ExamHistories
                                    .Include(e => e.Exam)
                                    .Where(x => x.UserId == userid);

            if (examId.HasValue)
            {
                histories = histories.Where(h => h.ExamId == examId.Value);
            }

            // 👉 Lọc theo ngày thi chính xác
            if (examDate.HasValue)
            {
                histories = histories.Where(h => h.DateTaken.HasValue && h.DateTaken.Value.Date == examDate.Value.Date);
            }

            // Sắp xếp
            ViewBag.CurrentSort = sortOrder;
            ViewBag.ScoreSortParam = sortOrder == "score_desc" ? "score_asc" : "score_desc";
            ViewBag.DateSortParam = sortOrder == "date_desc" ? "date_asc" : "date_desc";

            histories = sortOrder switch
            {
                "score_asc" => histories.OrderBy(h => h.Score),
                "score_desc" => histories.OrderByDescending(h => h.Score),
                "date_asc" => histories.OrderBy(h => h.DateTaken),
                "date_desc" => histories.OrderByDescending(h => h.DateTaken),
                _ => histories.OrderByDescending(h => h.DateTaken),
            };

            ViewBag.ExamHistory = histories.ToList();
            ViewBag.ExamList = _context.Exams.ToList();
            ViewBag.SelectedExamId = examId;
            ViewBag.SelectedDate = examDate?.ToString("yyyy-MM-dd");

            return View();
        }



        [HttpPost]
        public IActionResult Detail(int id)
        {
            var examHistory = _context.ExamHistories
                .Include(h => h.Exam)
                .FirstOrDefault(h => h.Id == id);

            if (examHistory == null) return NotFound();

            var userAnswers = _context.UserAnswers
                .Include(ua => ua.Question)
                .Where(ua => ua.ExamHistoryId == id)
                .ToList();

            ViewBag.Exam = examHistory.Exam;
            ViewBag.UserAnswers = userAnswers;
            ViewBag.Score = examHistory.Score;
            ViewBag.DateTaken = examHistory.DateTaken;

            return View("ExamDetail");
        }
    }
}