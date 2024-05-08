namespace Holiday.Api.Repository.Models;

public class Location
{
    public Guid Id { get; set; }
    
    public string? Street { get; set; }
    
    public string? Number { get; set; }
    
    public string Locality { get; set; }
    
    public string PostalCode { get; set; }

    public string Country { get; set; }
    
    
    public string GetFormattedAddress()
    {
        return $"{Street} {Number}, {PostalCode} {Locality}, {Country}";
    }
}