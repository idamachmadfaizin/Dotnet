using FastEndpoints.Swagger;
using NSwag.Generation.AspNetCore;

namespace Configurations;

public class Swagger
{
    public required IList<DocumentOption> DocumentOptions { get; init; } = [];
}

public class DocumentOption
{
    public int AutoTagPathSegmentIndex { get; set; } = 1;
    public AspNetCoreOpenApiDocumentGeneratorSettings DocumentSettings { get; set; } = new();
    public bool EnableGetRequestsWithBody { get; set; }
    public bool EnableJWTBearerAuth { get; set; } = true;
    public bool ExcludeNonFastEndpoints { get; set; }
    public bool FlattenSchema { get; set; }
    public int MaxEndpointVersion { get; set; }
    public int MinEndpointVersion { get; set; }
    public int ReleaseVersion { get; set; }
    public bool ShowDeprecatedOps { get; set; }
    public bool RemoveEmptyRequestSchema { get; set; }
    public bool ShortSchemaNames { get; set; }
    public TagCase TagCase { get; set; } = TagCase.TitleCase;
    public bool TagStripSymbols { get; set; } = false;
    public Dictionary<string, string>? TagDescriptions { get; set; }
    public bool UsePropertyNamingPolicy { get; set; } = true;
    public bool UseOneOfForPolymorphism { get; set; }
}