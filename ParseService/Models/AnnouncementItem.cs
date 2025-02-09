using System.ComponentModel.DataAnnotations;

namespace ParseService.Models;

public class AnnouncementItem
{
    [Key]
    public int AnnId { get; set; }
    [Required]
    public string AnnTitle { get; set; }
    [Required]
    public string AnnDesc { get; set; }
    public long CTime { get; set; }
    public string Language { get; set; }
    [Required]
    public string AnnUrl { get; set; }
}
