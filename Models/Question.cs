﻿using System;
using System.Collections.Generic;

namespace PRN222_EnglishQuiz.Models;

public partial class Question
{
    public int Id { get; set; }

    public int? ExamId { get; set; }

    public string Content { get; set; } = null!;

    public string OptionA { get; set; } = null!;

    public string OptionB { get; set; } = null!;

    public string OptionC { get; set; } = null!;

    public string OptionD { get; set; } = null!;

    public string CorrectOption { get; set; } = null!;

    public string? Category { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
