namespace WebApi.App.Http.Endpoints.V1.Article.Create;

internal sealed class Request
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public string? Tags { get; set; }
}