using FoodBridge.Api;
using FoodBridge.Infrastructure;
using FoodBridge.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiLayer(builder.Configuration);

var app = builder.Build();
await DevelopmentSeeder.SeedAsync(app.Services, app.Environment.IsDevelopment());
app.UseApiPipeline();
app.Run();
