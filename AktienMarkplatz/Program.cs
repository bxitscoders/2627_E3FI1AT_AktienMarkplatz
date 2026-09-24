using AktienMarkplatz.Classes;
using AktienMarkplatz.Data;

using AktienMarkplatz.Models;

using AktienMarkplatz.Services;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Identity.UI.Services;

using Microsoft.EntityFrameworkCore;
using IdentitaetsSeeder = AktienMarkplatz.Classes.IdentitaetsSeeder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>

    options.UseSqlite(builder.Configuration.GetConnectionString("AktienMarkplatzDB")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>

{

    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(45);
    options.User.RequireUniqueEmail = true;

})
    .AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{ 
    await IdentitaetsSeeder.SeedRolesAsync(scope.ServiceProvider);

}
// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Error");

    app.UseHsts();

}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()

   .WithStaticAssets();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Identity/Account/Login?returnUrl=/Aktienmarkt");
    return Task.CompletedTask;
});

app.Run();
