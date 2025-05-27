namespace BlazorMarkedsplads.Models
{
    public class FilterOptions
    {
        public List<string> Brands { get; set; } = [];
        public List<string> Cpus { get; set; } = [];
        public List<string> Rams { get; set; } = [];
        public List<string> Storages { get; set; } = [];
        public List<string> ScreenSizes { get; set; } = [];
        public List<string> OSes { get; set; } = [];
        public List<string> Conditions { get; set; } = [];
    }
}
