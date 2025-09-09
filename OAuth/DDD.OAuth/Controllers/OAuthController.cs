using Microsoft.AspNetCore.Mvc;

namespace DDD.OAuth.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OAuthController(IConfiguration configuration, ILogger<OAuthController> logger)
    : ControllerBase
{
    /// <summary>
    /// Get Keycloak authorization URL for frontend redirect
    /// </summary>
    /// <param name="redirectUri">Frontend callback URL</param>
    /// <param name="state">Security state parameter</param>
    /// <returns>Authorization URL</returns>
    [HttpGet("authorize-url")]
    public ActionResult<object> GetAuthorizeUrl([FromQuery] string redirectUri, [FromQuery] string? state = null)
    {
        try
        {
            var keycloakConfig = configuration.GetSection("Keycloak");
            var authority = keycloakConfig["Authority"];
            var clientId = keycloakConfig["ClientId"];

            if (string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(clientId))
            {
                return BadRequest(new { message = "Keycloak configuration is missing" });
            }

            // Generate state if not provided
            state ??= Guid.NewGuid().ToString();

            var authUrl = $"{authority}/protocol/openid-connect/auth" +
                         $"?client_id={clientId}" +
                         $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                         $"&response_type=code" +
                         $"&scope=openid profile email" +
                         $"&state={state}";

            return Ok(new
            {
                authorizeUrl = authUrl,
                state = state,
                clientId = clientId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating authorize URL");
            return BadRequest(new { message = "Failed to generate authorize URL" });
        }
    }

    /// <summary>
    /// Exchange authorization code for tokens
    /// </summary>
    /// <param name="code">Authorization code from Keycloak</param>
    /// <param name="redirectUri">Original redirect URI</param>
    /// <param name="state">Security state parameter</param>
    /// <returns>Tokens and user info</returns>
    [HttpPost("callback")]
    public async Task<ActionResult> HandleCallback(
        [FromForm] string code,
        [FromForm] string redirectUri,
        [FromForm] string? state = null)
    {
        try
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = "Authorization code is required" });
            }

            var keycloakConfig = configuration.GetSection("Keycloak");
            var authority = keycloakConfig["Authority"];
            var clientId = keycloakConfig["ClientId"];
            var clientSecret = keycloakConfig["ClientSecret"];

            var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

            var formParams = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "authorization_code"),
                new("client_id", clientId ?? ""),
                new("code", code),
                new("redirect_uri", redirectUri)
            };

            if (!string.IsNullOrEmpty(clientSecret))
            {
                formParams.Add(new("client_secret", clientSecret));
            }

            using var httpClient = new HttpClient();
            var formContent = new FormUrlEncodedContent(formParams);
            
            var response = await httpClient.PostAsync(tokenEndpoint, formContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Token exchange failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return BadRequest(new { message = "Failed to exchange code for tokens" });
            }

            // Return tokens to frontend
            return Ok(responseContent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling OAuth callback");
            return BadRequest(new { message = "OAuth callback failed" });
        }
    }
}
