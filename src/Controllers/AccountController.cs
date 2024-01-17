namespace Miniblog.Web.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Models;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

[Authorize]
public class AccountController : Controller
{
    private readonly IConfiguration config;

    public AccountController(IConfiguration config)
    {
        this.config = config;
    }

    [Route("/login")]
    [AllowAnonymous]
    [HttpGet]
    [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "MVC binding")]
    public IActionResult Login(string? returnUrl = null)
    {
        this.ViewData[Constants.ReturnUrl] = returnUrl;
        return this.View();
    }

    [Route("/login")]
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "MVC binding")]
    public async Task<IActionResult> LoginAsync(string? returnUrl, LoginViewModel? model)
    {
        this.ViewData[Constants.ReturnUrl] = returnUrl;

        if (model is null || model.UserName is null || model.Password is null)
        {
            this.ModelState.AddModelError(string.Empty, Properties.Resources.UsernameOrPasswordIsInvalid);
            return this.View(nameof(Login), model);
        }

        if (!this.ModelState.IsValid || !this.ValidateUser(model.UserName, model.Password))
        {
            this.ModelState.AddModelError(string.Empty, Properties.Resources.UsernameOrPasswordIsInvalid);
            return this.View(nameof(Login), model);
        }

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.Name, model.UserName));

        var principle = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties { IsPersistent = model.RememberMe };
        await this.HttpContext.SignInAsync(principle, properties).ConfigureAwait(false);

        return this.LocalRedirect(returnUrl ?? "/");
    }

    [Route("/logout")]
    public async Task<IActionResult> LogOutAsync()
    {
        await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        return this.LocalRedirect("/");
    }

    public bool ValidateUser(string username, string password) =>
        username == this.config[Constants.Config.User.UserName] && this.VerifyHashedPassword(password, this.config);

    private bool VerifyHashedPassword(string password, IConfiguration config)
    {
        var saltBytes = Encoding.UTF8.GetBytes(config[Constants.Config.User.Salt]!);

        var hashBytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 1000,
            numBytesRequested: 256 / 8);

        var hashText = BitConverter.ToString(hashBytes).Replace(Constants.Dash, string.Empty, StringComparison.OrdinalIgnoreCase);
        return hashText == config[Constants.Config.User.Password];
    }
}
