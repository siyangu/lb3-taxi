using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaxiService.Auth.Services;
using Xunit;

namespace TaxiService.Auth.Tests;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IAuthService _auth;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        var app = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddSingleton<IAuthService, AuthService>()));

        _client = app.CreateClient();
        _auth = app.Services.GetRequiredService<IAuthService>();
    }
    public void Dispose() => _auth.ClearDriverDatabase();

    // ── POST /api/taxi/register ───────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidData_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/taxi/register",
            new { callsign = "driver_1", password = "123456pass" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_Duplicate_Returns400()
    {
        await _client.PostAsJsonAsync("/api/taxi/register",
            new { callsign = "driver_duplicate", password = "123456pass" });

        var response = await _client.PostAsJsonAsync("/api/taxi/register",
            new { callsign = "driver_duplicate", password = "123456pass" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── POST /api/taxi/login ──────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        await _client.PostAsJsonAsync("/api/taxi/register",
            new { callsign = "driver_3", password = "123456pass" });

        var response = await _client.PostAsJsonAsync("/api/taxi/login",
            new { callsign = "driver_3", password = "123456pass" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await _client.PostAsJsonAsync("/api/taxi/register",
            new { callsign = "driver_4", password = "123456pass" });

        var response = await _client.PostAsJsonAsync("/api/taxi/login",
            new { callsign = "driver_4", password = "wrong_password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}