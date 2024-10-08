using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Configurations;

public class SmtpSettings
{
    [Required(AllowEmptyStrings = false)]
    public string Host { get; set; } = null!;

    public int Port { get; set; }

    [Required]
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Sender { get; set; } = null!;
}
