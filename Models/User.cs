using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRN222_EnglishQuiz.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]

    public string? Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;


    [StringLength(100, ErrorMessage = "Address can't exceed 100 characters")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(20, ErrorMessage = "Phone number is too long")]
    public string? Phone { get; set; }

    public virtual ICollection<ExamHistory> ExamHistories { get; set; } = new List<ExamHistory>();
}
