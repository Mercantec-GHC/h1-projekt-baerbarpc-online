﻿namespace BlazorMarkedsplads.Models 
{
    public class ListingImage
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
    }
}