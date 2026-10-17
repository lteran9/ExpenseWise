using ExpenseWise.DependencyConfiguration;
using OpenAPI;
using UI.Filters;

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
});

builder.Services
    .ConfigureDependencies(builder.Configuration)
    .RegisterUseCases()
    .AddHttpContextAccessor()
   .AddScoped<UserInfoActionFilter>()
   .ConfigureSession();

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

app.UseAuthorization();

app.MapStaticAssets();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
