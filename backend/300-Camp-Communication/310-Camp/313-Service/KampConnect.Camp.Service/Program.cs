using KampConnect.Camp.Data.Context.Implementation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<KampConnectCampDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("kampconnect_camp"));
});

builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
