namespace BlazorMarkedplads.Models;

public class CreateListingViewModel
{
	// produkt-felter
	public string Brand { get; set; } = "";
	public string Model { get; set; } = "";
	public string GPU { get; set; } = "";
	public string CPU { get; set; } = "";
	public int Ram { get; set; }
	public int Storage { get; set; }
	public decimal Price { get; set; }

	// listing-felter
	public string Title { get; set; } = "";
	public string Description { get; set; } = "";
	public string Phone { get; set; } = "";
	public string Location { get; set; } = "";
}
