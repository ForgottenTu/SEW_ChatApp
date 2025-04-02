using Microsoft.AspNetCore.Identity;

namespace ChatApp.Model.Mdels;

public class ApplicationUser: IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}