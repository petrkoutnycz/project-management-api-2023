using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Data.Entities;

[Table(nameof(Status))]
public class Status : ITrackable
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<Todo> Todos { get; set; } = new HashSet<Todo>();

    public bool IsDone { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
public static class StatusExtensions
{
    public static IQueryable<Status> FilterDeleted(this IQueryable<Status> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}
