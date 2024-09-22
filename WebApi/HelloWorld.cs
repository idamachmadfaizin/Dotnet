sealed class HelloWorld : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/HelloWorld");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync("Hello world", ct);
    }
}