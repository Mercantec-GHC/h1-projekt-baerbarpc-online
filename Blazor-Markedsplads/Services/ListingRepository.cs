using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Blazor_Markedsplads.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Blazor_Markedsplads.Services
{
    public class ListingRepository : IListingRepository
    {
        private readonly string _connectionString;

        public ListingRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("DefaultConnection mangler i appsettings.");
        }

        public async Task<int> InsertAsync(Listing listing)
        {
            const string sql = @"
                INSERT INTO listings
                  (brand, model, gpu, cpu, ram, storage, os, price, screen_size, condition,
                   title, description, phone, location, created_utc)
                VALUES
                  (@brand, @model, @gpu, @cpu, @ram, @storage, @os, @price, @screen_size, @condition,
                   @title, @description, @phone, @location, @created)
                RETURNING id;
            ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@brand", listing.Brand ?? string.Empty);
            cmd.Parameters.AddWithValue("@model", listing.Model ?? string.Empty);
            cmd.Parameters.AddWithValue("@gpu", listing.Gpu ?? string.Empty);
            cmd.Parameters.AddWithValue("@cpu", listing.Cpu ?? string.Empty);
            cmd.Parameters.AddWithValue("@ram", listing.Ram);
            cmd.Parameters.AddWithValue("@storage", listing.Storage);
            cmd.Parameters.AddWithValue("@os", listing.OS ?? string.Empty);
            cmd.Parameters.AddWithValue("@price", listing.Price ?? string.Empty);
            cmd.Parameters.AddWithValue("@screen_size", listing.ScreenSize ?? string.Empty);
            cmd.Parameters.AddWithValue("@condition", listing.Condition ?? string.Empty);

            cmd.Parameters.AddWithValue("@title", listing.Title ?? string.Empty);
            cmd.Parameters.AddWithValue("@description", listing.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("@phone", listing.Phone ?? string.Empty);
            cmd.Parameters.AddWithValue("@location", listing.Location ?? string.Empty);
            cmd.Parameters.AddWithValue("@created", listing.CreatedUtc);


            var idObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
        }

        public async Task<List<Listing>> GetPremiumAsync(int take = 4)
        {
            var list = new List<Listing>();

            const string sql = @"
                SELECT *
                  FROM listings
                 ORDER BY (regexp_replace(price, '[^0-9]', '', 'g'))::int DESC
                 LIMIT @take;
            ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@take", take);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Listing
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Brand = reader.GetString(reader.GetOrdinal("brand")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                    Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                    Ram = reader.GetInt32(reader.GetOrdinal("ram")),
                    Storage = reader.GetInt32(reader.GetOrdinal("storage")),
                    OS = reader.GetString(reader.GetOrdinal("os")),
                    Price = reader.GetString(reader.GetOrdinal("price")),
                    ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                    Condition = reader.GetString(reader.GetOrdinal("condition")),

                    Title = reader.GetString(reader.GetOrdinal("title")),
                    Description = reader.GetString(reader.GetOrdinal("description")),
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Location = reader.GetString(reader.GetOrdinal("location")),
                    CreatedUtc = reader.GetDateTime(reader.GetOrdinal("created_utc")),


                });
            }

            return list;
        }

        public async Task<List<Listing>> SearchAsync(string term)
        {
            var list = new List<Listing>();

            const string sql = @"
                SELECT *
                  FROM listings
                 WHERE brand ILIKE @t
                    OR model ILIKE @t
                    OR cpu ILIKE @t
                    OR gpu ILIKE @t
                    OR os ILIKE @t
                    OR title ILIKE @t
                    OR description ILIKE @t;
            ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@t", $"%{term}%");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Listing
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Brand = reader.GetString(reader.GetOrdinal("brand")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                    Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                    Ram = reader.GetInt32(reader.GetOrdinal("ram")),
                    Storage = reader.GetInt32(reader.GetOrdinal("storage")),
                    OS = reader.GetString(reader.GetOrdinal("os")),
                    Price = reader.GetString(reader.GetOrdinal("price")),
                    ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                    Condition = reader.GetString(reader.GetOrdinal("condition")),

                    Title = reader.GetString(reader.GetOrdinal("title")),
                    Description = reader.GetString(reader.GetOrdinal("description")),
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Location = reader.GetString(reader.GetOrdinal("location")),
                    CreatedUtc = reader.GetDateTime(reader.GetOrdinal("created_utc")),


                });
            }

            return list;
        }

        public async Task<List<Listing>> GetByUserIdAsync(int userId)
        {
            var list = new List<Listing>();

            const string sql = @"
                SELECT *
                  FROM listings
                 WHERE user_id = @uid
                 ORDER BY created_utc DESC;
            ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Listing
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Brand = reader.GetString(reader.GetOrdinal("brand")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                    Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                    Ram = reader.GetInt32(reader.GetOrdinal("ram")),
                    Storage = reader.GetInt32(reader.GetOrdinal("storage")),
                    OS = reader.GetString(reader.GetOrdinal("os")),
                    Price = reader.GetString(reader.GetOrdinal("price")),
                    ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                    Condition = reader.GetString(reader.GetOrdinal("condition")),

                    Title = reader.GetString(reader.GetOrdinal("title")),
                    Description = reader.GetString(reader.GetOrdinal("description")),
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Location = reader.GetString(reader.GetOrdinal("location")),
                    CreatedUtc = reader.GetDateTime(reader.GetOrdinal("created_utc")),

                });
            }

            return list;
        }

        public async Task<Listing?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT *
                  FROM listings
                 WHERE id = @id
                 LIMIT 1;
            ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

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
                Price = reader.GetString(reader.GetOrdinal("price")),
                ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                Condition = reader.GetString(reader.GetOrdinal("condition")),

                Title = reader.GetString(reader.GetOrdinal("title")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Phone = reader.GetString(reader.GetOrdinal("phone")),
                Location = reader.GetString(reader.GetOrdinal("location")),
                CreatedUtc = reader.GetDateTime(reader.GetOrdinal("created_utc")),
            };
        }
    }
}
