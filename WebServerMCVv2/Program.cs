using Microsoft.EntityFrameworkCore;
using WebServerMCVv2;
using WebServerMVCv2.Data;
using WebServerMVCv2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")));

builder.Services.AddAuthentication()
    .AddCookie(Settings.AuthCookieName,
    options => 
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Forbidden";
        options.Cookie.Name = Settings.AuthCookieName;
    });

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthorization(
    options => 
    {
        //a policy is a collection of claims
        options.AddPolicy("Admin", policy => policy.RequireClaim("admin", "true"));
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//login/logout
app.UseAuthentication();

//what you can do 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}"
    );



app.Run();
