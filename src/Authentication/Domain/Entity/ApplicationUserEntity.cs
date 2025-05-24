using Microsoft.AspNetCore.Identity;

namespace Authentication.Domain.Entity
{
    public class ApplicationUserEntity : IdentityUser
    {
        public string? DisplayName { get; set; }
    }
}
