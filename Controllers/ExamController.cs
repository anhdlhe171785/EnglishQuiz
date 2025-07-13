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
