namespace ParseService.Models;

public class AnnouncementItem
{
    public int AnnId { get; set; }
    public string AnnTitle { get; set; }
    public string AnnDesc { get; set; }
    public long CTime { get; set; }
    public string Language { get; set; }
    public string AnnUrl { get; set; }
}
