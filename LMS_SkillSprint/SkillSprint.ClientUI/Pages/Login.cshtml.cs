using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillSprint.ClientUI.Services;

public class LoginModel : PageModel
{
    private readonly IAuthService _auth;

    public LoginModel(IAuthService auth) => _auth = auth;

    [BindProperty] public InputModel Input { get; set; } = new();
    public string? Error { get; set; }

    public class InputModel
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _auth.LoginAsync(Input.Email, Input.Password);
        if (!result.IsSuccess)
        {
            Error = result.Problem?.Detail ?? result.Error ?? "Login failed.";
            return Page();
        }

        // Build cookie principal (you can decode JWT to claims or request user info from API)
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, Input.Email),
            // Map roles/ids from API as needed
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToPage("/Dashboard");
    }
}
