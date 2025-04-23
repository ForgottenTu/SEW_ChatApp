// File: Controllers/AccountController.cs

using System.ComponentModel.DataAnnotations;
using ChatApp.Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ILogger<AccountController> _log;

    public AccountController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        ILogger<AccountController> log)
    {
        _users  = users;
        _signIn = signIn;
        _log    = log;
    }

    /*──────────────────────────────  Register  ──────────────────────────────*/

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = new ApplicationUser
        {
            UserName    = dto.Email,
            Email       = dto.Email,
            DisplayName = dto.DisplayName
        };

        var result = await _users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _signIn.SignInAsync(user, isPersistent: false);
        _log.LogInformation("New user {Email} registered.", dto.Email);

        return Ok(new { success = true });
    }

    /*──────────────────────────────  Login  ─────────────────────────────────*/

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _signIn.PasswordSignInAsync(
            dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: true);

        if (!result.Succeeded)
            return Unauthorized("Invalid credentials.");

        _log.LogInformation("User {Email} signed in.", dto.Email);
        return Ok(new { success = true });
    }

    /*──────────────────────────────  Logout  ────────────────────────────────*/

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return Ok();
    }
}

/*─────────────────────────────  DTO classes  ───────────────────────────────*/

public sealed class RegisterRequest
{
    [Required, StringLength(32)]
    public string DisplayName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";
}

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
