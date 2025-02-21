namespace ParseService.Services.ErrorsLogger
{
    internal interface IErrorsLoggerService<T>
    {
        void ErrorSendLogToTelegram(string MessageError);
    }
}