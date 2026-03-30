using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using ServiceDesk.Application;
using ServiceDesk.Infrastructure;
using ServiceDesk.Infrastructure.Data.Seeds;
using ServiceDesk.Web.Auth;
using ServiceDesk.Web.Filters;
using ServiceDesk.Web.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console());

// Регистрация модулей
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllersWithViews(options =>
{
    // Принудительный разлогин деактивированных пользователей
    options.Filters.Add<DeactivatedUserFilter>();
    // Глобальная проверка согласия на обработку ПД (ФЗ-152)
    options.Filters.Add<ConsentRequiredFilter>();
});

// Хранилище сессий в БД — кука содержит только ID сессии
builder.Services.AddSingleton<ITicketStore, DatabaseTicketStore>();
builder.Services.AddHttpContextAccessor();

// Аутентификация через Cookies (сессии хранятся в БД)
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(365);
        options.SlidingExpiration = true;
    });

// Подключаем хранилище сессий через IPostConfigureOptions (без BuildServiceProvider)
builder.Services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>,
    ConfigureCookieTicketStore>();

var app = builder.Build();

// Начальное заполнение БД
await DbSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Обработка ошибок
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Маршрут по умолчанию: /Home/Dashboard
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.Run();
