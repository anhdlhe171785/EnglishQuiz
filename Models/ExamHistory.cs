using System;
using System.Collections.Generic;

namespace PRN222_EnglishQuiz.Models;

public partial class ExamHistory
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? ExamId { get; set; }

    public int? Score { get; set; }

    public DateTime? DateTaken { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
