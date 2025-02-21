
namespace ParseService.Services.ErrorsLogger
{
    internal class ErrorsLoggerService<T> : IErrorsLoggerService<T>
    {
        private readonly ILogger<T> _logger;

        public ErrorsLoggerService(ILogger<T> logger) => _logger = logger;

        public void ErrorSendLogToTelegram(string MessageError)
        {

            _logger.LogError(message: MessageError);

        }
    }
}
