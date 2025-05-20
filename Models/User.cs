namespace TennisCourtRentalSystem.Models
{
    public class User
    {
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime DateCreated { get; set; }
    }
}