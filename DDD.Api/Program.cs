using System.Net;
using DDD.Api.Startup;
using DDD.Infrastructure;
using DDD.Application;
using DDD.Application.Exceptions;
using DDD.Application.Messaging;
using Microsoft.AspNetCore.Diagnostics;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Startup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

IConfiguration configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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