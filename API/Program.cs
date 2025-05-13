using API.APIEndpoints;
using API.Middleware;
using BusinessLogicLayer;
using DataAccessLayer;
using DataAccessLayer.Context;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System;

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

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
// Endpoint routing
app.Run();

public partial class Program { }