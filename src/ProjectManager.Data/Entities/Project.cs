using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Data.Entities;
public class Project : ITrackable
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<Todo> Todos { get; set; } = new HashSet<Todo>();

    public ICollection<Status> Statuses { get; set; } = new HashSet<Status>();

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
public static class ProjectExtensions
{
    public static IQueryable<Project> FilterDeleted(this IQueryable<Project> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}
