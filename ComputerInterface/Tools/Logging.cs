using BepInEx.Logging;

namespace ComputerInterface.Tools;

public static class Logging {
    private static void Log(LogLevel logLevel, object logContent) {
        // FIX: if any logging fires before Plugin.Awake runs, Plugin.Logger is null and this
        // threw NullReferenceException (silently swallowing the original error).
        if (Plugin.Logger == null)
            return;
        Plugin.Logger.Log(logLevel, logContent);
    }
    
    public static void Info(object logContent) =>
        Log(LogLevel.Info, logContent);
    
    public static void Warning(object logContent) =>
        Log(LogLevel.Warning, logContent);
    
    public static void Error(object logContent) =>
        Log(LogLevel.Error, logContent);
}