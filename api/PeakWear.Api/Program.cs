using Microsoft.EntityFrameworkCore;
using PeakWear.Api.Extensions;
using PeakWear.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using PeakWear.Core.Services;
using PeakWear.Data.Clients;

// Starts the app and loads config from appsettings.json, user-secrets and env vars
var builder = WebApplication.CreateBuilder(args);

// ============================================================
//  SERVICES — registering what the app can use.
//  Order here doesn't matter; it's just filling a container.
// ============================================================

// Finds our controller classes and turns on attribute routing
builder.Services.AddControllers();

// Generates the API description document at /openapi/v1.json
builder.Services.AddOpenApi();

// Our own extension method — Scrutor scans and registers every Repository and Service
builder.Services.AddAppDependencies();

// Dapper only: tells it that a display_name column maps to a DisplayName property
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Registers the DbContext (Scoped — one per HTTP request).
// UseNpgsql picks the Postgres driver; the naming convention makes EF write snake_case columns.
builder.Services.AddDbContext<PeakWearDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention());

// Lets the Angular dev server (port 4200) call this API (port 5248).
// Browsers block cross-port requests by default.
builder.Services.AddCors(o => o.AddPolicy("dev", p => p
    .WithOrigins("http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Teaches the app how to read and verify a JWT from the Authorization header
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // The rules a token must pass to be accepted
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,             // was it issued by us?
            ValidateAudience = true,           // was it meant for our client?
            ValidateLifetime = true,           // has it expired?
            ValidateIssuerSigningKey = true,   // the important one — proves nobody forged it
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Turns on the [Authorize] attribute — the permission layer
builder.Services.AddAuthorization();

builder.Services.AddHttpClient<ISizeRecommendationClient, GroqClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});

// Everything is registered; build the app
var app = builder.Build();

// ============================================================
//  PIPELINE — every request travels through these in order,
//  top to bottom. Order matters a lot here.
// ============================================================

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PeakWearDbContext>();
        await ProductSeeder.SeedAsync(db);
    }

// Only expose API docs while developing, never in production
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();   // browse the API at /scalar/v1
}

// Sends http:// requests over to https://
app.UseHttpsRedirection();

// Must come BEFORE authentication: the browser's preflight OPTIONS request
// carries no token, so auth would reject it and the real request would never happen.
app.UseCors("dev");

// Reads and verifies the token, then fills in User.Claims — "who are you?"
app.UseAuthentication();

// Checks those claims against [Authorize] attributes — "are you allowed?"
app.UseAuthorization();

// Hands the request to the matching controller action
app.MapControllers();

// Start listening for requests
app.Run();