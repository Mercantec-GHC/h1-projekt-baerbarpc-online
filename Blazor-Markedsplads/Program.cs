using Blazor_Markedsplads.Components;  
using Blazor_Markedsplads.Services;
using BlazorMarkedsplads.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── 2. Registrér database‐service + repositories ──────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();

var app = builder.Build();

// ─── 4. Konfigurer middleware‐pipeline i præcis denne rækkefølge ──────────────────────────────
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
app.MapBlazorHub();

app.Run();
