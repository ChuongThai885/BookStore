using Microsoft.AspNetCore.Identity;

namespace Authentication.Domain.Entity
{
    public class ApplicationUserEntity : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
    }
}
