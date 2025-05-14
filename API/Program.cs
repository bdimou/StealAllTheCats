using API.APIEndpoints;
using API.Middleware;
using BusinessLogicLayer;
using DataAccessLayer;
using DataAccessLayer.Context;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Layer DI
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

// InMemory Cache
builder.Services.AddMemoryCache();

// FluentValidations
builder.Services.AddFluentValidationAutoValidation();

// Endpoint routing
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StealAllTheCats API",
        Version = "v1",
        Description = "API for managing cat images and tags"
    });

    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseRouting();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapCatsAPIEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    int retries = 0;
    while (retries < 5)
    {
        try
        {
            context.Database.Migrate(); // or EnsureCreated()
            break;
        }
        catch (Exception ex)
        {
            retries++;
            Console.WriteLine($"Database not ready yet, retry {retries}... {ex.Message}");
            Thread.Sleep(60);
        }
    }
}

app.Run();

public partial class Program { }