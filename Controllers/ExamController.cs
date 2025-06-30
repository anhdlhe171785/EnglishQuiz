using Microsoft.AspNetCore.Mvc;
using PRN222_EnglishQuiz.Models;

namespace PRN222_EnglishQuiz.Controllers
{
    public class ExamController : Controller
    {
        private readonly EnglishQuizContext _context;
        public ExamController(EnglishQuizContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.Exams = _context.Exams.ToList();
            return View();
        }
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult TakeExam(int id)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Home");
            }
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var exam = _context.Exams.FirstOrDefault(e => e.Id == id);
            if (exam == null) return NotFound();

            List<Question> questions;

            if (exam.IsRandom == true)
            {
                questions = _context.Questions
                            .OrderBy(r => Guid.NewGuid())
                            .Take(5)
                            .ToList();
            }
            else
            {
                questions = _context.Questions
                            .Where(q => q.ExamId == id)
                            .ToList();
            }
            ViewBag.Exam = exam;
            ViewBag.Questions = questions;
            ViewBag.Step = 1;
            ViewBag.TimeLimitMinutes = 0.5;

            return View();
        }
        [HttpPost]
        public IActionResult SubmitExam(IFormCollection form)
        {
            int? id = HttpContext.Session.GetInt32("Id");

            HttpContext.Session.SetString("AlreadySubmitted", "true");

            // 1. Lấy ID đề thi
            int examId = int.Parse(form["examId"]);

            // 2. Lấy danh sách câu hỏi đã hiển thị
            var questionIds = form["questionIds"].Select(int.Parse).Distinct().ToList();
            var questions = _context.Questions
                                    .Where(q => questionIds.Contains(q.Id))
                                    .ToList();

            int total = questions.Count;
            int correctCount = 0;

            // 3. Duyệt từng câu hỏi để chấm điểm
            foreach (var question in questions)
            {
                string userAnswer = form[$"answers[{question.Id}]"];
                if (!string.IsNullOrEmpty(userAnswer) && userAnswer == question.CorrectOption)
                {
                    correctCount++;
                }
            }

            ViewBag.Questions = questions;
            ViewBag.UserAnswers = form; // hoặc lưu tạm vào Dictionary nếu cần

            // 4. Tính điểm
            double score = (double)correctCount / total * 10;

            // 5. Truyền sang view kết quả
            ViewBag.Total = total;
            ViewBag.Correct = correctCount;
            ViewBag.Score = Math.Round(score, 2);

            // 6. Lưu lịch sử làm bài vào bảng ExamHistory
            var examHistory = new ExamHistory
            {
                UserId = id.Value, // id lấy từ Session, là ID người dùng
                ExamId = examId, // luôn có giá trị, kể cả khi là đề ngẫu nhiên
                Score = (int)Math.Round(score), // vì Score trong model là int?
                DateTaken = DateTime.Now
            };
            _context.ExamHistories.Add(examHistory);
            _context.SaveChanges(); // Lưu để có Id của examHistory

            // 7. Lưu từng câu trả lời vào bảng UserAnswers
            foreach (var question in questions)
            {
                string userAnswer = form[$"answers[{question.Id}]"];

                var userAnswerEntity = new UserAnswer
                {
                    ExamHistoryId = examHistory.Id,
                    QuestionId = question.Id,
                    SelectedOption = userAnswer ?? ""
                };

                _context.UserAnswers.Add(userAnswerEntity);
            }
            _context.SaveChanges();

            return View("Result");
        }

        public IActionResult Result()
        {
            return View();
        }

    }
}
