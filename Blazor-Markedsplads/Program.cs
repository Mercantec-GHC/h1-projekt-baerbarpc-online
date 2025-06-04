using Blazor_Markedsplads.Components;
using Blazor_Markedsplads.Services;
using BlazorMarkedsplads.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ======== 1) Kun Blazor Server (Standalone Razor Components) ========
// Vi _bruger ikke_ Razor Pages, så vi fjerner alt, der hedder AddRazorPages().
// Til gengæld tilføjer vi Razor Components (= Blazor) direkte:
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

//services/repositories:
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<DBService>();

var app = builder.Build();

// ======== 2) Engangs‐oprettelse af database/tabeller (PostgreSQL) ========
using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();

    // 2.1) Opret ’users’‐tabel, hvis den ikke findes:
    var createUsersSql = @"
        CREATE TABLE IF NOT EXISTS users (
            id        SERIAL PRIMARY KEY,
            name      VARCHAR(255),
            email     VARCHAR(255),
            password  VARCHAR(255),
            phone     VARCHAR(255),
            address   VARCHAR(255),
            city      VARCHAR(255),
            zip_code  VARCHAR(255)
        );";
    await using (var cmd1 = new NpgsqlCommand(createUsersSql, conn))
    {
        await cmd1.ExecuteNonQueryAsync();
    }

    // 2.2) Opret én samlet ’listings’‐tabel (produkt + annonce):
    var createListingsSql = @"
        CREATE TABLE IF NOT EXISTS listings (
            id           SERIAL PRIMARY KEY,
            brand        VARCHAR(255) NOT NULL,
            model        VARCHAR(255) NOT NULL,
            gpu          VARCHAR(255),
            cpu          VARCHAR(255) NOT NULL,
            ram          INT          NOT NULL,
            storage      INT          NOT NULL,
            os           VARCHAR(255) NOT NULL,
            price        VARCHAR(255) NOT NULL,
            screen_size  VARCHAR(255) NOT NULL,
            condition    VARCHAR(255) NOT NULL,
            title        TEXT         NOT NULL,
            description  TEXT         NOT NULL,
            phone        VARCHAR(255) NOT NULL,
            location     VARCHAR(255) NOT NULL,
            created_utc  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
        );";

}

// ======== 3) Konfigurer middleware‐pipeline (ingen antiforgery!) ========
if (!app.Environment.IsDevelopment())
{
    // Hvis I ønsker en “Error”‐side, skal I have en Blazor‐komponent med @page "/Error" (ikke Razor Page):
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Brug kun “UseRouting” (SignalR‐huben klarer resten under motorhjelmen):
app.UseRouting();

// **Ingen** kald til app.UseAntiforgery(), app.UseAuthentication(), AddRazorPages() osv.
// **Ingen** MapFallbackToPage("/_Host") – den er fjernet fuldstændigt.

// ======== 4) Map Blazor Hub & Razor Components (App) ========
app.MapBlazorHub();
app.MapRazorComponents<App>()
   .DisableAntiforgery()
   .AddInteractiveServerRenderMode();

// ======== 5) Kør applikationen ========
app.Run();
