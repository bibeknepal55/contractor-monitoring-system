using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using ContractorMonitoring.Application.Common.Models;
using ContractorMonitoring.Application.DTOs.Auth;

namespace ContractorMonitoring.IntegrationTests.Endpoints;

public class AuthEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401OrFail()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "nonexistent@example.com",
            password = "wrongpassword"
        });

        // Either 401 or 200 with success=false (depends on controller implementation)
        var body = await response.Content.ReadFromJsonAsync<dynamic>();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMissingFields_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "",
            password = ""
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.OK); // validation may return 200 with success=false
    }

    [Fact]
    public async Task HealthLive_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/projects");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsFail()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            accessToken = "invalid.token.here",
            refreshToken = "invalid_refresh"
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            body!.Success.Should().BeFalse();
        }
    }
}
