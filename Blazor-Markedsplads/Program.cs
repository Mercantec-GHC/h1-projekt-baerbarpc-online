using BlazorMarkedsplads.Services;                    // som før
using BlazorMarkedsplads.Components;
using BlazorMarkedsplads;
using Microsoft.Extensions.DependencyInjection;
using Blazor_Markedsplads.Components;       // ? NYT (til DI helpers)

var builder = WebApplication.CreateBuilder(args);



/* ---------- Services ---------- */
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

/* eksisterende service */
builder.Services.AddScoped<DBService>();

/* NYT: repository-lag til annoncer  */
builder.Services.AddScoped<IListingRepository, ListingRepository>();

/* NYT: repository-lag til brugere  */
builder.Services.AddScoped<DBService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();

var app = builder.Build();

/* ---------- Engangs-setup af DB-tabeller ---------- */
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DBService>();

    // (her kunne du også kalde SetupUserTable / SetupProductModelsTable
    //  hvis du vil køre dem ved opstart)
    await db.SetupListingsTableAsync();               // ? NYT
}

/* ---------- Pipeline ---------- */
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();                                   // prod-HSTS
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
