using log4net;
using log4net.Config;

namespace MainLogger
{
    class Log4net<T>
    {
        private delegate void LoggerList(string Message);
        private readonly ILog Log = LogManager.GetLogger(typeof(T));
        private readonly List<LoggerList> logger;
        private bool initialized;

        public Log4net()
        {
            logger = new(){
                Log.Info,
                Log.Warn,
                Log.Error,
                Log.Fatal,
            };
        }
        public void Config(bool Status)
        {
            initialized = Status;
            if (Status) XmlConfigurator.Configure(new FileInfo("Helpers/Logger/log4net.config"));
        }

        public void Message(string message, LogLevel LogLevel)
        {
            if (initialized) logger[(int)LogLevel](message);
        }
    }
}

public enum LogLevel
{
    Info = 0,
    Warn = 1,
    Error = 2,
    Fatal = 3,
}