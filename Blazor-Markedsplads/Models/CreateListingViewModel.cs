using System.ComponentModel.DataAnnotations;

namespace Blazor_Markedsplads.Models
{
    public class CreateListingViewModel
    {

        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? GPU { get; set; }
        public string? CPU { get; set; }
        public int RAM { get; set; }
        public int Storage { get; set; }
        public string? OS { get; set; }
        public decimal Price { get; set; }
        public string? ScreenSize { get; set; }
        public string? Condition { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public bool AcceptTerms { get; set; }
    }
}
