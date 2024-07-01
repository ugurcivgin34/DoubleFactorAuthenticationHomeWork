using Microsoft.AspNetCore.Identity;

namespace DoubleFactorAuthenticationHomeWork.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastLogoutTime { get; set; }
        public bool IsTwoFactorAuthenticated { get; set; }
        public bool LastLogoutWithoutVerification { get; set; }
        public int TwoFactorCode { get; set; } 
    }

}
