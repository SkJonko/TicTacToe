namespace TicToe.Infinite.Utilities;

/// <summary>
/// Service helper to discover services from the DI container.
/// </summary>
public class ServiceHelper
{

    /// <summary>
    /// Get the Services.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetService<T>() =>
        Current.GetService<T>()!;

    private static IServiceProvider Current =>
        Application.Current?.Handler?.MauiContext?.Services!;
}
