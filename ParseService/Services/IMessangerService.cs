using ParseService.Models;

namespace ParseService.Services
{
    public interface IMessangerService
    {
        Task SendToTelegram(AnnouncementItem ann);
    }
}
