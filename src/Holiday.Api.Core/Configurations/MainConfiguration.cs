namespace Holiday.Api.Core.Configurations;

// sealed car imuable finito
public sealed class MainConfiguration
{
    public static readonly string Section = "MainConfiguration";

    public DatabaseConfiguration DataBase { get; set; } = new();
}