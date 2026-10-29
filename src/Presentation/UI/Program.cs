using OpenAPI;
using UI.Filters;
using UI.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    // Container Apps ingress is the only public entry point and its proxy addresses are dynamic.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

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
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
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
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Phase 1 is USD-only; pin the culture so currency formatting does not depend on the host locale.
const string displayCulture = "en-US";
app.UseRequestLocalization(options => options
    .SetDefaultCulture(displayCulture)
    .AddSupportedCultures(displayCulture)
    .AddSupportedUICultures(displayCulture));

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapGet("/health/live", () => Results.Ok()).AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
