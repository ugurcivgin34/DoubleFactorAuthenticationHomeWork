using DoubleFactorAuthenticationHomeWork.Models;
using DoubleFactorAuthenticationHomeWork.ViemModels;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IFluentEmail _emailSender;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger,
        IFluentEmail emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }
    private static Random random = new Random();

    private int GenerateRandomCode()
    {
        return random.Next(100000, 999999);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                return RedirectToAction("Login", "Account");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }


    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        _logger.LogInformation($"Login attempt for email: {model.Email}");
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            _logger.LogInformation($"User found for email: {model.Email}");
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User signed in: {model.Email}");


                // Kullanıcı admin mi kontrol et
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    user.LastLoginTime = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction("Index", "Admin");
                }

                int code = GenerateRandomCode();
                user.TwoFactorCode = code;
                user.LastLoginTime = DateTime.Now;
                await _userManager.UpdateAsync(user);

                await _emailSender
                    .To(model.Email)
                    .Subject("Giriş Doğrulama Kodu")
                    .Body($"Doğrulama kodunuz: {code}")
                    .SendAsync();

                return RedirectToAction(nameof(VerifyTwoFactorToken), new { email = model.Email });
            }
            else
            {
                _logger.LogWarning($"Failed login attempt for user: {model.Email}");
            }
        }
        else
        {
            _logger.LogWarning($"No user found for email: {model.Email}");
        }
        ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
        return View(model);
    }

    [HttpGet]
    public IActionResult VerifyTwoFactorToken(string email)
    {
        var model = new VerifyTwoFactorTokenViewModel { Email = email };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyTwoFactorToken(VerifyTwoFactorTokenViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (user.TwoFactorCode == model.Token)
                {
                    user.IsTwoFactorAuthenticated = true;
                    await _userManager.UpdateAsync(user);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid token.");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            user.LastLogoutWithoutVerification = true; // Doğrulama yapmadan çıkış
            var admin = await _userManager.GetUsersInRoleAsync("Admin");
            if (admin.Any())
            {
                var adminUser = admin.First();
                await _emailSender
                    .To(adminUser.Email)
                    .Subject("Doğrulama Yapılmadan Çıkış")
                    .Body($"Kullanıcı {user.Email} doğrulama yapmadan çıkış yaptı.")
                    .SendAsync();
            }

            user.LastLogoutTime = DateTime.Now;
            await _userManager.UpdateAsync(user);
            user.IsTwoFactorAuthenticated = false;

            await _signInManager.SignOutAsync();

        }
        return RedirectToAction(nameof(AccountController.Login), "Account");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendVerificationCode()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            int code = GenerateRandomCode();
            user.TwoFactorCode = code;
            await _userManager.UpdateAsync(user);
            await _emailSender
                .To(user.Email)
                .Subject("Çıkış Doğrulama Kodu")
                .Body($"Doğrulama kodunuz: {code}")
                .SendAsync();

            TempData["VerificationSent"] = "Doğrulama kodu e-posta adresinize gönderildi.";
        }
        return RedirectToAction(nameof(VerifyTwoFactorToken), new { email = user.Email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyToken(string token)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, token);
            if (result)
            {
                user.IsTwoFactorAuthenticated = true;
                await _userManager.UpdateAsync(user);
                TempData["VerificationSuccess"] = "Çıkış doğrulandı. Artık güvenle çıkış yapabilirsiniz.";
            }
            else
            {
                TempData["VerificationFailed"] = "Geçersiz doğrulama kodu.";
            }
        }
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendVerificationCodeForLogout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            int code = GenerateRandomCode();
            user.TwoFactorCode = code;
            user.LastLogoutWithoutVerification = false;
            await _userManager.UpdateAsync(user);
            await _emailSender
                .To(user.Email)
                .Subject("Çıkış Doğrulama Kodu")
                .Body($"Doğrulama kodunuz: {code}")
                .SendAsync();

        }
        return RedirectToAction(nameof(VerifyTwoFactorTokenForLogout), new { email = user.Email });
    }

    [HttpGet]
    public IActionResult VerifyTwoFactorTokenForLogout(string email)
    {
        var model = new VerifyTwoFactorTokenViewModel { Email = email };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyTwoFactorTokenForLogout(VerifyTwoFactorTokenViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (user.TwoFactorCode == model.Token)
                {
                    user.IsTwoFactorAuthenticated = true;
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction("Login", "Account");
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid token.");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            user.LastLogoutTime = DateTime.Now;
            user.LastLogoutWithoutVerification = false;
            await _userManager.UpdateAsync(user);
            await _signInManager.SignOutAsync();
        }
        return RedirectToAction(nameof(AccountController.Login), "Account");
    }
}

