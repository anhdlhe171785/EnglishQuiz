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
            int? userId = HttpContext.Session.GetInt32("Id");

            var exams = _context.Exams
                                .Where(e => e.IsVisible == true)
                                .ToList();

            Dictionary<int, int> examAttemptCounts = new();

            if (userId != null)
            {
                examAttemptCounts = _context.ExamHistories
                    .Where(eh => eh.UserId == userId && eh.ExamId.HasValue)
                    .GroupBy(eh => eh.ExamId.Value)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            ViewBag.Exams = exams;
            ViewBag.ExamAttemptCounts = examAttemptCounts;

            return View();
        }


        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult TakeExam(int id)
        {
            // Kiểm tra session đăng nhập
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Ngăn trình duyệt cache trang
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Lấy bài thi theo ID
            var exam = _context.Exams.FirstOrDefault(e => e.Id == id);
            if (exam == null) return NotFound();

            // Lấy danh sách câu hỏi
            List<Question> questions;
            if (exam.IsRandom == true)
            {
                questions = _context.Questions
                    .OrderBy(r => Guid.NewGuid())
                    .Take(20)
                    .ToList();
            }
            else
            {
                questions = _context.Questions
                    .Where(q => q.ExamId == id)
                    .ToList();
            }

            // Trộn nội dung các phương án nhưng giữ label A–E
            var shuffledAnswers = new Dictionary<int, List<(string, string)>>();

            foreach (var q in questions)
            {
                var options = new List<(string, string)>
        {
            ("A", q.OptionA),
            ("B", q.OptionB),
            ("C", q.OptionC),
            ("D", q.OptionD)
        };

                if (!string.IsNullOrWhiteSpace(q.OptionE))
                {
                    options.Add(("E", q.OptionE));
                }

                // Chỉ trộn nội dung, giữ nguyên label
                var texts = options
                    .Where(opt => !string.IsNullOrWhiteSpace(opt.Item2))
                    .Select(opt => opt.Item2)
                    .OrderBy(x => Guid.NewGuid())
                    .ToList();

                var shuffled = new List<(string, string)>();
                for (int i = 0; i < texts.Count; i++)
                {
                    shuffled.Add((options[i].Item1, texts[i]));
                }

                shuffledAnswers[q.Id] = shuffled;
            }

            // Truyền dữ liệu qua ViewBag
            ViewBag.Exam = exam;
            ViewBag.Questions = questions;
            ViewBag.ShuffledAnswers = shuffledAnswers;
            ViewBag.Step = 1;
            ViewBag.TimeLimitMinutes = 20;

            return View();
        }



        [HttpPost]
        public IActionResult SubmitExam(IFormCollection form)
        {
            int? id = HttpContext.Session.GetInt32("Id");
            if (id == null)
            {
                return RedirectToAction("Login", "Home");
            }

            HttpContext.Session.SetString("AlreadySubmitted", "true");

            int examId = int.Parse(form["examId"]);
            var questionIds = form["questionIds"].Select(int.Parse).Distinct().ToList();
            var questions = _context.Questions
                                    .Where(q => questionIds.Contains(q.Id))
                                    .ToList();

            int total = questions.Count;
            int correctCount = 0;

            foreach (var question in questions)
            {
                string userAnswer = form[$"answers[{question.Id}]"];
                if (!string.IsNullOrEmpty(userAnswer) && userAnswer == question.CorrectOption)
                {
                    correctCount++;
                }
            }

            double score = (double)correctCount / total * 10;

            // Truyền sang View
            ViewBag.Questions = questions;
            ViewBag.UserAnswers = form;
            ViewBag.Total = total;
            ViewBag.Correct = correctCount;
            ViewBag.Score = Math.Round(score, 2);

            // Ghi vào ExamHistories
            var examHistory = new ExamHistory
            {
                UserId = id.Value,
                ExamId = examId,
                Score = (int)Math.Round(score),
                DateTaken = DateTime.Now
            };
            _context.ExamHistories.Add(examHistory);
            _context.SaveChanges();

            // Ghi từng câu trả lời vào UserAnswers
            foreach (var question in questions)
            {
                string userAnswer = form[$"answers[{question.Id}]"];

                var userAnswerEntity = new UserAnswer
                {
                    ExamHistoryId = examHistory.Id,
                    QuestionId = question.Id,
                    SelectedOption = string.IsNullOrEmpty(userAnswer) ? "X" : userAnswer
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
