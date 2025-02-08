using Microsoft.Extensions.Options;
using ParseService.Models;
using ParseService.Options;
using Telegram.Bot;

namespace ParseService.Services
{
    public class TelegramService : IMessangerService
    {
        private readonly MainOptions _mainOptions;

        public TelegramService(IOptions<MainOptions> mainOptions)
        {
            _mainOptions = mainOptions.Value;
        }

        public async Task SendToTelegram(AnnouncementItem ann)
        {
            var botClient = new TelegramBotClient(_mainOptions.TelegramToken);
            string message = $"📢 {ann.AnnTitle}\n📝 {ann.AnnDesc}\n🔗 {ann.AnnUrl}\n🕒";
            await botClient.SendTextMessageAsync(_mainOptions.ChatId, message);
        }
    }
}
