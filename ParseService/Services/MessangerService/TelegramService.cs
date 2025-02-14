using Microsoft.Extensions.Options;
using ParseService.Models;
using ParseService.Options;
using Telegram.Bot;

namespace ParseService.Services.MessangerService
{
    public class TelegramService : IMessangerService
    {
        private readonly MainOptions _mainOptions;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(IOptions<MainOptions> mainOptions,
            ILogger<TelegramService> logger
            )
        {
            _mainOptions = mainOptions.Value;
            _logger = logger;
        }

        public async Task SendToTelegram(AnnouncementItem ann)
        {
            var botClient = new TelegramBotClient(_mainOptions.TELEGRAM_TOKEN);
            string message = $"📢 {ann.AnnTitle}\n📝 {ann.AnnDesc}\n🔗 {ann.AnnUrl}\n🕒";
            try
            {
                await botClient.SendTextMessageAsync(_mainOptions.CHAT_ID, message);
            }
            catch
            {
                _logger.LogError("Ошибка при отправке сообщения в телеграм");
            }
          
            
        }
    }
}
