using Newtonsoft.Json;
using NodaTime;
using ProjectManager.Api.Controllers.Models.InnerModels;
using ProjectManager.Api.Controllers.Models.Todos;
using ProjectManager.Data.Entities;

namespace ProjectManager.Api.Controllers.Models.Statuses;

public class StatusDetailModel
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = null!;

    [JsonProperty("project")]
    public GenericInnerModel Project { get; set; } = null!;

    [JsonProperty("todos")]
    public IEnumerable<TodoDetailModel> Todos { get; set; } = [];

    [JsonProperty("isDone")]
    public bool IsDone { get; set; }

    [JsonProperty("createdAt")]
    public Instant CreatedAt { get; set; }
}

public class StatusFilter
{
    public Guid? ProjectId { get; set; }
}

public static class StatusDetailModelExtensions
{
    public static IQueryable<Status> ApplyFilter(this IQueryable<Status> query, StatusFilter? filter)
    {
        if (filter != null)
        {
            if (filter.ProjectId != null)
            {
                query = query.Where(x => x.ProjectId == filter.ProjectId);
            }
        }

        return query;
    }

    public static StatusDetailModel ToDetail(this Status source)
        => new()
        {
            Id = source.Id,
            Title = source.Title,
            Project = new GenericInnerModel() { Id = source.ProjectId, Name = source.Project.Title },
            Todos = source.Todos.Select(x => x.ToDetail()),
            IsDone = source.IsDone,
            CreatedAt = source.CreatedAt,
        };
}
