using NodaTime.Text;
using ProjectManager.Api.Controllers.Models.Statuses;
using ProjectManager.Api.Controllers.Models.Todos;
using ProjectManager.Data.Entities;

namespace ProjectManager.Api.Controllers.Models.Projects;

public class ProjectDetailModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string CreatedAt { get; set; } = null!;

    public IEnumerable<TodoDetailModel> Todos { get; set; } = Enumerable.Empty<TodoDetailModel>();

    public IEnumerable<StatusDetailModel> Statuses { get; set; } = Enumerable.Empty<StatusDetailModel>();
}

public static class ProjectDetailModelExtensions
{
    public static ProjectDetailModel ToDetail(this Project source)
        => new()
        {
            Id = source.Id,
            Title = source.Title,
            Description = source.Description,
            CreatedAt = InstantPattern.ExtendedIso.Format(source.CreatedAt),
            Todos = source.Todos.Select(x => x.ToDetail()),
            Statuses = source.Statuses.Select(x => x.ToDetail()),
        };
}
