using API.APIEndpoints;
using API.Middleware;
using BusinessLogicLayer;
using DataAccessLayer;
using FluentValidation.AspNetCore;

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

// Endpoint routing
app.Run();