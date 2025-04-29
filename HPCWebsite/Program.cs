using DataLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Service;
using ViewModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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
