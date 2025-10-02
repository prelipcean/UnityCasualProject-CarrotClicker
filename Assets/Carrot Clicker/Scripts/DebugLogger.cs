using UnityEngine;

/// <summary>
/// A conditional debug logging utility for Unity projects that automatically manages log output based on build configuration.
/// 
/// This class provides a simple logging utility that can be enabled or disabled based on the build configuration.
/// It uses preprocessor directives to conditionally compile the logging methods only in the editor or development builds.
/// This allows for cleaner production builds without debug logs, while still providing useful logging during development.
/// 
/// Key Features:
/// - Automatically excludes debug logs from release builds (performance optimization)
/// - Runtime toggle via EnableDebug flag during development
/// - Zero performance impact in production builds (code is stripped out)
/// - Drop-in replacement for Unity's Debug.Log methods
/// 
/// Build Configuration Behavior:
/// - UNITY_EDITOR: Logs are available when running in Unity Editor
/// - DEVELOPMENT_BUILD: Logs are available in development builds
/// - Release builds: All logging code is completely removed by the compiler
/// 
/// Usage Examples:
/// DebugLogger.Log("Player clicked carrot - score: " + score);
/// DebugLogger.LogWarning("Low memory detected: " + availableMemory + "MB");
/// DebugLogger.LogError("Failed to save game data: " + exception.Message);
/// 
/// To disable logs at runtime during development:
/// DebugLogger.EnableDebug = false;
/// </summary>
public static class DebugLogger
{
    /// <summary>
    /// Controls whether debug logs should be printed to the console.
    /// This flag is automatically set based on build configuration:
    /// - true in Unity Editor and Development builds
    /// - false in Release builds (but the entire variable is stripped out anyway)
    /// 
    /// You can modify this at runtime during development to temporarily disable logging.
    /// </summary>
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool EnableDebug = true;
#else
    public static bool EnableDebug = false;
#endif

    /// <summary>
    /// Logs a message to the Unity console (equivalent to Debug.Log).
    /// Only compiled and executed in Unity Editor or Development builds.
    /// In release builds, this method and its calls are completely removed by the compiler.
    /// </summary>
    /// <param name="message">The message to log to the console</param>
    public static void Log(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (EnableDebug)
            Debug.Log($"[DEBUG] {message}");
#endif
    }

    /// <summary>
    /// Logs a warning message to the Unity console (equivalent to Debug.LogWarning).
    /// Warnings appear with a yellow warning icon in the Unity console.
    /// Only compiled and executed in Unity Editor or Development builds.
    /// </summary>
    /// <param name="message">The warning message to log to the console</param>
    public static void LogWarning(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (EnableDebug)
            Debug.LogWarning($"[WARNING] {message}");
#endif
    }

    /// <summary>
    /// Logs an error message to the Unity console (equivalent to Debug.LogError).
    /// Errors appear with a red error icon in the Unity console and may pause the editor if "Error Pause" is enabled.
    /// Only compiled and executed in Unity Editor or Development builds.
    /// Note: For critical errors that should always be logged (even in release builds), use Unity's Debug.LogError directly.
    /// </summary>
    /// <param name="message">The error message to log to the console</param>
    public static void LogError(string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (EnableDebug)
            Debug.LogError($"[ERROR] {message}");
#endif
    }
}
