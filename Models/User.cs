using System;
using System.Collections.Generic;

namespace PRN222_EnglishQuiz.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<ExamHistory> ExamHistories { get; set; } = new List<ExamHistory>();
}
