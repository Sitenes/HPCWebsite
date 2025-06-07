using AutomationEngine.CustomMiddlewares.Configuration;
using AutomationEngine.CustomMiddlewares.Extensions;
using DataLayer;
using DataLayer.DbContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Service;
using System.Reflection.PortableExecutable;
using Tools.AuthoraizationTools;
using ViewModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
//Add-Migration InitialCreate -Context Context
//Update-Database InitialCreate -Context Context
builder.Services.AddDbContext<Context>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("BasicAutomationEngine")));

//Add-Migration InitialCreate -DbContext DynamicDbContext
//Update-Database InitialCreate -DbContext DynamicDbContext
builder.Services.AddDbContext<DynamicDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("DynamicAutomationEngine")));


builder.Host.AddApplicationLogging(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddTransient<ICacheService, CacheService>();
// روش توصیه شده: استفاده از IOptions<T>
builder.Services.Configure<SmsConfiguration>(
    builder.Configuration.GetSection("SmsConfig")
);
// ثبت HttpClient برای SmsService
builder.Services.AddHttpClient<ISmsService, SmsService>();
// ثبت سرویس‌ها با عمر مناسب
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IBillingService, BillingService>();
builder.Services.AddTransient<IPaymentService, PaymentService>();
builder.Services.AddTransient<IPasargadService, PasargadService>();
builder.Services.AddTransient<IZarinpalService, ZarinpalService>();
builder.Services.AddTransient<IServerRentalService, ServerRentalService>();
builder.Services.AddTransient<IServerService, ServerService>();
builder.Services.AddTransient<IShoppingCartService, ShoppingCartService>();
builder.Services.AddTransient<IShoppingCartService, ShoppingCartService>();
builder.Services.AddTransient<TokenGenerator>();
builder.Services.AddTransient<OpenStackService>();
builder.Services.AddHttpClient();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        //options.AccessDeniedPath = "/User/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // مدت زمان اعتبار کوکی
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseCors(builder =>
{
    // تنظیمات CORS در محیط Development
    builder
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();

});

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
