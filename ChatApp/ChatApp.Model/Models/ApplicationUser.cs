using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Model.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(40)] public string DisplayName { get; set; } = default!;
}