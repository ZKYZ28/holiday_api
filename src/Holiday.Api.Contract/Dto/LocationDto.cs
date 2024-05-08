namespace DefaultNamespace;

public class LocationDto
{
    
    public string? Id { get; set; }
    
    public string? Street { get; set; }
    
    public string? Number { get; set; }
    
    public string Locality { get; set; }
    
    public string PostalCode { get; set; }

    public string Country { get; set; }
}

public class LocationEditDto : LocationDto
{
    public string LocationId { get; set; }
}