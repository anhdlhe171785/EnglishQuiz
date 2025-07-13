using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PRN222_EnglishQuiz.Models;
using System.Linq;

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

            // Số người làm từng đề + điểm trung bình
            var examStats = _context.ExamHistories
                .GroupBy(ue => ue.ExamId)
                .ToList() // Thực hiện truy vấn trước
                .Select(g =>
                {
                    var exam = _context.Exams.FirstOrDefault(e => e.Id == g.Key);
                    return new
                    {
                        ExamId = g.Key,
                        ExamTitle = exam != null ? exam.Title : "Unknown",
                        Count = g.Count(),
                        AvgScore = g.Average(x => x.Score)
                    };
                }).ToList();


            // Người có điểm cao nhất
            var topUser = _context.ExamHistories
                .OrderByDescending(ue => ue.Score)
                .Select(ue => new
                {
                    ue.User.UserName,
                    ue.Score,
                    ue.Exam.Title
                })
                .FirstOrDefault();

            ViewBag.Users = users;
            ViewBag.Exams = exams;
            ViewBag.Questions = questions;
            ViewBag.ExamStats = examStats;
            ViewBag.TopUser = topUser;

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
            var selectuser = _context.Users.FirstOrDefault(x => x.Id == id);

            if (selectuser == null)
            {
                TempData["Error"] = "User does not exist.";
                return RedirectToAction("UserManagement");
            }

            if (adminid != null && selectuser.Id == adminid.Value)
            {
                TempData["Error"] = "You cannot delete yourself.";
                return RedirectToAction("UserManagement");
            }

            if (selectuser.Role == "Admin")
            {
                TempData["Error"] = "Unable to delete Admin account.";
                return RedirectToAction("UserManagement");
            }

            // Xóa liên quan
            var histories = _context.ExamHistories
                                    .Where(h => h.UserId == id)
                                    .ToList();

            var historyIds = histories.Select(h => h.Id).ToList();

            var answers = _context.UserAnswers
                                  .Where(a => historyIds.Contains(a.ExamHistoryId.Value))
                                  .ToList();

            _context.UserAnswers.RemoveRange(answers);
            _context.ExamHistories.RemoveRange(histories);
            _context.Users.Remove(selectuser);

            _context.SaveChanges();


            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("UserManagement");
        }


        public IActionResult ExamManagement()
        {
            var exams = _context.Exams.ToList();
            ViewBag.Exams = exams;
            return View();
        }
        [HttpPost]
        public IActionResult AddExam(Exam exam)
        {
            if (exam != null)
            {
                if (!Request.Form.ContainsKey("IsVisible"))
                {
                    exam.IsVisible = false;
                }

                _context.Exams.Add(exam);
                _context.SaveChanges();
                TempData["Success"] = "Add Exam successfully.";
            }
            return RedirectToAction("ExamManagement");
        }
        public IActionResult ExamDetail(int id)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.Id == id);
            ViewBag.Exam = exam;
            return View();
        }
        [HttpPost]
        public IActionResult UpdateExam(Exam exam)
        {
            var slexam = _context.Exams.FirstOrDefault(e => e.Id == exam.Id);
            if (slexam != null)
            {
                slexam.Title = exam.Title;
                slexam.Description = exam.Description;
                slexam.IsVisible = Request.Form.ContainsKey("IsVisible");

                _context.SaveChanges();
                TempData["Success"] = "Update Exam successfully.";
            }
            return RedirectToAction("ExamManagement");
        }
        public IActionResult DeleteExam(int id)
        {
            var exam = _context.Exams.FirstOrDefault(e => e.Id == id);
            if (exam == null)
            {
                TempData["Error"] = "No Exam found.";
                return RedirectToAction("ExamManagement");
            }

            var histories = _context.ExamHistories.Where(h => h.ExamId == id).ToList();

            // 2. Lấy toàn bộ UserAnswers liên quan
            var historyIds = histories.Select(h => h.Id).ToList();
            var answers = _context.UserAnswers
                                  .Where(ua => ua.ExamHistoryId.HasValue && historyIds.Contains(ua.ExamHistoryId.Value))
                                  .ToList();

            _context.UserAnswers.RemoveRange(answers);
            _context.ExamHistories.RemoveRange(histories);

            var questions = _context.Questions.Where(q => q.ExamId == id).ToList();
            foreach (var q in questions)
            {
                q.ExamId = null;
            }
            _context.Questions.UpdateRange(questions);


            // 4. Xóa đề
            _context.Exams.Remove(exam);
            _context.SaveChanges();

            TempData["Success"] = "The Exam and all related data have been deleted.";
            return RedirectToAction("ExamManagement");
        }

        public IActionResult QuestionManagement(string? search, int? examId)
        {
            var questionsQuery = _context.Questions
                .Include(q => q.Exam)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var loweredSearch = search.ToLower();
                questionsQuery = questionsQuery.Where(q =>
                    q.Content.ToLower().Contains(loweredSearch));
            }

            if (examId.HasValue)
            {
                if (examId == -1)
                {
                    questionsQuery = questionsQuery.Where(q => q.ExamId == null);
                }
                else
                {
                    questionsQuery = questionsQuery.Where(q => q.ExamId == examId.Value);
                }
            }


            var questions = questionsQuery.ToList();
            var exams = _context.Exams.ToList();

            ViewBag.Questions = questions;
            ViewBag.Exams = exams;
            ViewBag.Search = search;
            ViewBag.ExamId = examId;

            return View();
        }

        [HttpPost]
        public IActionResult AddQuestion(Question q)
        {
            // Lấy text tương ứng với đáp án đúng
            string? selectedText = q.CorrectOption switch
            {
                "A" => q.OptionA,
                "B" => q.OptionB,
                "C" => q.OptionC,
                "D" => q.OptionD,
                "E" => q.OptionE,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(q.Content) || string.IsNullOrWhiteSpace(q.CorrectOption))
            {
                TempData["Error"] = "Content và CorrectOption không được để trống!";
                return RedirectToAction("QuestionManagement");
            }

            if (string.IsNullOrWhiteSpace(selectedText))
            {
                TempData["Error"] = $"Không thể chọn {q.CorrectOption} làm đáp án đúng vì nội dung bị trống!";
                return RedirectToAction("QuestionManagement");
            }

            _context.Questions.Add(q);
            _context.SaveChanges();
            TempData["Success"] = "Thêm câu hỏi thành công!";
            return RedirectToAction("QuestionManagement");
        }

        [HttpPost]
        public IActionResult UpdateQuestion(Question question)
        {
            var existing = _context.Questions.FirstOrDefault(q => q.Id == question.Id);
            if (existing != null)
            {
                // Kiểm tra nội dung của option đúng
                string? selectedText = question.CorrectOption switch
                {
                    "A" => question.OptionA,
                    "B" => question.OptionB,
                    "C" => question.OptionC,
                    "D" => question.OptionD,
                    "E" => question.OptionE,
                    _ => null
                };

                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    TempData["Error"] = $"Không thể chọn đáp án {question.CorrectOption} làm đáp án đúng vì nội dung bị trống.";
                    return RedirectToAction("QuestionManagement");
                }

                // Gán các giá trị
                existing.ExamId = question.ExamId;
                existing.Category = question.Category;
                existing.Content = question.Content;
                existing.OptionA = question.OptionA;
                existing.OptionB = question.OptionB;
                existing.OptionC = question.OptionC;
                existing.OptionD = question.OptionD;
                existing.OptionE = question.OptionE;
                existing.CorrectOption = question.CorrectOption;

                _context.SaveChanges();
                TempData["Success"] = "Cập nhật câu hỏi thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy câu hỏi để cập nhật.";
            }

            return RedirectToAction("QuestionManagement");
        }


        [HttpPost]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _context.Questions.FirstOrDefault(q => q.Id == id);

            if (question == null)
            {
                TempData["Error"] = "No question found.";
                return RedirectToAction("QuestionManagement");
            }

            // Kiểm tra xem câu hỏi có đang được sử dụng trong UserAnswers không
            bool isUsed = _context.UserAnswers.Any(a => a.QuestionId == id);

            if (isUsed)
            {
                TempData["Error"] = "The Question cannot be deleted because it has already been used in a user's test.";
                return RedirectToAction("QuestionManagement");
            }

            _context.Questions.Remove(question);
            _context.SaveChanges();
            TempData["Success"] = "The Question was successfully deleted.";

            return RedirectToAction("QuestionManagement");
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