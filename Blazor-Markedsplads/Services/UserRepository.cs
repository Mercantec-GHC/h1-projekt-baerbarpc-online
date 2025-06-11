using System;
using System.Threading.Tasks;
using BlazorMarkedsplads.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BlazorMarkedsplads.Services
{
    /// <summary>
    /// Denne klasse er den konkrete implementering af IUserRepository.
    /// Den indeholder den faktiske kode, der forbinder til databasen og udfører operationer på 'users'-tabellen.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        // En privat streng, der holder forbindelsesstrengen til databasen.
        private readonly string _connectionString;

        /// <summary>
        /// Klassens constructor, som modtager konfigurations-servicen via Dependency Injection.
        /// </summary>
        public UserRepository(IConfiguration configuration)
        {
            // Læser forbindelsesstrengen fra appsettings.json og gemmer den i den private variabel.
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("DefaultConnection mangler i appsettings.");
        }

        // --- Implementering af Interface Metoder ---

        /// <inheritdoc />
        public async Task<int> InsertAsync(User user)
        {
            // Definerer den SQL-kommando, der skal udføres.
            // RETURNING id; er en PostgreSQL-specifik kommando, der returnerer ID'et på den nye række.
            const string sql = @"
            INSERT INTO users 
                (name, email, password, phone, address, city, zip_code)
            VALUES
                (@name, @mail, @pwd, @phone, @addr, @city, @zip)
            RETURNING id;";

            // 'await using' sikrer, at databaseforbindelsen bliver oprettet og lukket korrekt.
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(); // Åbner forbindelsen til databasen.
            await using var cmd = new NpgsqlCommand(sql, conn); // Opretter et kommando-objekt.

            // Tilføjer værdier til SQL-kommandoens parametre for at forhindre SQL Injection.
            // Værdien fra 'user.Name' bliver f.eks. sikkert indsat, hvor der står '@name'.
            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@mail", user.Email);
            cmd.Parameters.AddWithValue("@pwd", user.Password); // Bemærk: Dette forventer det FÆRDIG-HASHEDE password.
            cmd.Parameters.AddWithValue("@phone", user.Phone ?? string.Empty); // Sikrer, at vi aldrig indsætter 'null' i tekst-felter.
            cmd.Parameters.AddWithValue("@addr", user.Address ?? string.Empty);
            cmd.Parameters.AddWithValue("@city", user.City ?? string.Empty);
            cmd.Parameters.AddWithValue("@zip", user.ZipCode ?? string.Empty);

            // Udfører kommandoen og returnerer den første værdi (vores ID).
            var result = await cmd.ExecuteScalarAsync();
            // Konverterer resultatet til en int og returnerer det.
            return Convert.ToInt32(result);
        }

        /// <inheritdoc />
        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = "SELECT * FROM users WHERE email = @mail LIMIT 1;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mail", email);

            // ExecuteReaderAsync bruges, når vi forventer at læse en eller flere rækker fra databasen.
            await using var reader = await cmd.ExecuteReaderAsync();

            // Forsøger at læse den første række. Hvis det lykkes (dvs. en bruger blev fundet),...
            if (await reader.ReadAsync())
            {
                // ...opretter vi et nyt User-objekt og udfylder det med data fra læseren.
                return new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Password = reader.GetString(reader.GetOrdinal("password")), // Vigtigt at hente det hashede password til verificering.
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    City = reader.GetString(reader.GetOrdinal("city")),
                    ZipCode = reader.GetString(reader.GetOrdinal("zip_code"))
                };
            }

            // Hvis 'reader.ReadAsync()' returnerer false, blev der ikke fundet nogen bruger, og vi returnerer null.
            return null;
        }

        /// <inheritdoc />
        public async Task<User?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM users WHERE id = @uid;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                // Mapper data fra databaselæseren til et nyt User-objekt.
                return new User
                {
                    Id = id,
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Password = reader.GetString(reader.GetOrdinal("password")),
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    City = reader.GetString(reader.GetOrdinal("city")),
                    ZipCode = reader.GetString(reader.GetOrdinal("zip_code"))
                };
            }

            return null;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(User user)
        {
            // Definerer en UPDATE SQL-kommando.
            // Bemærk, at vi bevidst IKKE opdaterer 'email' eller 'password' her.
            // Email er typisk en unik identifikator, man ikke ændrer, og password har sin egen dedikerede metode.
            const string sql = @"
            UPDATE users 
            SET
                name     = @name,
                phone    = @phone,
                address  = @addr,
                city     = @city,
                zip_code = @zip
            WHERE id = @uid;"; // WHERE-betingelsen er altafgørende for kun at opdatere den korrekte bruger.

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@phone", user.Phone ?? string.Empty);
            cmd.Parameters.AddWithValue("@addr", user.Address ?? string.Empty);
            cmd.Parameters.AddWithValue("@city", user.City ?? string.Empty);
            cmd.Parameters.AddWithValue("@zip", user.ZipCode ?? string.Empty);
            cmd.Parameters.AddWithValue("@uid", user.Id);

            // ExecuteNonQueryAsync bruges til kommandoer, der ikke returnerer data.
            await cmd.ExecuteNonQueryAsync();
        }

        /// <inheritdoc />
        public async Task UpdatePasswordAsync(int userId, string newHashedPassword)
        {
            const string sql = "UPDATE users SET password = @pwd WHERE id = @uid;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);

            // Indsætter det nye, FÆRDIG-HASHEDE password i databasen.
            // Denne metode stoler på, at den kode, der kalder den (f.eks. UserProfile.razor),
            // allerede har hashet passwordet korrekt med BCrypt.
            cmd.Parameters.AddWithValue("@pwd", newHashedPassword);
            cmd.Parameters.AddWithValue("@uid", userId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}