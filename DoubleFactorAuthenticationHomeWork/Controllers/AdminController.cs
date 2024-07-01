using DoubleFactorAuthenticationHomeWork.Models;
using DoubleFactorAuthenticationHomeWork.ViemModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoubleFactorAuthenticationHomeWork.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.Select(u => new UserViewModel
            {
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                LastLoginTime = u.LastLoginTime,
                LastLogoutTime = u.LastLogoutTime,
                LastLogoutWithoutVerification = u.LastLogoutWithoutVerification
            }).ToList();

            return View(users);
        }
    }


}
