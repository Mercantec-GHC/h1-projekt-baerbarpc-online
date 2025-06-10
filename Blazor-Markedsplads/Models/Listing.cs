using System;
using System.Collections.Generic;
namespace BlazorMarkedsplads.Models
{
    public class Listing
    {
        public int Id { get; set; }


        // — BRUGERFELTER — forsøg.

        public int? UserId { get; set; } // Foreign key til User. Skal være nullable for at matche ON DELETE SET NULL.
        public User? Seller { get; set; } // Til at holde sælgerens info, når vi henter den.


        // — PRODUKTFELTER —
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Cpu { get; set; } = string.Empty;
        public string Gpu { get; set; } = string.Empty;
        public int Ram { get; set; }
        public int Storage { get; set; }
        public string OS { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ScreenSize { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;

        // — ANNONCEFELTER —
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // — NYT FELT TIL BILLEDER —
        public List<ListingImage> Images { get; set; } = new List<ListingImage>();

        public bool AcceptTerms { get; set; } = true;
    }
}
