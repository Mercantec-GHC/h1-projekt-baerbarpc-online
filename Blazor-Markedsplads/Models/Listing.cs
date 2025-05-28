namespace BlazorMarkedplads.Models;

public class Listing
{
    public int Id { get; set; }          // PK
    public int ProductId { get; set; }          // FK ? product_models.id
    public int UserId { get; set; }          // FK ? users.id  (valgfrit – kan stå som 0)
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
