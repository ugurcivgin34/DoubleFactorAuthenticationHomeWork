namespace DoubleFactorAuthenticationHomeWork.ViemModels
{
    public class UserViewModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastLogoutTime { get; set; }
        public bool LastLogoutWithoutVerification { get; set; }
    }

}
