using System.ComponentModel.DataAnnotations;

namespace DoubleFactorAuthenticationHomeWork.ViemModels
{
    public class VerifyTwoFactorTokenViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }


}
