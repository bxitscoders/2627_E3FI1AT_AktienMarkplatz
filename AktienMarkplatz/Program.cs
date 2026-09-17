using CoreIdent.Core.Extensions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Der: SigningKeySecret wurde in user secrets gesichert
builder.Services.AddCoreIdent(o => {
    o.Issuer = "https://localhost:7280";
    o.Audience = "https://localhost:7280/resources/my-api"; 
    o.SigningKeySecret = builder.Configuration["CoreIdent:SigningKeySecret"];
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
