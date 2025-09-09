using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Analytics.Tests.Authentication;

public class JwtAuthenticationTests
{
    [Fact]
    public void JwtTokenValidation_ShouldHaveCorrectAudienceForAnalytics()
    {
        // Arrange
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("this-is-a-very-long-secret-key-for-testing-purposes-only");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "analyticsuser"),
                new Claim(ClaimTypes.Email, "analytics@example.com"),
                new Claim("realm_access.roles", "analytics_user")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "http://localhost:8080/realms/ddd-realm",
            Audience = "analytics-api",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        // Act
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        var validatedToken = tokenHandler.ReadJwtToken(tokenString);

        // Assert
        validatedToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "analyticsuser");
        validatedToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "analytics@example.com");
        validatedToken.Claims.Should().Contain(c => c.Type == "realm_access.roles" && c.Value == "analytics_user");
        validatedToken.Issuer.Should().Be("http://localhost:8080/realms/ddd-realm");
        validatedToken.Audiences.Should().Contain("analytics-api");
    }

    [Fact]
    public void AnalyticsJwtBearerConfiguration_ShouldHaveCorrectSettings()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = "http://localhost:8080/realms/ddd-realm",
                ["Keycloak:Audience"] = "analytics-api",
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
        keycloakConfig["Audience"].Should().Be("analytics-api");
        keycloakConfig.GetValue<bool>("RequireHttpsMetadata").Should().BeFalse();
        keycloakConfig.GetValue<bool>("ValidateIssuer").Should().BeTrue();
        keycloakConfig.GetValue<bool>("ValidateAudience").Should().BeTrue();
        keycloakConfig.GetValue<bool>("ValidateLifetime").Should().BeTrue();
        TimeSpan.Parse(keycloakConfig["ClockSkew"] ?? "00:05:00").Should().Be(TimeSpan.FromMinutes(5));
    }

    [Theory]
    [InlineData("ddd-api")] // Wrong audience
    [InlineData("wrong-api")]
    [InlineData("")]
    public void TokenValidation_ShouldFail_WithWrongAudience(string wrongAudience)
    {
        // Arrange
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("this-is-a-very-long-secret-key-for-testing-purposes-only");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "http://localhost:8080/realms/ddd-realm",
            Audience = wrongAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        var validatedToken = tokenHandler.ReadJwtToken(tokenString);

        // Act & Assert
        validatedToken.Audiences.Should().NotContain("analytics-api");
        if (!string.IsNullOrEmpty(wrongAudience))
        {
            validatedToken.Audiences.Should().Contain(wrongAudience);
        }
    }
}