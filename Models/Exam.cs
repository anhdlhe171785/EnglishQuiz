using System;
using System.Collections.Generic;

namespace PRN222_EnglishQuiz.Models;

public partial class Exam
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsRandom { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsVisible { get; set; }

    public virtual ICollection<ExamHistory> ExamHistories { get; set; } = new List<ExamHistory>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
