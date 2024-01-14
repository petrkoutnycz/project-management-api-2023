using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.Controllers.Models.Projects;

public class ProjectCreateModel
{
    [Required(ErrorMessage = "{0} is required.", AllowEmptyStrings = false)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }
}
