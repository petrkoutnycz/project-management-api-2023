using Microsoft.AspNetCore.Authentication;
using ProjectManager.Data.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProjectManager.Api.Controllers.Models.Auth;
using ProjectManager.Data.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using ProjectManager.Api.Services;

namespace ProjectManager.Api.Controllers;
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IClock _clock;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(
        IClock clock,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
        )
    {
        _clock = clock; 
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("api/v1/Auth/Register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(
       [FromBody] RegisterModel model
       )
    {
        var validator = new PasswordValidator<ApplicationUser>();
        var now = _clock.GetCurrentInstant();

        var newUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = model.Name,
            Email = model.Email,
            UserName = model.Email,
            EmailConfirmed = true,
        }.SetCreateBySystem(now);

        var checkPassword = await validator.ValidateAsync(_userManager, newUser, model.Password);

        if (!checkPassword.Succeeded)
        {
            ModelState.AddModelError<RegisterModel>(
                x => x.Password, string.Join("\n", checkPassword.Errors.Select(x => x.Description)));
            return ValidationProblem(ModelState);
        }

        await _userManager.CreateAsync(newUser);
        await _userManager.AddPasswordAsync(newUser, model.Password);

        return Ok();
    }

    [HttpPost("api/v1/Auth/Login")]
    public async Task<ActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Email);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "LOGIN_FAILED");
            return ValidationProblem(ModelState);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "LOGIN_FAILED");
            return ValidationProblem(ModelState);
        }

        var userPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        await HttpContext.SignInAsync(userPrincipal);

        return Ok();
    }

    [HttpGet("api/v1/Auth/UserInfo")]
    public async Task<ActionResult<string>> UserInfo()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            throw new InvalidOperationException("user not logged in");
        }
        var name = User.Claims.First(x => x.Type == ClaimTypes.Name).Value;

        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            throw new InvalidOperationException("user not logged in");
        }
        var idString = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var guid = Guid.Parse(idString);
        return Ok($"{name} ({guid})");
    }

    [Authorize]
    [HttpPost("api/v1/Auth/Logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    [HttpGet("api/v1/Auth/TestMail")]
    public async Task<ActionResult> Test(
        [FromServices] EmailSenderService service
        )
    {
        await service.AddEmailToSendAsync("test@test.cz", "Suuuubject", "<h1>Aaaaaaaaaaa</h1>");
        return Ok();
    }
 }
