using BepInEx.Logging;

namespace Trashy
{
    public static class Log
    {
        private static readonly ManualLogSource s_logger;

        static Log()
        {
            s_logger = BepInEx.Logging.Logger.CreateLogSource(nameof(TrashyPlugin));
        }

        public static void Debug(object value)
        {
            s_logger.LogDebug(value);
        }

        public static void Debug<T>(object message)
        {
            s_logger.LogDebug($"[{typeof(T).Name}] {message}");
        }

        public static void Info(object value)
        {
            s_logger.LogInfo(value);
        }

        public static void Info<T>(object message)
        {
            s_logger.LogInfo($"[{typeof(T).Name}] {message}");
        }

        public static void Warn(object value)
        {
            s_logger.LogWarning(value);
        }

        public static void Warn<T>(object message)
        {
            s_logger.LogWarning($"[{typeof(T).Name}] {message}");
        }

        public static void Error(object value)
        {
            s_logger.LogError(value);
        }

        public static void Error<T>(object message)
        {
            s_logger.LogError($"[{typeof(T).Name}] {message}");
        }
    }
}
