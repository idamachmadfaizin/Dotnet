namespace WebApi.App.Http.Endpoints.V2;

internal sealed class HelloWorldV2 : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("helloWorld");
        Version(2);
        AllowAnonymous();
        Description(x => x
            .Produces<string>()
            .ProducesProblemDetails(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync("Hello world v2", ct);
    }
}