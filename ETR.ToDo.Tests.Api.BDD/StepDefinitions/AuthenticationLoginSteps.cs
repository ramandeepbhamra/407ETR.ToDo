using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using ETR.ToDo.Services.Core.Authentication.Dto;
using ETR.ToDo.Tests.Api.BDD.Infrastructure;
using ETR.ToDo.Web.Core.Models;
using FluentAssertions;
using Reqnroll;

namespace ETR.ToDo.Tests.Api.BDD.StepDefinitions;

[Binding]
public class AuthenticationLoginSteps : StepDefinitionBase
{
    public AuthenticationLoginSteps(ScenarioContextHelper context, CustomWebApplicationFactory factory)
        : base(context, factory)
    {
    }

    [Given(@"the API is running")]
    public void GivenTheApiIsRunning()
    {
        // The API is initialized by the CustomWebApplicationFactory
        HttpClient.Should().NotBeNull();
    }

    [Given(@"the following users exist in the system:")]
    public void GivenTheFollowingUsersExistInTheSystem(Table table)
    {
        // Users are already seeded in CustomWebApplicationFactory.SeedTestData()
        // This step is declarative for BDD readability
        table.RowCount.Should().BeGreaterThan(0);
    }

    [Given(@"I am not authenticated")]
    public void GivenIAmNotAuthenticated()
    {
        Context.ClearAuthToken();
    }

    [When(@"I attempt to login with the following credentials:")]
    public async Task WhenIAttemptToLoginWithTheFollowingCredentials(Table table)
    {
        var row = table.Rows[0];
        var loginRequest = new LoginRequestDto
        {
            Email = row["Email"],
            Password = row["Password"]
        };

        Context.Set("LoginRequest", loginRequest);

        var response = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        Context.LastResponse = response;
        Context.LastResponseContent = await response.Content.ReadAsStringAsync();
    }

    [Then(@"the response status code should be (.*)")]
    public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
    {
        Context.LastResponse.Should().NotBeNull();
        ((int)Context.LastResponse!.StatusCode).Should().Be(expectedStatusCode);
    }

    [Then(@"the response should contain an access token")]
    public void ThenTheResponseShouldContainAnAccessToken()
    {
        Context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var response = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(
            Context.LastResponseContent!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        response.Should().NotBeNull();
        response!.Data.Should().NotBeNull();
        response.Data!.Token.Should().NotBeNull();
        response.Data.Token.AccessToken.Should().NotBeNullOrEmpty();

        Context.Set("AccessToken", response.Data.Token.AccessToken);
    }

    [Then(@"the response should contain a refresh token")]
    public void ThenTheResponseShouldContainARefreshToken()
    {
        var response = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(
            Context.LastResponseContent!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        response.Should().NotBeNull();
        response!.Data.Should().NotBeNull();
        response.Data!.Token.Should().NotBeNull();
        response.Data.Token.RefreshToken.Should().NotBeNullOrEmpty();

        Context.Set("RefreshToken", response.Data.Token.RefreshToken);
    }

    [Then(@"the access token should be valid")]
    public void ThenTheAccessTokenShouldBeValid()
    {
        var accessToken = Context.Get<string>("AccessToken");
        accessToken.Should().NotBeNullOrEmpty();

        // Decode JWT without validation (testing purposes)
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;

        jsonToken.Should().NotBeNull();
        jsonToken!.Claims.Should().NotBeEmpty();

        // Verify expiration is in the future
        var exp = jsonToken.ValidTo;
        exp.Should().BeAfter(DateTime.UtcNow);
    }

    [Then(@"the token should contain the user email ""(.*)""")]
    public void ThenTheTokenShouldContainTheUserEmail(string expectedEmail)
    {
        var accessToken = Context.Get<string>("AccessToken");
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;

        var emailClaim = jsonToken!.Claims.FirstOrDefault(c => 
            c.Type == ClaimTypes.Email || c.Type == "email");

        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(expectedEmail);
    }

    [Then(@"the token should contain role ""(.*)""")]
    public void ThenTheTokenShouldContainRole(string expectedRole)
    {
        var accessToken = Context.Get<string>("AccessToken");
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;

        var roleClaim = jsonToken!.Claims.FirstOrDefault(c => 
            c.Type == ClaimTypes.Role || c.Type == "role");

        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be(expectedRole);
    }

    [Then(@"the response should contain error message ""(.*)""")]
    public void ThenTheResponseShouldContainErrorMessage(string expectedError)
    {
        Context.LastResponseContent.Should().NotBeNullOrEmpty();
        Context.LastResponseContent!.Should().Contain(expectedError);
    }

    [Then(@"the response should contain validation errors")]
    public void ThenTheResponseShouldContainValidationErrors()
    {
        Context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Context.LastResponseContent.Should().NotBeNullOrEmpty();
        
        // Check if response contains validation error structure
        Context.LastResponseContent!.Should().MatchRegex("(error|validation|required|invalid)", 
            "Response should contain validation error indicators");
    }
}
