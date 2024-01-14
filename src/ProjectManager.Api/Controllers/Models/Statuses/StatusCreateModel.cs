using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.Controllers.Models.Statuses
{
    public class StatusCreateModel
    {
        [Required(ErrorMessage = "{0} is required.", AllowEmptyStrings = false)]
        [JsonProperty("title")]
        public string Title { get; set; } = null!;

        [Required]
        [JsonProperty("projectId")]
        public Guid ProjectId { get; set; }
    }
}
