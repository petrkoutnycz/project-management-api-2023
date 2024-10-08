using FluentValidation;

namespace ProjectManager.Application.Contracts.Projects.Commands;

public class AddProjectCommandValidator : AbstractValidator<AddProjectCommand>
{
    public AddProjectCommandValidator()
    {
        RuleFor(x => x.Title).Length(0, 20);
    }
}