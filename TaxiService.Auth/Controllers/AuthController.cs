using TaxiService.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace TaxiService.Auth.Controllers;

[ApiController]
[Route("api/taxi")] 
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;
    // Модели данных для запросов
    public record RegisterRequest(string Callsign, string Password);
    public record LoginRequest(string Callsign, string Password);
    public record ChangePasswordRequest(string Callsign, string OldPassword, string NewPassword);

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        var result = _auth.RegisterDriver(req.Callsign, req.Password);
        return result.Success ? Created("", result) : BadRequest(result);
    }
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var result = _auth.LoginDriver(req.Callsign, req.Password);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
    [HttpPost("change-password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var result = _auth.ChangeDriverPassword(req.Callsign, req.OldPassword, req.NewPassword);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpGet("active-drivers")]
    public IActionResult GetUsers() => Ok(new { Drivers = _auth.GetActiveDrivers() });
}