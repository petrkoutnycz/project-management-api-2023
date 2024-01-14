using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.Controllers.Models.Todos;

public class TodoCreateModel
{
    [Required(ErrorMessage = "{0} is required.", AllowEmptyStrings = false)]
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    [Required]
    public Guid StatusId { get; set; }

    [Required]
    public Guid ProjectId { get; set; }
}
