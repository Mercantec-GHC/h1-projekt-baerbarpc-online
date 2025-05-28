using BlazorMarkedplads.Models;
using BlazorMarkedsplads.Models;
using Npgsql;
using System.Threading.Tasks;

namespace BlazorMarkedsplads.Services
{
    public class ListingRepository : IListingRepository
    {
        private readonly string _cs;

        public ListingRepository(IConfiguration cfg)
        {
            _cs = cfg.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("ConnectionString mangler");
        }

        public async Task<int> InsertAsync(Listing l)
        {
            const string sql = @"
                INSERT INTO listings
                    (product_id, user_id, title, description, phone, location, created_utc)
                VALUES
                    (@pid, @uid, @title, @desc, @phone, @loc, @created)
                RETURNING id;";

            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@pid", l.ProductId);
            cmd.Parameters.AddWithValue("@uid", l.UserId);
            cmd.Parameters.AddWithValue("@title", l.Title);
            cmd.Parameters.AddWithValue("@desc", l.Description);
            cmd.Parameters.AddWithValue("@phone", l.Phone);
            cmd.Parameters.AddWithValue("@loc", l.Location);
            cmd.Parameters.AddWithValue("@created", l.CreatedUtc);

            var idObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(idObj);
        }
    }
}