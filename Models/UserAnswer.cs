using System;
using System.Collections.Generic;

namespace PRN222_EnglishQuiz.Models;

public partial class UserAnswer
{
    public int Id { get; set; }

    public int? ExamHistoryId { get; set; }

    public int? QuestionId { get; set; }

    public string? SelectedOption { get; set; }

    public virtual ExamHistory? ExamHistory { get; set; }

    public virtual Question? Question { get; set; }
}
