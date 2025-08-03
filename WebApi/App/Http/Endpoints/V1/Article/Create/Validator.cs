using FluentValidation;

namespace WebApi.App.Http.Endpoints.V1.Article.Create;

internal sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty();
    }
}