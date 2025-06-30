using Microsoft.EntityFrameworkCore;
using PRN222_EnglishQuiz.Models; // Namespace  EnglishQuizContext

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();  // Add session services

// SignUp DbContext here
builder.Services.AddDbContext<EnglishQuizContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=DashBoard}/{id?}");

app.Run();
