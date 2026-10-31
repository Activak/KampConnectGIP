using KampConnect.Shared.ServiceDefaults.Authentication;
using KampConnect.Shared.ServiceDefaults.Cors;
using KampConnect.Shared.ServiceDefaults.Messaging;
using KampConnect.User.Data.Context.Implentation;
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

// Add authentication
builder.AddKampConnectJwtAuthentication();

builder.Services.AddKampConnectAuthorization();

builder.AddKampConnectMessaging(typeof(Program).Assembly);

builder.Services.AddKampConnectCors(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(CorsExtension.CorsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
