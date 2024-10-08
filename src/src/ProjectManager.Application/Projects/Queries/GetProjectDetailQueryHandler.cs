using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Application.Contracts.Projects.Queries;
using ProjectManager.Data;
using ProjectManager.Data.Entities;

namespace ProjectManager.Application.Projects.Queries;

public class GetProjectDetailQueryHandler : IRequestHandler<GetProjectDetailQuery, ProjectDetailDto?>
{
    private readonly ApplicationDbContext _dbContext;

    public GetProjectDetailQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectDetailDto?> Handle(
        GetProjectDetailQuery request, CancellationToken cancellationToken)
    {
        var dbEntity = await _dbContext.Projects
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, cancellationToken: cancellationToken);

        if (dbEntity == null)
        {
            return null;
        }

        return new ProjectDetailDto(dbEntity.Id, dbEntity.Title, dbEntity.Description);
    }
}