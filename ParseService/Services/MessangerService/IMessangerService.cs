using ParseService.Models;

namespace ParseService.Services.MessangerService
{
    public interface IMessangerService
    {
        Task SendToTelegram(AnnouncementItem ann);
    }
}
