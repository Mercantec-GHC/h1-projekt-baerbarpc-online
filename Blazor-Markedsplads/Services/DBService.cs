using Npgsql;
using Microsoft.Extensions.Configuration;
using BlazorMarkedsplads.Models;
using System.Text;
using System.Data;

namespace BlazorMarkedsplads.Services
{
    public class DBService
    {
        private readonly string _connectionString;

        public DBService(IConfiguration configuration)
            => _connectionString = configuration.GetConnectionString("DefaultConnection");

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // --- USERS ----------------------------------------------------

        public async Task<string> SetupUserTable()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id         SERIAL PRIMARY KEY,
                    name       VARCHAR(255),
                    email      VARCHAR(255),
                    password   VARCHAR(255),
                    phone      VARCHAR(255),
                    address    VARCHAR(255),
                    city       VARCHAR(255),
                    zip_code   VARCHAR(255)
                );";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();

            return "User table created successfully";
        }

        public async Task<List<User>> GetAllUsers()
        {
            var list = new List<User>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT * FROM users", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Password = reader.GetString(reader.GetOrdinal("password")),
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    City = reader.GetString(reader.GetOrdinal("city")),
                    ZipCode = reader.GetString(reader.GetOrdinal("zip_code"))
                });
            }

            return list;
        }


 
        public async Task<FilterOptions> GetFilterOptionsAsync()
        {
            async Task<List<string>> Distinct(string column)
            {
                var list = new List<string>();
                var sql = $"SELECT DISTINCT {column} FROM product_models ORDER BY {column};";

                await using var c = new NpgsqlConnection(_connectionString);
                await c.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, c);

                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    list.Add(r.GetString(0));

                return list;
            }

            return new FilterOptions
            {
                Brands = await Distinct("brand"),
                Cpus = await Distinct("cpu"),
                Rams = await Distinct("ram"),
                Storages = await Distinct("storage"),
                ScreenSizes = await Distinct("screen_size"),
                OSes = await Distinct("os"),
                Conditions = await Distinct("condition")
            };
        }

   
        /* =========================================================
 *  USERS – CRUD-hjælpere til CreateUser / Login / Profile
 * ========================================================= */

        /// <summary>
        /// Henter alle annoncer (listings) for en given bruger (user_id),
        /// og join’er med product_models for at få specs/pris.
        /// </summary>
        public async Task<List<ListingCardModel>> GetListingsByUserIdAsync(int userId)
        {
            var list = new List<ListingCardModel>();

            // Her antager vi, at vi har sat 'listings'‐tabellen op som:
            //   id, product_id, user_id, title, description, phone, location, created_utc
            // Og at product_models‐tabellen har felter: brand, model, price etc.
            //
            // Vi join’er så på product_id for at hente CPU/RAM/etc. (i Subtitle)

            const string sql = @"
SELECT 
    l.id                   AS listing_id,
    l.title                AS title,
    pm.brand               AS brand,
    pm.model               AS model,
    pm.cpu                 AS cpu,
    pm.ram                 AS ram,
    pm.storage             AS storage,
    pm.price               AS price,
    l.created_utc          AS created_utc
    -- Du kan lægge flere felter ind her, fx status fra l.status, views osv.
FROM listings AS l
JOIN product_models AS pm
    ON l.product_id = pm.id
WHERE l.user_id = @uid
ORDER BY l.created_utc DESC;
";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Vi samler Subtitle: “cpu, ram RAM, storage”
                var cpu = reader.GetString(reader.GetOrdinal("cpu"));
                var ram = reader.GetString(reader.GetOrdinal("ram"));
                var storage = reader.GetString(reader.GetOrdinal("storage"));
                var subtitle = $"{cpu}, {ram} RAM, {storage}";

                // Hent prisen (gemt som tekst “12 499 kr” eller lignende),
                // konverter til decimal hvis du har brug for det:
                var rawPrice = reader.GetString(reader.GetOrdinal("price"));
                var priceVal = CleanPrice(rawPrice);

                // Opret model‐instans
                list.Add(new ListingCardModel
                {
                    ListingId = reader.GetInt32(reader.GetOrdinal("listing_id")),
                    Title = reader.GetString(reader.GetOrdinal("title")),
                    Subtitle = subtitle,
                    Price = priceVal,
                    CreatedUtc = reader.GetDateTime(reader.GetOrdinal("created_utc")),
                    // For Views / Status: enten tilføj kolonne "views" og "status" i SQL‐query,
                    // eller sæt dummy‐værdi:
                    Views = 0,           // alternativt hent r.GetInt32("views")
                    Status = "Aktiv"      // eller tag det fra reader.GetString("status") 
                });
            }

            return list;
        }

        /// <summary>
        /// Hjælpefunktion til at konvertere den tekst‐pris, vi gemmer i DB, til decimal‐format.
        /// Fjerner alt ikke‐numerisk og konverterer til integer.
        /// </summary>
        private static decimal CleanPrice(string raw)
        {
            // Træk cifre ud (lader os konvertere “12 499 kr” til 12499)
            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var v))
                return v;
            return 0m;
        }



        public async Task<int> InsertUserAsync(User u)
        {
            const string sql = @"
        INSERT INTO users (name,email,password,phone,address,city,zip_code)
        VALUES           (@name,@mail,@pwd,@phone,@addr,@city,@zip)
        RETURNING id;";

            await using var c = new NpgsqlConnection(_connectionString);
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@name", u.Name);
            cmd.Parameters.AddWithValue("@mail", u.Email);
            cmd.Parameters.AddWithValue("@pwd", u.Password);      // ← hash i produktion!
            cmd.Parameters.AddWithValue("@phone", u.Phone ?? "");
            cmd.Parameters.AddWithValue("@addr", u.Address ?? "");
            cmd.Parameters.AddWithValue("@city", u.City ?? "");
            cmd.Parameters.AddWithValue("@zip", u.ZipCode ?? "");

            var idObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            const string sql = "SELECT * FROM users WHERE email = @e LIMIT 1;";
            await using var c = new NpgsqlConnection(_connectionString);
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@e", email);

            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync())
                return new User
                {
                    Id = r.GetInt32("id"),
                    Name = r.GetString("name"),
                    Email = r.GetString("email"),
                    Password = r.GetString("password"),
                    Phone = r.GetString("phone"),
                    Address = r.GetString("address"),
                    City = r.GetString("city"),
                    ZipCode = r.GetString("zip_code")
                };
            return null;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            const string sql = "SELECT * FROM users WHERE id = @id;";
            await using var c = new NpgsqlConnection(_connectionString);
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);

            await using var r = await cmd.ExecuteReaderAsync();
            return await r.ReadAsync()
                ? new User
                {
                    Id = id,
                    Name = r.GetString("name"),
                    Email = r.GetString("email"),
                    Password = r.GetString("password"),
                    Phone = r.GetString("phone"),
                    Address = r.GetString("address"),
                    City = r.GetString("city"),
                    ZipCode = r.GetString("zip_code")
                }
                : null;
        }

        public async Task UpdateUserAsync(User u)
        {
            const string sql = @"
        UPDATE users SET
            name      = @name,
            phone     = @phone,
            address   = @addr,
            city      = @city,
            zip_code  = @zip
        WHERE id = @id;";

            await using var c = new NpgsqlConnection(_connectionString);
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@name", u.Name);
            cmd.Parameters.AddWithValue("@phone", u.Phone ?? "");
            cmd.Parameters.AddWithValue("@addr", u.Address ?? "");
            cmd.Parameters.AddWithValue("@city", u.City ?? "");
            cmd.Parameters.AddWithValue("@zip", u.ZipCode ?? "");
            cmd.Parameters.AddWithValue("@id", u.Id);

            await cmd.ExecuteNonQueryAsync();
        }



    }





}

