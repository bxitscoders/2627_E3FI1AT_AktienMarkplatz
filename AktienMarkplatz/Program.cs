using AktienMarkplatz.Data;

using AktienMarkplatz.Models;

using AktienMarkplatz.Services;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Identity.UI.Services;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>

    options.UseSqlite(builder.Configuration.GetConnectionString("AktienMarkplatzDB")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>

{

    options.Password.RequiredLength = 8;

    options.Password.RequireNonAlphanumeric = true;

    options.Password.RequireUppercase = true;

    options.Lockout.MaxFailedAccessAttempts = 5;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(45);

    options.User.RequireUniqueEmail = true;

})

.AddEntityFrameworkStores<ApplicationDbContext>()

.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

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

app.Run();
