using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net;
using System.Net.Mail;

var sender = new SmtpSender(() => new SmtpClient("smtp.gmail.com")
{
    UseDefaultCredentials = false,
    Port = 587,
    Credentials = new NetworkCredential("u.civgin@gmail.com", "ifcfcxyfwypvcknx"),
    EnableSsl = true,
});

Email.DefaultSender = sender;

var email = await Email
    .From("u.civgin@gmail.com")
    .To("u.civgin@gmail.com", "Recipient")
    .Subject("Test Email")
    .Body("This is a test email.")
    .SendAsync();

Console.WriteLine(email.Successful ? "Email sent successfully!" : "Failed to send email.");
