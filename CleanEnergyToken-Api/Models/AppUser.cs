using Microsoft.AspNetCore.Identity;

namespace CleanEnergyToken_Api.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PK { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
