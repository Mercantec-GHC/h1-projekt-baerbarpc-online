using Blazor_Markedsplads.Components;
using BlazorMarkedsplads.Services;
using BlazorMarkedsplads.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

// ----------- Applikations-opbygning -----------
// Dette er startpunktet for hele vores program.

// 1. Opret en 'builder'. Dette objekt bruges til at konfigurere alle dele af applikationen.
var builder = WebApplication.CreateBuilder(args);

// --- Konfiguration af Services (Dependency Injection Container) ---
// Her "fortæller" vi applikationen, hvilke services den skal kende til.

// Konfigurer SignalR, som er den teknologi, Blazor Server bruger til realtidskommunikation mellem server og browser.
builder.Services.AddSignalR(options =>
{
    // Vi øger den maksimale meddelelsesstørrelse. Dette er vigtigt for at kunne håndtere
    // f.eks. fil-uploads via InputFile-komponenten, da fil-data sendes over SignalR-forbindelsen.
    options.MaximumReceiveMessageSize = 1024 * 1024 * 10; // Sættes til 10 MB.
});

// Tilføj de grundlæggende services, der er nødvendige for at køre Blazor-komponenter.
builder.Services
    .AddRazorComponents()
    // Tilføj specifikt de services, der kræves for Blazor Server-hostingmodellen (interaktivitet via SignalR).
    .AddInteractiveServerComponents();

// Registrer vores egne, custom-byggede services i Dependency Injection (DI) containeren.
// Dette gør, at vi kan få dem "injiceret" i vores komponenter med @inject.
builder.Services.AddHttpContextAccessor(); // En standard-service, der kan give adgang til HTTP-kontekst.

// 'AddScoped' betyder, at der oprettes én instans af servicen pr. bruger-session (pr. connection).
// Dette er afgørende for, at f.eks. UserStateService kan holde styr på én brugers login-status.
// Når en komponent beder om en 'IUserRepository', får den en ny instans af 'UserRepository'.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<UserStateService>();

// 2. Byg selve applikationen baseret på alle de konfigurerede services.
// Fra dette punkt har vi 'app'-objektet, som vi bruger til at definere, hvordan webserveren skal opføre sig.
var app = builder.Build();

// --- Konfiguration af Middleware Pipeline ---
// "Middleware Pipeline" er en kæde af komponenter, der håndterer en indkommende web-forespørgsel.
// Rækkefølgen her er VIGTIG, da forespørgslen sendes igennem dem én efter én.

// Tjekker om applikationen kører i produktionsmiljø.
if (!app.Environment.IsDevelopment())
{
    // Hvis ja, opsættes en generel fejlhåndtering, der viser en pæn fejlside i stedet for en teknisk fejlmeddelelse.
    app.UseExceptionHandler("/Error");
    // Tilføjer en HSTS-header, som tvinger browseren til kun at bruge HTTPS. En sikkerhedsforanstaltning.
    app.UseHsts();
}

// Omdirigerer alle HTTP-forespørgsler til HTTPS.
app.UseHttpsRedirection();

// Gør det muligt for webserveren at servere statiske filer (som CSS, JavaScript og billeder) fra 'wwwroot'-mappen.
app.UseStaticFiles();

// Tilføjer routing-funktionalitet til pipelinen, så URL'er kan matches til de rigtige sider/endpoints.
app.UseRouting();

// Tilføjer beskyttelse mod Cross-Site Request Forgery (CSRF) angreb. Vigtigt for sikkerheden i formularer.
app.UseAntiforgery();

// Kortlægger Blazor-komponenterne til request-pipelinen.
// Den fortæller appen, at den skal starte ved 'App'-komponenten (defineret i App.razor).
// '.AddInteractiveServerRenderMode()' anvender den interaktive server-tilstand til hele applikationen.
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// 3. Start applikationen.
// Denne linje starter webserveren og får den til at lytte efter indkommende forespørgsler. Programmet kører herfra.
app.Run();