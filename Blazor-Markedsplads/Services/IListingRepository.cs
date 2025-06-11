using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorMarkedsplads.Models;

namespace BlazorMarkedsplads.Services
{
    public interface IListingRepository
    {
        // --- Metodesignaturer ---
        // Hver linje her er en "underskrift" på en metode, der skal findes i den implementerende klasse.

        /// <summary>
        /// Indsætter en ny annonce i databasen.
        /// </summary>
        /// <param name="listing">Et 'Listing'-objekt, der indeholder alle data for den nye annonce.</param>
        /// <returns>En 'Task', der, når den er færdig, returnerer det unikke ID (en 'int') for den nyoprettede annonce.</returns>
        Task<int> InsertAsync(Listing listing);

        /// <summary>
        /// Henter et bestemt antal "premium"-annoncer (typisk de nyeste eller dyreste) til visning på forsiden.
        /// </summary>
        /// <param name="take">Et valgfrit tal, der specificerer, hvor mange annoncer der skal hentes. Standard er 4.</param>
        /// <returns>En 'Task', der returnerer en liste ('List<Listing>') af annonce-objekter.</returns>
        Task<List<Listing>> GetPremiumAsync(int take = 4);

        /// <summary>
        /// Søger efter annoncer i databasen, hvor et eller flere felter matcher et søgeord.
        /// </summary>
        /// <param name="term">Det søgeord, brugeren har indtastet.</param>
        /// <returns>En 'Task', der returnerer en liste over de annoncer, der matchede søgningen.</returns>
        Task<List<Listing>> SearchAsync(string term);

        /// <summary>
        /// Henter alle annoncer, der er oprettet af en specifik bruger. Anvendes på UserProfile-siden.
        /// </summary>
        /// <param name="userId">ID'et på den bruger, hvis annoncer vi vil finde.</param>
        /// <returns>En 'Task', der returnerer en liste over brugerens annoncer.</returns>
        Task<List<Listing>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Henter én specifik annonce baseret på dens unikke ID. Anvendes på ListingDetailsPage.
        /// </summary>
        /// <param name="id">ID'et på den annonce, der skal hentes.</param>
        /// <returns>En 'Task', der returnerer det fundne 'Listing'-objekt, eller 'null' hvis ingen annonce med det ID findes. '?' i Listing? indikerer, at 'null' er en gyldig returværdi.</returns>
        Task<Listing?> GetByIdAsync(int id);

        /// <summary>
        /// Tilføjer en reference til et billede i databasen og kobler det til en specifik annonce.
        /// </summary>
        /// <param name="listingId">ID'et på den annonce, billedet tilhører.</param>
        /// <param name="imagePath">Det unikke filnavn på billedet, som det er gemt på serveren.</param>
        /// <returns>En 'Task', der repræsenterer, at operationen er fuldført. Returnerer ikke en værdi.</returns>
        Task AddImageAsync(int listingId, string imagePath);

        /// <summary>
        /// Opdaterer en eksisterende annonce i databasen med nye oplysninger.
        /// </summary>
        /// <param name="listing">Et 'Listing'-objekt, der indeholder de opdaterede data (inklusive dets ID).</param>
        /// <returns>En 'Task', der repræsenterer, at operationen er fuldført.</returns>
        Task UpdateAsync(Listing listing);

        /// <summary>
        /// Sletter en annonce (og dens tilknyttede billedreferencer) fra databasen.
        /// </summary>
        /// <param name="listingId">ID'et på den annonce, der skal slettes.</param>
        /// <returns>En 'Task', der repræsenterer, at operationen er fuldført.</returns>
        Task DeleteAsync(int listingId);
    }

}
