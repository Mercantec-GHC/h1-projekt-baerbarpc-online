namespace BlazorMarkedsplads.Models
{
    /// <summary>
    /// Denne klasse indeholder præcis de felter, vi skal vise i "Aktive Annoncer"-tabellen.
    /// </summary>
    public class ListingCardModel
    {
        public int ListingId { get; set; }       // selve listingens PK
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty; // kan være fx "Brand Model, RAM etc."
        public decimal Price { get; set; }
        public DateTime CreatedUtc { get; set; }
        public int Views { get; set; }       // hvis du gemmer visninger (ellers dummy)
        public string Status { get; set; } = string.Empty; // fx "Aktiv" / "Reserveret"
    }
}
