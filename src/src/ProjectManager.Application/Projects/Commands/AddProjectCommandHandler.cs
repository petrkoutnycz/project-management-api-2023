using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Application.Contracts;
using ProjectManager.Application.Contracts.Projects.Commands;
using ProjectManager.Data;
using ProjectManager.Data.Entities;
using ProjectManager.Data.Interfaces;

namespace ProjectManager.Application.Projects.Commands;

public class AddProjectCommandHandler : IRequestHandler<AddProjectCommand, AddProjectCommandError?>
{
    private readonly AddProjectCommandValidator _validator;
    private readonly ApplicationDbContext _dbContext;

    public AddProjectCommandHandler(
        AddProjectCommandValidator validator,
        ApplicationDbContext dbContext)
    {
        _validator = validator;
        _dbContext = dbContext;
    }

    public async Task<AddProjectCommandError?> Handle(
        AddProjectCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult);
        }

        var newProject = new Project
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
        }.SetCreateBySystem(NodaTime.SystemClock.Instance.GetCurrentInstant());

        if (await _dbContext.Projects.AnyAsync(x => x.Title == newProject.Title,
                cancellationToken: cancellationToken))
        {
            return AddProjectCommandError.TitleIsNotUnique;
        }

        _dbContext.Add(newProject);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return null;
    }
}