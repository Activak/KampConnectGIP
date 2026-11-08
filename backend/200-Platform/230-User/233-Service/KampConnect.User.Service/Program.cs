using System.Threading.RateLimiting;
using KampConnect.Shared.Contracts.Authorization;
using KampConnect.Shared.ServiceDefaults.Authentication;
using KampConnect.Shared.ServiceDefaults.Cors;
using KampConnect.Shared.ServiceDefaults.Messaging;
using KampConnect.User.Data.Context.Implentation;
using KampConnect.User.Service.Controllers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<KampConnectUserDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("kampconnect_user"));
});

// Add controllers
builder.Services.AddControllers();

// Add Data
builder.Services.AddUserData();

// Add Business
builder.Services.AddUserBusiness();

// Add address validation
builder.Services.AddGoogleAddressValidation(builder.Configuration);

// Add authentication
builder.AddKampConnectJwtAuthentication();

builder.Services.AddKampConnectAuthorization();

builder.AddKampConnectMessaging(typeof(Program).Assembly);

builder.Services.AddKampConnectCors(builder.Configuration);

// Brute force op korte invite-codes beperken
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(FamilyController.JoinRateLimitPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.FindFirst(KampConnectClaimTypes.Subject)?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(CorsExtension.CorsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
