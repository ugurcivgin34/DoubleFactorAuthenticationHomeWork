using DoubleFactorAuthenticationHomeWork.Context;
using DoubleFactorAuthenticationHomeWork.Models;
using DoubleFactorAuthenticationHomeWork.Utility;
using FluentEmail.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// SMTP ayarlarýný yapýn
var smtpClient = new SmtpClient(builder.Configuration["EmailSettings:Host"])
{
    UseDefaultCredentials = false,
    Port = int.Parse(builder.Configuration["EmailSettings:Port"]),
    Credentials = new NetworkCredential(builder.Configuration["EmailSettings:Username"], builder.Configuration["EmailSettings:Password"]),
    EnableSsl = true,
};

var smtpSender = new SmtpSender(() => smtpClient);

builder.Services
    .AddFluentEmail(builder.Configuration["EmailSettings:FromEmail"])
    .AddRazorRenderer()
    .AddSmtpSender(smtpClient);

builder.Services.AddRazorPages();

builder.Services.AddTransient<DataSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapRazorPages();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = services.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
