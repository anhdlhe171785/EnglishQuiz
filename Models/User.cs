using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRN222_EnglishQuiz.Models;

public partial class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<ExamHistory> ExamHistories { get; set; } = new List<ExamHistory>();
}
