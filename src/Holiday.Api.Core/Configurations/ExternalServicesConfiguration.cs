namespace Holiday.Api.Core.Configurations;

public class ExternalServicesConfiguration
{
    public static readonly string Section = "ExternalServices";

    public static readonly string GoogleSubSection = "Google";
    
    public GoogleConfiguration Google { get; set; } = new();
}