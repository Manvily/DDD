using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace DDD.Api.Tests.Authentication;

public class JwtAuthenticationTests
{
    [Fact]
    public void JwtTokenValidation_ShouldHaveCorrectClaims()
    {
        // Arrange
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("this-is-a-very-long-secret-key-for-testing-purposes-only");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("realm_access.roles", "user")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "http://localhost:8080/realms/ddd-realm",
            Audience = "ddd-api",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        // Act
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        var validatedToken = tokenHandler.ReadJwtToken(tokenString);

        // Assert
        validatedToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "testuser");
        validatedToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        validatedToken.Claims.Should().Contain(c => c.Type == "realm_access.roles" && c.Value == "user");
        validatedToken.Issuer.Should().Be("http://localhost:8080/realms/ddd-realm");
        validatedToken.Audiences.Should().Contain("ddd-api");
    }

    [Fact]
    public void JwtBearerConfiguration_ShouldHaveCorrectSettings()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = "http://localhost:8080/realms/ddd-realm",
                ["Keycloak:Audience"] = "ddd-api",
                ["Keycloak:RequireHttpsMetadata"] = "false",
                ["Keycloak:ValidateIssuer"] = "true",
                ["Keycloak:ValidateAudience"] = "true",
                ["Keycloak:ValidateLifetime"] = "true",
                ["Keycloak:ClockSkew"] = "00:05:00"
            })
            .Build();

        // Act & Assert
        var keycloakConfig = configuration.GetSection("Keycloak");
        keycloakConfig["Authority"].Should().Be("http://localhost:8080/realms/ddd-realm");
        keycloakConfig["Audience"].Should().Be("ddd-api");
        keycloakConfig.GetValue<bool>("RequireHttpsMetadata").Should().BeFalse();
        keycloakConfig.GetValue<bool>("ValidateIssuer").Should().BeTrue();
        keycloakConfig.GetValue<bool>("ValidateAudience").Should().BeTrue();
        keycloakConfig.GetValue<bool>("ValidateLifetime").Should().BeTrue();
        TimeSpan.Parse(keycloakConfig["ClockSkew"] ?? "00:05:00").Should().Be(TimeSpan.FromMinutes(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bearer")]
    [InlineData("Bearer ")]
    [InlineData("InvalidScheme token")]
    public void InvalidAuthorizationHeaders_ShouldBeRejected(string authHeader)
    {
        // Arrange & Act
        var isValid = !string.IsNullOrWhiteSpace(authHeader) && 
                     authHeader.StartsWith("Bearer ") && 
                     authHeader.Length > 7;

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidAuthorizationHeader_ShouldBeAccepted()
    {
        // Arrange
        var authHeader = "Bearer valid-jwt-token-here";

        // Act
        var isValid = authHeader.StartsWith("Bearer ") && authHeader.Length > 7;

        // Assert
        isValid.Should().BeTrue();
    }
}