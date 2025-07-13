using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRN222_EnglishQuiz.Models;

public partial class EnglishQuizContext : DbContext
{
    public EnglishQuizContext()
    {
    }

    public EnglishQuizContext(DbContextOptions<EnglishQuizContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamHistory> ExamHistories { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server =(local); database =EnglishQuiz; uid=sa;pwd=123;Trusted_Connection=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exams__3214EC0716532C28");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsRandom).HasDefaultValue(false);
            entity.Property(e => e.IsVisible).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<ExamHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ExamHist__3214EC07EBFF0D25");

            entity.Property(e => e.DateTaken)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamHistories)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__ExamHisto__ExamI__4316F928");

            entity.HasOne(d => d.User).WithMany(p => p.ExamHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ExamHisto__UserI__440B1D61");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07AE43AB7F");

            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CorrectOption)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__Questions__ExamI__44FF419A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07D67C87B7");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserAnsw__3214EC0740B72383");

            entity.Property(e => e.SelectedOption)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.ExamHistory).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.ExamHistoryId)
                .HasConstraintName("FK__UserAnswe__ExamH__45F365D3");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__UserAnswe__Quest__46E78A0C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
