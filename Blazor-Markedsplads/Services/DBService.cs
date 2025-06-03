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


        // --- PRODUCTMODELS --------------------------------------------

        public async Task<string> SetupProductModelsTable()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS product_models (
                    id           SERIAL PRIMARY KEY,
                    brand        VARCHAR(255),
                    model        VARCHAR(255),
                    gpu          VARCHAR(255),
                    cpu          VARCHAR(255),
                    ram          VARCHAR(255),
                    storage      VARCHAR(255),
                    os           VARCHAR(255),
                    price        VARCHAR(255),
                    screen_size  VARCHAR(255),
                    condition    VARCHAR(255)
                );";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();

            return "ProductModels table created successfully";
        }

        public async Task<List<ProductModel>> GetAllProductModelsAsync()
        {
            var list = new List<ProductModel>();

            try
            {
                const string sql = "SELECT * FROM product_models;";
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // bemærk lille l  ↓ ↓
                    list.Add(new ProductModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Brand = reader.GetString(reader.GetOrdinal("brand")),
                        Model = reader.GetString(reader.GetOrdinal("model")),
                        Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                        Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                        Ram = reader.GetString(reader.GetOrdinal("ram")),
                        Storage = reader.GetString(reader.GetOrdinal("storage")),
                        OS = reader.GetString(reader.GetOrdinal("os")),
                        Price = reader.GetString(reader.GetOrdinal("price")),
                        ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                        Condition = reader.GetString(reader.GetOrdinal("condition"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product models: {ex.Message}");
                // evt. _logger.LogError(ex, "...");
            }

            return list;        
        }
        // Henter de dyreste laptops (default 4)
        public async Task<List<ProductModel>> GetPremiumProductModelsAsync(int take = 4)
        {
            var list = new List<ProductModel>();

            const string sql = @"
        SELECT *
        FROM   product_models
        ORDER  BY (regexp_replace(price, '[^0-9]', '', 'g'))::int DESC   -- cast ”12 499 kr” → 12499
        LIMIT  @take;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("take", take);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ProductModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Brand = reader.GetString(reader.GetOrdinal("brand")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                    Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                    Ram = reader.GetString(reader.GetOrdinal("ram")),
                    Storage = reader.GetString(reader.GetOrdinal("storage")),
                    OS = reader.GetString(reader.GetOrdinal("os")),
                    Price = reader.GetString(reader.GetOrdinal("price")),
                    ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                    Condition = reader.GetString(reader.GetOrdinal("condition"))
                });
            }

            return list;
        }

        // Fritekstsøgning (brand, model, cpu, gpu, os …)
        public async Task<List<ProductModel>> SearchProductModelsAsync(string term)
        {
            var list = new List<ProductModel>();

            const string sql = @"
        SELECT *
        FROM   product_models
        WHERE  brand  ILIKE @t
           OR  model  ILIKE @t
           OR  cpu    ILIKE @t
           OR  gpu    ILIKE @t
           OR  os     ILIKE @t;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("t", $"%{term}%");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ProductModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Brand = reader.GetString(reader.GetOrdinal("brand")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                    Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                    Ram = reader.GetString(reader.GetOrdinal("ram")),
                    Storage = reader.GetString(reader.GetOrdinal("storage")),
                    OS = reader.GetString(reader.GetOrdinal("os")),
                    Price = reader.GetString(reader.GetOrdinal("price")),
                    ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                    Condition = reader.GetString(reader.GetOrdinal("condition"))
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

        // ------------------------------------------------------------
        // Dynamisk filtrering (bruges af ListingsPage)
        // ------------------------------------------------------------
        public async Task<List<ProductModel>> SearchProductModelsAsync(
                ListingFilter f, string? sort = "priceDesc")
        {
            var list = new List<ProductModel>();
            var where = new List<string>();
            var pars = new List<NpgsqlParameter>();

            void Add(string clause, string name, object value)
            {
                where.Add(clause);
                pars.Add(new NpgsqlParameter(name, value));
            }

            if (f.Brand.Any())
                Add("brand = ANY(@brands)", "brands", f.Brand);

            if (!string.IsNullOrWhiteSpace(f.ModelSearch))
                Add("model ILIKE @model", "model", $"%{f.ModelSearch}%");

            if (f.Cpu.Any())
                Add("cpu = ANY(@cpus)", "cpus", f.Cpu);

            if (f.Ram.Any())
                Add("ram = ANY(@rams)", "rams", f.Ram);

            if (f.Storage.Any())
                Add("storage = ANY(@stores)", "stores", f.Storage);

            if (f.ScreenSize.Any())
                Add("screen_size = ANY(@sizes)", "sizes", f.ScreenSize);

            if (f.OS.Any())
                Add("os = ANY(@oses)", "oses", f.OS);

            if (f.Condition.Any())
                Add("\"condition\" = ANY(@conds)", "conds", f.Condition);

            if (f.MinPrice is not null)
                Add("(regexp_replace(price,'[^0-9]','','g'))::int >= @min", "min", f.MinPrice);

            if (f.MaxPrice is not null)
                Add("(regexp_replace(price,'[^0-9]','','g'))::int <= @max", "max", f.MaxPrice);

            var sb = new StringBuilder("SELECT * FROM product_models");
            if (where.Count > 0) sb.Append(" WHERE ").Append(string.Join(" AND ", where));

            sb.Append(sort == "priceAsc"
                ? " ORDER BY (regexp_replace(price,'[^0-9]','','g'))::int ASC"
                : " ORDER BY (regexp_replace(price,'[^0-9]','','g'))::int DESC");

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sb.ToString(), conn);
            pars.ForEach(p => cmd.Parameters.Add(p));

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new ProductModel
                {
                    Id = r.GetInt32(r.GetOrdinal("id")),
                    Brand = r.GetString(r.GetOrdinal("brand")),
                    Model = r.GetString(r.GetOrdinal("model")),
                    Gpu = r.GetString(r.GetOrdinal("gpu")),
                    Cpu = r.GetString(r.GetOrdinal("cpu")),
                    Ram = r.GetString(r.GetOrdinal("ram")),
                    Storage = r.GetString(r.GetOrdinal("storage")),
                    OS = r.GetString(r.GetOrdinal("os")),
                    Price = r.GetString(r.GetOrdinal("price")),
                    ScreenSize = r.GetString(r.GetOrdinal("screen_size")),
                    Condition = r.GetString(r.GetOrdinal("condition"))
                });
            }
            return list;
        }


        public async Task SetupListingsTableAsync()
        {
            const string sql = @"
        CREATE TABLE IF NOT EXISTS listings(
            id           SERIAL PRIMARY KEY,
            product_id   INT NOT NULL REFERENCES product_models(id) ON DELETE CASCADE,
            user_id      INT NOT NULL REFERENCES users(id)          ON DELETE CASCADE,
            title        TEXT        NOT NULL,
            description  TEXT        NOT NULL,
            phone        TEXT        NOT NULL,
            location     TEXT        NOT NULL,
            created_utc  TIMESTAMPTZ NOT NULL DEFAULT now()
        );";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertProductModelAsync(ProductModel p)
        {
            const string sql = @"
        INSERT INTO product_models
            (brand, model, gpu, cpu, ram, storage, os, price, screen_size, condition)
        VALUES
            (@brand, @model, @gpu, @cpu, @ram, @storage, @os, @price, @screen_size, @condition)
        RETURNING id;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@brand", p.Brand);
            cmd.Parameters.AddWithValue("@model", p.Model);
            cmd.Parameters.AddWithValue("@gpu", p.Gpu);
            cmd.Parameters.AddWithValue("@cpu", p.Cpu);
            cmd.Parameters.AddWithValue("@ram", p.Ram);
            cmd.Parameters.AddWithValue("@storage", p.Storage);
            cmd.Parameters.AddWithValue("@os", p.OS);
            cmd.Parameters.AddWithValue("@price", p.Price);
            cmd.Parameters.AddWithValue("@screen_size", p.ScreenSize);
            cmd.Parameters.AddWithValue("@condition", p.Condition);

            var idObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
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

