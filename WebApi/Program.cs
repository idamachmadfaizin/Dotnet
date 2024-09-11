using FastEndpoints.Security;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthenticationJwtBearer(o => o.SigningKey = builder.Configuration["Auth:SigningKey"])
    .AddAuthorization()
    .AddFastEndpoints()
    .AddResponseCaching();

if (!builder.Environment.IsProduction())
{
    builder.Services.SwaggerDocument(o =>
    {
        o.DocumentSettings = settings =>
        {
            settings.DocumentName = "v0";
            settings.Version = "0.0.0";
        };
        o.FlattenSchema = true;
    });
}

var app = builder.Build();
app.UseAuthentication()
    .UseAuthorization()
    .UseDefaultExceptionHandler()
    .UseFastEndpoints(config => config.Errors.UseProblemDetails());

if (!app.Environment.IsProduction())
{
    app.UseSwaggerGen(uiConfig: settings => settings.DeActivateTryItOut());
}

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.Run();
