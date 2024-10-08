using MediatR;

namespace ProjectManager.Application.Contracts.Projects.Commands;

public record AddProjectCommand(string Title, string Description)
    : IRequest<AddProjectCommandError?>;
    // : IRequest<AddProjectCommandErrorRecord>;

public enum AddProjectCommandError
{
    TitleIsNotUnique
}

public abstract record AddProjectCommandErrorRecord;

public sealed record ProjectTitleNotUniqueError() : AddProjectCommandErrorRecord;