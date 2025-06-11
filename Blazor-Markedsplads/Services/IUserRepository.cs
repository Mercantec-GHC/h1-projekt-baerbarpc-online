using System.Threading.Tasks;
using BlazorMarkedsplads.Models;

namespace BlazorMarkedsplads.Services
{
    // ----------- Interface-definition -----------
    // Et 'interface' er en kontrakt, der definerer en række metoder, som en klasse skal implementere.
    // 'IUserRepository' specificerer, hvilke operationer der skal være mulige at udføre på brugere i databasen.
    // Den konkrete implementering findes i 'UserRepository.cs'.
    public interface IUserRepository
    {
        // --- Metodesignaturer ---
        // Hver linje er en "underskrift" på en metode, der skal findes i den implementerende klasse.

        /// <summary>
        /// Opretter en ny bruger i databasen.
        /// </summary>
        /// <param name="user">Et 'User'-objekt, der indeholder alle data for den nye bruger.</param>
        /// <returns>En 'Task', der, når den er færdig, returnerer det unikke ID (en 'int') for den nyoprettede bruger.</returns>
        Task<int> InsertAsync(User user);

        /// <summary>
        /// Henter en enkelt bruger fra databasen baseret på deres unikke email-adresse.
        /// Anvendes primært i login-processen.
        /// </summary>
        /// <param name="email">Den email-adresse, der skal søges efter.</param>
        /// <returns>En 'Task', der returnerer det fundne 'User'-objekt, eller 'null' hvis ingen bruger med den email findes.</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Henter en enkelt bruger fra databasen baseret på deres unikke ID.
        /// Anvendes typisk til at hente en logget-ind brugers profiloplysninger.
        /// </summary>
        /// <param name="id">Det unikke ID for den bruger, der skal hentes.</param>
        /// <returns>En 'Task', der returnerer det fundne 'User'-objekt, eller 'null' hvis ingen bruger med det ID findes. '?' i User? indikerer, at 'null' er en gyldig returværdi.</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Opdaterer en eksisterende brugers profiloplysninger (navn, adresse osv.) i databasen.
        /// Denne metode bør IKKE bruges til at opdatere adgangskoden.
        /// </summary>
        /// <param name="user">Et 'User'-objekt, der indeholder de opdaterede data, inklusiv ID'et på den bruger, der skal opdateres.</param>
        /// <returns>En 'Task', der repræsenterer, at operationen er fuldført.</returns>
        Task UpdateAsync(User user);

        /// <summary>
        /// Opdaterer adgangskoden for en specifik bruger.
        /// </summary>
        /// <param name="userId">ID'et på den bruger, hvis adgangskode skal ændres.</param>
        /// <param name="newPassword">Den nye, FÆRDIG-HASHEDE adgangskode, der skal gemmes i databasen.</param>
        /// <returns>En 'Task', der repræsenterer, at operationen er fuldført.</returns>
        Task UpdatePasswordAsync(int userId, string newPassword);
    }
}