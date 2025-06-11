using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;
using BlazorMarkedsplads.Models; // Sørg for at begge dine modeller er tilgængelige
using Microsoft.Extensions.Configuration;
using Npgsql; // Database-driver

namespace BlazorMarkedsplads.Services
{
    /// <summary>
    /// Denne klasse implementerer IListingRepository-interfacet. Den indeholder den konkrete logik
    /// for at udføre databaseoperationer (CRUD: Create, Read, Update, Delete) på 'listings'-tabellen.
    /// Dette kaldes Repository Pattern, som adskiller dataadgangslogik fra resten af applikationen.
    /// </summary>
    public class ListingRepository : IListingRepository
    {
        // En privat, skrivebeskyttet (_readonly) streng, der holder forbindelsesstrengen til databasen.
        private readonly string _connectionString;

        /// <summary>
        /// Dette er klassens constructor. Den kører, når en ny instans af ListingRepository oprettes.
        /// Dependency Injection sørger for at give den en 'IConfiguration'-service.
        /// </summary>
        /// <param name="configuration">En service, der kan læse fra appsettings.json.</param>
        public ListingRepository(IConfiguration configuration)
        {
            // Hent forbindelsesstrengen ved navn "DefaultConnection" fra appsettings.json.
            // '?? throw...' er en sikkerhedsforanstaltning, der crasher programmet med en klar fejl,
            // hvis forbindelsesstrengen mangler, hvilket forhindrer uventede fejl senere.
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("DefaultConnection mangler i appsettings.");
        }

        // --- Offentlige metoder (implementering af interfacet) ---

        /// <inheritdoc />
        public async Task<int> InsertAsync(Listing listing)
        {
            // Definerer SQL-kommandoen. '@'-tegnet foran strengen tillader strengen at gå over flere linjer.
            // RETURNING id; er en PostgreSQL-specifik kommando, der effektivt returnerer ID'et på den netop indsatte række.
            const string sql = @"
            INSERT INTO listings
             (brand, model, gpu, cpu, ram, storage, os, price, screen_size, condition,
             title, description, phone, location, created_utc, user_id)
             VALUES
             (@brand, @model, @gpu, @cpu, @ram, @storage, @os, @price, @screen_size, @condition,
             @title, @description, @phone, @location, @created, @user_id)
             RETURNING id;";

            // 'await using' sikrer, at databaseforbindelsen bliver oprettet og lukket korrekt, selv hvis der sker fejl.
            await using var conn = new NpgsqlConnection(_connectionString);
            // Åbner forbindelsen til databasen asynkront.
            await conn.OpenAsync();

            // Opretter et kommando-objekt med SQL-strengen og forbindelsen.
            await using var cmd = new NpgsqlCommand(sql, conn);

            // Tilføjelse af parametre. Dette er den sikre måde at indsætte data på og forhindrer SQL Injection.
            // Værdierne fra 'listing'-objektet bliver bundet til pladsholderne (@brand, @model etc.) i SQL-strengen.
            cmd.Parameters.AddWithValue("@brand", listing.Brand);
            cmd.Parameters.AddWithValue("@model", listing.Model);
            // ... (resten af parametrene) ...
            cmd.Parameters.AddWithValue("@gpu", listing.Gpu);
            cmd.Parameters.AddWithValue("@cpu", listing.Cpu);
            cmd.Parameters.AddWithValue("@ram", listing.Ram);
            cmd.Parameters.AddWithValue("@storage", listing.Storage);
            cmd.Parameters.AddWithValue("@os", listing.OS);
            cmd.Parameters.AddWithValue("@price", listing.Price);
            cmd.Parameters.AddWithValue("@screen_size", listing.ScreenSize);
            cmd.Parameters.AddWithValue("@condition", listing.Condition);
            cmd.Parameters.AddWithValue("@title", listing.Title);
            cmd.Parameters.AddWithValue("@description", listing.Description);
            cmd.Parameters.AddWithValue("@phone", listing.Phone);
            cmd.Parameters.AddWithValue("@location", listing.Location);
            cmd.Parameters.AddWithValue("@created", listing.CreatedUtc);
            // Håndterer, at UserId kan være 'null'. Hvis det er, indsættes en database-specifik 'NULL'-værdi.
            cmd.Parameters.AddWithValue("@user_id", listing.UserId.HasValue ? (object)listing.UserId.Value : DBNull.Value);

            // Udfører kommandoen og returnerer den første værdi fra den første række (vores 'RETURNING id').
            var idObj = await cmd.ExecuteScalarAsync();
            // Konverterer det returnerede objekt (som kan være f.eks. en long) til en standard int.
            return Convert.ToInt32(idObj);
        }

        /// <inheritdoc />
        public async Task AddImageAsync(int listingId, string imagePath)
        {
            // Simpel SQL til at indsætte en ny række i 'listing_images'-tabellen.
            const string sql = @"INSERT INTO listing_images (listing_id, image_path) VALUES (@listingId, @imagePath);";
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@listingId", listingId);
            cmd.Parameters.AddWithValue("@imagePath", imagePath);
            // ExecuteNonQueryAsync bruges, når vi ikke forventer data retur (som ved INSERT, UPDATE, DELETE uden RETURNING).
            await cmd.ExecuteNonQueryAsync();
        }

