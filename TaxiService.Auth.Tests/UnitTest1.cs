using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace TaxiService.Auth.Tests;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string SharedPassword = "123456pass";

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ── Регистрация ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidData_ReturnsSuccess()
    {
        var request = new { callsign = "driver_1", password = SharedPassword };
        var response = await _client.PostAsJsonAsync("/api/taxi/register", request);

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Register_Duplicate_ReturnsFail()
    {
        var request = new { callsign = "driver_2", password = SharedPassword };

        await _client.PostAsJsonAsync("/api/taxi/register", request); // Первая регистрация
        var response = await _client.PostAsJsonAsync("/api/taxi/register", request); // Повтор

        Assert.False(response.IsSuccessStatusCode);
    }

    // ── Авторизация ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        var driver = "driver_3";
        await _client.PostAsJsonAsync("/api/taxi/register", new { callsign = driver, password = SharedPassword });

        var loginRequest = new { callsign = driver, password = SharedPassword };
        var response = await _client.PostAsJsonAsync("/api/taxi/login", loginRequest);

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsFail()
    {
        var driver = "driver_4";
        await _client.PostAsJsonAsync("/api/taxi/register", new { callsign = driver, password = SharedPassword });

        var loginRequest = new { callsign = driver, password = "wrong_password" };
        var response = await _client.PostAsJsonAsync("/api/taxi/login", loginRequest);

        Assert.False(response.IsSuccessStatusCode);
    }
}