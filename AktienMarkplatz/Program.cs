using CoreIdent.Extensions;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreIdent(o => {
    o.Issuer = "";
    o.Audience = ""; 
});

builder.Services.AddSigningKey(o => o.UseRsa(""))
// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapCoreIdentEndpoints();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
