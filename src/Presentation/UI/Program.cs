using OpenAPI;
using UI.Filters;
using UI.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
   .AddControllersWithViews()
   .AddRazorRuntimeCompilation();

var expenseWiseApiBaseUrl = builder.Configuration["ExpenseWiseApi:BaseUrl"]
     ?? throw new InvalidOperationException("ExpenseWiseApi:BaseUrl must be configured.");
builder.Services.AddHttpClient<IExpenseWiseClient, ExpenseWiseClient>(client =>
{
     client.BaseAddress = new Uri(expenseWiseApiBaseUrl);
}).AddHttpMessageHandler<ApiTokenHandler>();

builder.Services
    .AddHttpContextAccessor()
    .AddTransient<ApiTokenHandler>()
    .AddScoped<UserInfoActionFilter>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/SignIn";
        options.AccessDeniedPath = "/Home/Index";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = false;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
