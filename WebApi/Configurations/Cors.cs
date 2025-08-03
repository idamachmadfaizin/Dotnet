namespace WebApi.Configurations;

public class Cors
{
    public List<string> AllowedOrigins { get; init; } = [];
    public int PreflightMaxAgeMinutes { get; init; } = 10;
}