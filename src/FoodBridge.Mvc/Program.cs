using FoodBridge.Mvc.Api;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenHandler>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
builder.Services.AddHttpClient<IFoodBridgeApiClient, FoodBridgeApiClient>(client =>
{
    var baseUrl = builder.Configuration["FoodBridgeApi:BaseUrl"]
        ?? throw new InvalidOperationException("FoodBridgeApi:BaseUrl is missing.");
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Listings}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
