namespace Blazor_Markedsplads.Models // Sørg for at bruge det korrekte namespace
{
    public class ListingImage
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
    }
}