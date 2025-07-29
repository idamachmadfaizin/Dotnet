namespace WebApi.App.Http.Endpoints.V1;

internal sealed class HelloWorld : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("helloWorld");
        AllowAnonymous();
        Description(x => x
            .Produces<string>()
            .ProducesProblemDetails(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Logger.LogInformation("FE info logger");
        await Send.OkAsync("Hello world", ct);
    }
}