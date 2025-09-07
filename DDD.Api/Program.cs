using System.Net;
using DDD.Api.Startup;
using DDD.Infrastructure;
using DDD.Application;
using DDD.Application.Exceptions;
using DDD.Application.Messaging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
Serilog.Debugging.SelfLog.Enable(Console.Error);
// Add services to the container.

IConfiguration configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Keycloak Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakConfig = builder.Configuration.GetSection("Keycloak");
        options.Authority = keycloakConfig["Authority"];
        options.RequireHttpsMetadata = false;
        options.Audience = keycloakConfig["Audience"];
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = keycloakConfig.GetValue<bool>("ValidateIssuer"),
            ValidateAudience = keycloakConfig.GetValue<bool>("ValidateAudience"),
            ValidateLifetime = keycloakConfig.GetValue<bool>("ValidateLifetime"),
            ValidateIssuerSigningKey = true,
            ValidIssuer = keycloakConfig["Authority"],
            ValidAudience = keycloakConfig["Audience"], 
            ClockSkew = TimeSpan.Parse(keycloakConfig["ClockSkew"] ?? "00:05:00"),
        };
        
        options.MetadataAddress = keycloakConfig.GetValue<string>("MetadataAddress") ?? throw new Exception("MetadataAddress is required");
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx => { 
                Console.WriteLine("AuthFailed: " + ctx.Exception.ToString());
                return Task.CompletedTask;
            },
            OnChallenge = ctx => {
                Console.WriteLine($"Challenge: {ctx.Error} - {ctx.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DDD API", Version = "v1" });
    
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

// Custom configuration
builder.Services.AddInfrastructure(configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddCacheConfiguration(configuration);

// Register domain event handlers
builder.Services.AddScoped<DomainEventHandler>();

var app = builder.Build();

// Apply database migrations on startup
await app.Services.ApplyDatabaseMigrationsAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// TODO move to external startup class
app.UseExceptionHandler(c => c.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
    var response = new CustomResponse
    {
        Message = exception.Message
    };

    if (exception.GetType() == typeof(NotExistsException))
    {
        var realException = (NotExistsException)exception;
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        response.Status = Statuses.NotFound;
        response.Ids = realException.Ids;
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }

    response.Code = context.Response.StatusCode;

    await context.Response.WriteAsJsonAsync(response);
}));

app.Run();