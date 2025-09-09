using AutoFixture;
using Bogus;
using DDD.OAuth.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DDD.OAuth.Tests.Controllers;

public class OAuthControllerTests
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OAuthController> _logger;
    private readonly OAuthController _controller;
    private readonly IFixture _fixture;
    private readonly Faker _faker;

    public OAuthControllerTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<OAuthController>>();
        _controller = new OAuthController(_configuration, _logger);
        _fixture = new Fixture();
        _faker = new Faker();
    }

    [Fact]
    public void GetAuthorizeUrl_Should_ReturnOkResult_When_ConfigurationIsValid()
    {
        // Arrange
        var redirectUri = "https://example.com/callback";
        var state = "test-state";
        var authority = "https://keycloak.example.com/auth/realms/test";
        var clientId = "test-client-id";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns(authority);
        keycloakSection["ClientId"].Returns(clientId);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri, state);

        // Assert
        result.Should().BeOfType<ActionResult<object>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var value = okResult.Value;
        value.Should().NotBeNull();

        // Use reflection to check the anonymous object properties
        var valueType = value!.GetType();
        var authorizeUrlProperty = valueType.GetProperty("authorizeUrl");
        var stateProperty = valueType.GetProperty("state");
        var clientIdProperty = valueType.GetProperty("clientId");

        authorizeUrlProperty.Should().NotBeNull();
        stateProperty.Should().NotBeNull();
        clientIdProperty.Should().NotBeNull();

        var authorizeUrl = authorizeUrlProperty!.GetValue(value)?.ToString();
        var returnedState = stateProperty!.GetValue(value)?.ToString();
        var returnedClientId = clientIdProperty!.GetValue(value)?.ToString();

        authorizeUrl.Should().Contain(authority);
        authorizeUrl.Should().Contain(clientId);
        authorizeUrl.Should().Contain("redirect_uri=" + Uri.EscapeDataString(redirectUri));
        returnedState.Should().Be(state);
        returnedClientId.Should().Be(clientId);
    }

    [Fact]
    public void GetAuthorizeUrl_Should_GenerateState_When_StateIsNotProvided()
    {
        // Arrange
        var redirectUri = "https://example.com/callback";
        var authority = "https://keycloak.example.com/auth/realms/test";
        var clientId = "test-client-id";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns(authority);
        keycloakSection["ClientId"].Returns(clientId);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        result.Should().BeOfType<ActionResult<object>>();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;

        var value = okResult.Value;
        var valueType = value!.GetType();
        var stateProperty = valueType.GetProperty("state");
        var returnedState = stateProperty!.GetValue(value)?.ToString();

        returnedState.Should().NotBeNullOrEmpty();
        Guid.TryParse(returnedState, out _).Should().BeTrue();
    }

    [Fact]
    public void GetAuthorizeUrl_Should_ReturnBadRequest_When_AuthorityIsMissing()
    {
        // Arrange
        var redirectUri = "https://example.com/callback";
        var clientId = "test-client-id";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns((string?)null);
        keycloakSection["ClientId"].Returns(clientId);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        result.Should().BeOfType<ActionResult<object>>();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var value = badRequestResult.Value;
        var valueType = value!.GetType();
        var messageProperty = valueType.GetProperty("message");
        var message = messageProperty!.GetValue(value)?.ToString();

        message.Should().Be("Keycloak configuration is missing");
    }

    [Fact]
    public void GetAuthorizeUrl_Should_ReturnBadRequest_When_ClientIdIsMissing()
    {
        // Arrange
        var redirectUri = "https://example.com/callback";
        var authority = "https://keycloak.example.com/auth/realms/test";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns(authority);
        keycloakSection["ClientId"].Returns((string?)null);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        result.Should().BeOfType<ActionResult<object>>();
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public void GetAuthorizeUrl_Should_IncludeCorrectScopes_When_GeneratingUrl()
    {
        // Arrange
        var redirectUri = "https://example.com/callback";
        var authority = "https://keycloak.example.com/auth/realms/test";
        var clientId = "test-client-id";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns(authority);
        keycloakSection["ClientId"].Returns(clientId);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value;
        var valueType = value!.GetType();
        var authorizeUrlProperty = valueType.GetProperty("authorizeUrl");
        var authorizeUrl = authorizeUrlProperty!.GetValue(value)?.ToString();

        authorizeUrl.Should().Contain("scope=openid profile email");
        authorizeUrl.Should().Contain("response_type=code");
    }

    [Fact]
    public void GetAuthorizeUrl_Should_EscapeRedirectUri_When_UriContainsSpecialCharacters()
    {
        // Arrange
        var redirectUri = "https://example.com/callback?param=value&other=test";
        var authority = "https://keycloak.example.com/auth/realms/test";
        var clientId = "test-client-id";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns(authority);
        keycloakSection["ClientId"].Returns(clientId);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value;
        var valueType = value!.GetType();
        var authorizeUrlProperty = valueType.GetProperty("authorizeUrl");
        var authorizeUrl = authorizeUrlProperty!.GetValue(value)?.ToString();

        authorizeUrl.Should().Contain("redirect_uri=" + Uri.EscapeDataString(redirectUri));
    }

    [Fact]
    public async Task HandleCallback_Should_ReturnBadRequest_When_CodeIsEmpty()
    {
        // Arrange
        var code = "";
        var redirectUri = "https://example.com/callback";

        // Act
        var result = await _controller.HandleCallback(code, redirectUri);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var value = badRequestResult.Value;
        var valueType = value!.GetType();
        var messageProperty = valueType.GetProperty("message");
        var message = messageProperty!.GetValue(value)?.ToString();

        message.Should().Be("Authorization code is required");
    }

    [Fact]
    public async Task HandleCallback_Should_ReturnBadRequest_When_CodeIsNull()
    {
        // Arrange
        string code = null!;
        var redirectUri = "https://example.com/callback";

        // Act
        var result = await _controller.HandleCallback(code, redirectUri);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public void GetAuthorizeUrl_Should_HandleConfigurationException_When_ConfigurationThrows()
    {
        // Arrange
        var redirectUri = "https://example.com/callback";

        _configuration.GetSection("Keycloak").Returns(x => throw new InvalidOperationException("Configuration error"));

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        var value = badRequestResult.Value;
        var valueType = value!.GetType();
        var messageProperty = valueType.GetProperty("message");
        var message = messageProperty!.GetValue(value)?.ToString();

        message.Should().Be("Failed to generate authorize URL");
    }

    [Theory]
    [InlineData("")]
    public void GetAuthorizeUrl_Should_ReturnBadRequest_When_AuthorityIsEmpty(string authority)
    {
        // Arrange
        var redirectUri = "https://example.com/callback";
        var clientId = "test-client-id";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns(authority);
        keycloakSection["ClientId"].Returns(clientId);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Theory]
    [InlineData("")]
    public void GetAuthorizeUrl_Should_ReturnBadRequest_When_ClientIdIsEmpty(string clientId)
    {
        // Arrange
        var redirectUri = "https://example.com/callback";
        var authority = "https://keycloak.example.com/auth/realms/test";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns(authority);
        keycloakSection["ClientId"].Returns(clientId);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public void GetAuthorizeUrl_Should_HandleLongRedirectUri_When_UriIsVeryLong()
    {
        // Arrange
        var longPath = string.Join("/", Enumerable.Repeat("segment", 100));
        var redirectUri = $"https://example.com/{longPath}?param=value";
        var authority = "https://keycloak.example.com/auth/realms/test";
        var clientId = "test-client-id";

        var keycloakSection = Substitute.For<IConfigurationSection>();
        keycloakSection["Authority"].Returns(authority);
        keycloakSection["ClientId"].Returns(clientId);
        _configuration.GetSection("Keycloak").Returns(keycloakSection);

        // Act
        var result = _controller.GetAuthorizeUrl(redirectUri);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }
}
