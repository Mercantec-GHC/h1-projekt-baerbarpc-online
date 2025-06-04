using System;

namespace Blazor_Markedsplads.Models
{
    public class Listing
    {
        public int Id { get; set; }

        // — Produkt-felter —
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Gpu { get; set; } = string.Empty;
        public string Cpu { get; set; } = string.Empty;
        public int Ram { get; set; }
        public int Storage { get; set; }
        public string OS { get; set; } = string.Empty;
        public string Price { get; set; } = "0";
        public string ScreenSize { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;

        // — Annonce-felter —
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    }
}
