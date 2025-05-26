using Microsoft.Extensions.Logging;

namespace Maui.PDFView.Helpers.DataSource;

public static class ElementExtensions
{
    internal static IMauiContext? FindMauiContext(this Element element, bool fallbackToAppMauiContext = false)
    {
        if (element is IElement { Handler.MauiContext: not null } fe)
        {
            return fe.Handler.MauiContext;
        }

        foreach (var parent in element.GetParentsPath())
        {
            if (parent is IElement { Handler.MauiContext: not null } parentView)
            {
                return parentView.Handler.MauiContext;
            }
        }

        return fallbackToAppMauiContext
            ? Application.Current?.FindMauiContext()
            : null;
    }
    
    internal static ILogger<T>? CreateLogger<T>(this IMauiContext context) =>
        context.Services.CreateLogger<T>();

    private static ILogger<T>? CreateLogger<T>(this IServiceProvider services) =>
        services.GetService<ILogger<T>>();

    private static IEnumerable<Element> GetParentsPath(this Element self)
    {
        Element current = self;
        while (!IsApplicationOrNull(current.RealParent))
        {
            current = current.RealParent;
            yield return current;
        }
    }

    private static bool IsApplicationOrNull(object? element) =>
        element == null || element is IApplication;
}