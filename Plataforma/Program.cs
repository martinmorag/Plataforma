using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Plataforma.Data;
using Plataforma.Models;
using Xabe.FFmpeg.Downloader;
using Xabe.FFmpeg;

DotNetEnv.Env.Load();

var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ffmpeg");

// Solo descarga si no existen ya los ejecutables
if (!Directory.Exists(ffmpegPath) || !File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")))
{
    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
}

FFmpeg.SetExecutablesPath(ffmpegPath);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddSingleton(System.TimeProvider.System);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PlataformaContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnectionString") // Use PostgreSQL connection string
    )
);


// AUTHENTICATION
builder.Services.AddIdentity<UsuarioIdentidad, IdentityRole<Guid>>(options => // Use your custom user class and Role
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Or true, if you implement email confirmation
})
.AddEntityFrameworkStores<PlataformaContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/ingreso"; // Your custom login path
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Administrador")); // Requires the "Admin" role
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
app.UseStaticFiles();

app.UseRouting();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>(); // Or IdentityRole<string>
    if (!await roleManager.RoleExistsAsync("Administrador"))
    {
        await roleManager.CreateAsync(new IdentityRole<Guid>("Administrador")); // Or IdentityRole<string>
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
