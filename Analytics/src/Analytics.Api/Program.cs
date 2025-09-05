using Analytics.Application;
using Analytics.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Debugging;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
Serilog.Debugging.SelfLog.Enable(Console.Error);
// Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Keycloak Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakConfig = builder.Configuration.GetSection("Keycloak");
        options.Authority = keycloakConfig["Authority"];
        options.Audience = keycloakConfig["Audience"];
        options.RequireHttpsMetadata = keycloakConfig.GetValue<bool>("RequireHttpsMetadata");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = keycloakConfig.GetValue<bool>("ValidateIssuer"),
            ValidateAudience = keycloakConfig.GetValue<bool>("ValidateAudience"),
            ValidateLifetime = keycloakConfig.GetValue<bool>("ValidateLifetime"),
            ClockSkew = TimeSpan.Parse(keycloakConfig["ClockSkew"] ?? "00:05:00"),
            RoleClaimType = "realm_access.roles"
        };
    });

builder.Services.AddAuthorization();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Analytics API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Application Layer
builder.Services.AddApplication();

// Add Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
