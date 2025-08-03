namespace WebApi.Configurations;

public class Localization
{
    public string DefaultCulture { get; set; } = "en-US";
    public List<string> SupportedCultures { get; set; } = [];
}