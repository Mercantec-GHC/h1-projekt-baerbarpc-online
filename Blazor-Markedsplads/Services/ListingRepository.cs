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
    /// Repository til at håndtere al dataadgang for annoncer (listings) og deres billeder.
    /// </summary>
    public class ListingRepository : IListingRepository
    {
        private readonly string _connectionString;

        public ListingRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("DefaultConnection mangler i appsettings.");
        }

        /// <summary>
        /// Indsætter en ny annonce i 'listings'-tabellen.
        /// Returnerer det nye ID for den oprettede annonce.
        /// </summary>
        public async Task<int> InsertAsync(Listing listing)
        {
            const string sql = @"
            INSERT INTO listings
             (brand, model, gpu, cpu, ram, storage, os, price, screen_size, condition,
             title, description, phone, location, created_utc, user_id) -- <--- TILFØJ user_id
             VALUES
             (@brand, @model, @gpu, @cpu, @ram, @storage, @os, @price, @screen_size, @condition,
             @title, @description, @phone, @location, @created, @user_id) -- <--- TILFØJ @user_id
             RETURNING id;
             ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
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
            cmd.Parameters.AddWithValue("@created", listing.CreatedUtc);

            cmd.Parameters.AddWithValue("@user_id", listing.UserId.HasValue ? (object)listing.UserId.Value : DBNull.Value);

            var idObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
        }

        /// <summary>
        /// Tilføjer en reference til et billede i 'listing_images'-tabellen,
        /// koblet til en specifik annonce via listingId.
        /// </summary>
        public async Task AddImageAsync(int listingId, string imagePath)
        {
            const string sql = @"INSERT INTO listing_images (listing_id, image_path) VALUES (@listingId, @imagePath);";
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@listingId", listingId);
            cmd.Parameters.AddWithValue("@imagePath", imagePath);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Henter en enkelt annonce baseret på ID, inklusiv alle tilknyttede billeder.
        /// </summary>
        public async Task<Listing?> GetByIdAsync(int id)
        {
            // Bygger en SQL-forespørgsel, der starter fra "WHERE"-delen
            var sqlQueryPart = "WHERE l.id = @id LIMIT 1";
            var parameters = new[] { new NpgsqlParameter("@id", id) };

            // Bruger vores centrale helper-metode til at hente data
            var result = await GetListingsWithImagesAsync(sqlQueryPart, parameters);
            return result.FirstOrDefault(); // Returnerer den første (og eneste) annonce eller null
        }

        /// <summary>
        /// Henter et antal "premium"-annoncer (defineret som de dyreste), inklusiv billeder.
        /// </summary>
        public async Task<List<Listing>> GetPremiumAsync(int take = 4)
        {
            var sqlQueryPart = "ORDER BY l.price DESC LIMIT @take";
            var parameters = new[] { new NpgsqlParameter("@take", take) };
            return await GetListingsWithImagesAsync(sqlQueryPart, parameters);
        }

        /// <summary>
        /// Søger efter annoncer på tværs af flere felter, inklusiv billeder.
        /// </summary>
        public async Task<List<Listing>> SearchAsync(string term)
        {
            var sqlQueryPart = @"
                WHERE l.brand ILIKE @t OR l.model ILIKE @t OR l.cpu ILIKE @t OR l.gpu ILIKE @t OR l.os ILIKE @t OR l.title ILIKE @t OR l.description ILIKE @t
                ORDER BY l.created_utc DESC";
            var parameters = new[] { new NpgsqlParameter("@t", $"%{term}%") };
            return await GetListingsWithImagesAsync(sqlQueryPart, parameters);
        }

        /// <summary>
        /// Henter alle annoncer for en specifik bruger.
        /// OBS: Kræver at du har en 'user_id' kolonne i din 'listings' tabel.
        /// </summary>
        public async Task<List<Listing>> GetByUserIdAsync(int userId)
        {
            var sqlQueryPart = "WHERE l.user_id = @uid ORDER BY l.created_utc DESC";
            var parameters = new[] { new NpgsqlParameter("@uid", userId) };
            return await GetListingsWithImagesAsync(sqlQueryPart, parameters);
        }

        // ===================================================================================
        // PRIVATE HELPER-METODER
        // ===================================================================================

        /// <summary>
        /// Central metode til at hente annoncer og deres billeder.
        /// Den tager en SQL- stump (WHERE/ORDER BY) og samler data korrekt.
        /// </summary>
        private async Task<List<Listing>> GetListingsWithImagesAsync(string sqlQueryPart, NpgsqlParameter[]? parameters = null)
        {
            // Vi bruger en Dictionary til effektivt at samle billeder under den korrekte annonce.
            var listingsDictionary = new Dictionary<int, Listing>();

            // Basis SQL-forespørgsel der forbinder 'listings' med 'listing_images'.
            // LEFT JOIN sikrer, at vi også får annoncer, der endnu ikke har billeder.
            const string baseSql = @"
              SELECT
              l.id, l.brand, l.model, l.gpu, l.cpu, l.ram, l.storage, l.os, l.price, 
              l.screen_size, l.condition, l.title, l.description, l.phone, l.location, l.created_utc,
              l.user_id, -- Hent brugerens ID
              u.name as seller_name, -- Hent brugerens navn (med alias)
              u.email as seller_email, -- Hent brugerens email (med alias)
              li.id as image_id, 
              li.image_path
              FROM listings l
              LEFT JOIN listing_images li ON l.id = li.listing_id
              LEFT JOIN users u ON l.user_id = u.id -- <--- TILFØJ DETTE JOIN
              ";

            var finalSql = baseSql + sqlQueryPart; // Sæt basis og den specifikke del sammen.

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(finalSql, conn);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var listingId = reader.GetInt32(reader.GetOrdinal("id"));

                // Tjek om vi allerede har set denne annonce. Hvis ikke, opret den.
                if (!listingsDictionary.TryGetValue(listingId, out var listing))
                {
                    // OPDATER MAPPING LOGIKKEN
                    listing = MapReaderToListing(reader); // Kald opdateret helper
                    listingsDictionary.Add(listingId, listing);
                }

                // Tjek om der er et billede på denne række i resultatet.
                if (!reader.IsDBNull(reader.GetOrdinal("image_id")))
                {
                    listing.Images.Add(new ListingImage
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("image_id")),
                        ImagePath = reader.GetString(reader.GetOrdinal("image_path")),
                        ListingId = listingId
                    });
                }
            }
            return listingsDictionary.Values.ToList();
        }

        /// <summary>
        /// Mapper en enkelt række fra databasen til et Listing-objekt for at undgå at gentage kode.
        /// </summary>
        private Listing MapReaderToListing(NpgsqlDataReader reader)
        {
            return new Listing
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
        }


        public async Task UpdateAsync(Listing listing)
        {
            const string sql = @"
        UPDATE listings SET
            brand = @brand,
            model = @model,
            gpu = @gpu,
            cpu = @cpu,
            ram = @ram,
            storage = @storage,
            os = @os,
            price = @price,
            screen_size = @screen_size,
            condition = @condition,
            title = @title,
            description = @description,
            phone = @phone,
            location = @location
        WHERE id = @id;
    ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);

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
            cmd.Parameters.AddWithValue("@id", listing.Id); // Vigtigt at specificere hvilken annonce

            await cmd.ExecuteNonQueryAsync();


        }



        public async Task DeleteAsync(int listingId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Start en transaktion, så begge operationer enten lykkes eller fejler samlet
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Trin 1: Slet referencer i listing_images
                var deleteImagesSql = "DELETE FROM listing_images WHERE listing_id = @id";
                await using (var cmd = new NpgsqlCommand(deleteImagesSql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", listingId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Trin 2: Slet selve annoncen
                var deleteListingSql = "DELETE FROM listings WHERE id = @id";
                await using (var cmd = new NpgsqlCommand(deleteListingSql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", listingId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Hvis alt gik godt, gem ændringerne
                await transaction.CommitAsync();
            }
            catch
            {
                // Hvis noget fejler, rul tilbage
                await transaction.RollbackAsync();
                throw; // Kast fejlen videre
            }
        }
    }
}