        /// <inheritdoc />
        public async Task<Listing?> GetByIdAsync(int id)
        {
            // Definerer den unikke del af SQL'en for denne metode.
            var sqlQueryPart = "WHERE l.id = @id LIMIT 1";
            // Definerer de nødvendige parametre.
            var parameters = new[] { new NpgsqlParameter("@id", id) };

            // Genbruger vores centrale helper-metode til at udføre selve databasekaldet.
            var result = await GetListingsWithImagesAsync(sqlQueryPart, parameters);
            // Returnerer det første element i listen (som højst vil indeholde ét element), eller null hvis listen er tom.
            return result.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<List<Listing>> GetPremiumAsync(int take = 4)
        {
            var sqlQueryPart = "ORDER BY l.price DESC LIMIT @take";
            var parameters = new[] { new NpgsqlParameter("@take", take) };
            return await GetListingsWithImagesAsync(sqlQueryPart, parameters);
        }

        /// <inheritdoc />
        public async Task<List<Listing>> SearchAsync(string term)
        {
            // ILIKE er en PostgreSQL-specifik, case-insensitiv version af LIKE.
            // %-tegnet er et wildcard, der matcher enhver sekvens af tegn. '%{term}%' finder 'term' overalt i teksten.
            var sqlQueryPart = @"
            WHERE l.brand ILIKE @t OR l.model ILIKE @t OR l.cpu ILIKE @t OR l.gpu ILIKE @t OR l.os ILIKE @t OR l.title ILIKE @t OR l.description ILIKE @t
            ORDER BY l.created_utc DESC";
            var parameters = new[] { new NpgsqlParameter("@t", $"%{term}%") };
            return await GetListingsWithImagesAsync(sqlQueryPart, parameters);
        }

        /// <inheritdoc />
        public async Task<List<Listing>> GetByUserIdAsync(int userId)
        {
            var sqlQueryPart = "WHERE l.user_id = @uid ORDER BY l.created_utc DESC";
            var parameters = new[] { new NpgsqlParameter("@uid", userId) };
            return await GetListingsWithImagesAsync(sqlQueryPart, parameters);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Listing listing)
        {
            const string sql = @"
            UPDATE listings SET
                brand = @brand, model = @model, gpu = @gpu, cpu = @cpu, ram = @ram, 
                storage = @storage, os = @os, price = @price, screen_size = @screen_size, 
                condition = @condition, title = @title, description = @description, 
                phone = @phone, location = @location
            WHERE id = @id;"; // WHERE-delen er afgørende for kun at opdatere den korrekte række.

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);

            // Tilføjelse af alle parametre, inklusiv ID'et for den annonce, der skal opdateres.
            cmd.Parameters.AddWithValue("@brand", listing.Brand);
            cmd.Parameters.AddWithValue("@model", listing.Model);
            cmd.Parameters.AddWithValue("@gpu", listing.Gpu);
            cmd.Parameters.AddWithValue("@cpu", listing.Cpu);
            cmd.Parameters.AddWithValue("@ram", listing.Ram);
            cmd.Parameters.AddWithValue("@storage", listing.Storage);
            cmd.Parameters.AddWithValue("@os", listing.OS);
            cmd.Parameters.AddWithValue("@price", listing.Price);
            cmd.Parameters.AddWithValue("@screen_size", listing.ScreenSize);
            cmd.Parameters.AddWithValue("@condition", listing.Condition);
            cmd.Parameters.AddWithValue("@title", listing.Title);
            cmd.Parameters.AddWithValue("@description", listing.Description);
            cmd.Parameters.AddWithValue("@phone", listing.Phone);
            cmd.Parameters.AddWithValue("@location", listing.Location);
            cmd.Parameters.AddWithValue("@id", listing.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(int listingId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // En transaktion sikrer, at BEGGE slette-operationer (fra 'listing_images' og 'listings')
            // bliver udført succesfuldt. Hvis en af dem fejler, bliver ingen af ændringerne gemt (rollback).
            // Dette forhindrer "forældreløse" billeder i databasen.
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Trin 1: Slet alle billedreferencer, der hører til annoncen.
                var deleteImagesSql = "DELETE FROM listing_images WHERE listing_id = @id";
                await using (var cmd = new NpgsqlCommand(deleteImagesSql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", listingId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Trin 2: Slet selve annoncen fra 'listings'-tabellen.
                var deleteListingSql = "DELETE FROM listings WHERE id = @id";
                await using (var cmd = new NpgsqlCommand(deleteListingSql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", listingId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Hvis begge kommandoer lykkedes, gemmes ændringerne permanent i databasen.
                await transaction.CommitAsync();
            }
            catch
            {
                // Hvis en fejl opstod i try-blokken, rulles alle ændringer tilbage.
                await transaction.RollbackAsync();
                // 'throw' kaster fejlen videre op i systemet, så den kan håndteres eller logges.
                throw;
            }
        }

        // --- PRIVATE HELPER-METODER ---

        /// <summary>
        /// En central, genbrugelig metode til at hente lister af annoncer.
        /// Den kombinerer data fra 'listings', 'listing_images' og 'users' tabellerne i ét enkelt, effektivt databasekald.
        /// </summary>
        /// <param name="sqlQueryPart">Den SQL-stump (f.eks. WHERE, ORDER BY), der adskiller de forskellige Get-metoder.</param>
        /// <param name="parameters">De SQL-parametre, der hører til sqlQueryPart.</param>
        /// <returns>En færdig liste af 'Listing'-objekter med billeder og sælger-info.</returns>
        private async Task<List<Listing>> GetListingsWithImagesAsync(string sqlQueryPart, NpgsqlParameter[]? parameters = null)
        {
            // En Dictionary bruges til midlertidigt at holde de unikke annoncer, vi finder.
            // Den bruger annonce-ID som nøgle for hurtigt at kunne slå op og undgå dubletter.
            var listingsDictionary = new Dictionary<int, Listing>();

            // Den grundlæggende SQL, der henter alle nødvendige kolonner og forbinder tabellerne.
            // LEFT JOIN bruges for at sikre, at vi også får annoncer, der ikke har billeder eller en tilknyttet bruger.
            const string baseSql = @"
            SELECT
                l.id, l.brand, l.model, l.gpu, l.cpu, l.ram, l.storage, l.os, l.price, 
                l.screen_size, l.condition, l.title, l.description, l.phone, l.location, l.created_utc,
                l.user_id,
                u.name as seller_name,
                u.email as seller_email,
                li.id as image_id, 
                li.image_path
            FROM listings l
            LEFT JOIN listing_images li ON l.id = li.listing_id
            LEFT JOIN users u ON l.user_id = u.id ";

            // Sammensæt den fulde SQL-kommando.
            var finalSql = baseSql + sqlQueryPart;

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(finalSql, conn);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            // ExecuteReader bruges, når vi forventer flere rækker retur.
            await using var reader = await cmd.ExecuteReaderAsync();
            // Løkken kører for hver række, databasen returnerer.
            while (await reader.ReadAsync())
            {
                var listingId = reader.GetInt32(reader.GetOrdinal("id"));

                // Tjekker, om vi allerede har oprettet et Listing-objekt for dette ID.
                if (!listingsDictionary.TryGetValue(listingId, out var listing))
                {
                    // Hvis ikke, opret et nyt Listing-objekt ved hjælp af vores mapper-metode.
                    listing = MapReaderToListing(reader);
                    // Tilføj det nye objekt til vores dictionary, så vi kan finde det igen.
                    listingsDictionary.Add(listingId, listing);
                }

                // Tjekker, om rækken indeholder billed-information (image_id vil ikke være DBNull).
                if (!reader.IsDBNull(reader.GetOrdinal("image_id")))
                {
                    // Hvis ja, opret et nyt ListingImage-objekt og tilføj det til den korrekte annonces billedliste.
                    listing.Images.Add(new ListingImage
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("image_id")),
                        ImagePath = reader.GetString(reader.GetOrdinal("image_path")),
                        ListingId = listingId
                    });
                }
            }
            // Returner værdierne fra dictionary'en som en liste.
            return listingsDictionary.Values.ToList();
        }

        /// <summary>
        /// Mapper en enkelt række fra en NpgsqlDataReader til et færdigt Listing-objekt.
        /// Inkluderer nu mapping af sælger-information.
        /// </summary>
        private Listing MapReaderToListing(NpgsqlDataReader reader)
        {
            var listing = new Listing
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Brand = reader.GetString(reader.GetOrdinal("brand")),
                Model = reader.GetString(reader.GetOrdinal("model")),
                Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                Ram = reader.GetInt32(reader.GetOrdinal("ram")),
                Storage = reader.GetInt32(reader.GetOrdinal("storage")),
                OS = reader.GetString(reader.GetOrdinal("os")),
                Price = reader.GetDecimal(reader.GetOrdinal("price")),
                ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                Condition = reader.GetString(reader.GetOrdinal("condition")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Phone = reader.GetString(reader.GetOrdinal("phone")),
                Location = reader.GetString(reader.GetOrdinal("location")),
                CreatedUtc = reader.GetDateTime(reader.GetOrdinal("created_utc")),
                UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? null : reader.GetInt32(reader.GetOrdinal("user_id"))
            };

            // **RETTELSE:** Mapper nu også sælger-info, som vi henter i baseSql.
            if (listing.UserId.HasValue && !reader.IsDBNull(reader.GetOrdinal("seller_name")))
            {
                listing.Seller = new User
                {
                    Id = listing.UserId.Value,
                    Name = reader.GetString(reader.GetOrdinal("seller_name")),
                    Email = reader.GetString(reader.GetOrdinal("seller_email"))
                    // Password og andre følsomme oplysninger hentes bevidst ikke.
                };
            }

            return listing;
        }
    }
}