namespace WebApi.App.Http.Endpoints.V1.Article.Create;

internal sealed class Endpoint : Ep.Req<Request>.Res<Request>
{
    public override void Configure()
    {
        Post("article");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await Send.OkAsync(req, ct);
    }
}