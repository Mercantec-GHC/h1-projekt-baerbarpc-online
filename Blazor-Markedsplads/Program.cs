using Blazor_Markedsplads.Components;
using Blazor_Markedsplads.Services;
using BlazorMarkedsplads.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024 * 10; // Sæt til 10 MB
});

// Tilføj Blazor-komponenter som før.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();




// ─── Registrér dine egne services som før ───────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();

var app = builder.Build();

// ─── Konfigurer middleware‐pipeline som før ──────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();