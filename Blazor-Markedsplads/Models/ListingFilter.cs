namespace BlazorMarkedsplads.Models
{
    public class ListingFilter
    {
        // multiselect
        public List<string> Brand { get; set; } = [];
        public List<string> Cpu { get; set; } = [];
        public List<string> Ram { get; set; } = [];
        public List<string> Storage { get; set; } = [];
        public List<string> ScreenSize { get; set; } = [];
        public List<string> OS { get; set; } = [];
        public List<string> Condition { get; set; } = [];

        // fritekst
        public string? ModelSearch { get; set; }

        // pris
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }

        // plads til senere kolonner
        public List<string> Usage { get; set; } = [];
        public string? Location { get; set; }
    }
}
