namespace ParseService.Models;

public class AnnouncementsData
{
    public int TotalNum { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPage { get; set; }
    public AnnouncementItem[] Items { get; set; }
}
