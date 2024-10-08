using MediatR;

namespace ProjectManager.Application.Contracts.Projects.Queries;

public record GetProjectDetailQuery(Guid ProjectId) : IRequest<ProjectDetailDto?>;

public record ProjectDetailDto(Guid Id, string Title, string? Description);