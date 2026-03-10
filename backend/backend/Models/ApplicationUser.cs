using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    public class ApplicationUser:IdentityUser
    {
        public ICollection<UserDocument> Documents { get; set; } = new List<UserDocument>();
    }
}
