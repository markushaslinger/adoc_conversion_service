using Microsoft.Extensions.DependencyInjection;

namespace AdocLangService.Processing;

public static class Setup
{
    public static void PerformLangServiceSetup(this IServiceCollection services)
    {
        EnsureTempDirLocation();

        services.AddTransient<AdocService>();
        services.AddTransient<TempDir>();
    }

    private static void EnsureTempDirLocation()
    {
        var currDir = Directory.GetCurrentDirectory();
        var path = Path.Combine(currDir, "temp-files");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Const.TempDirPath = path;
    }
}

internal static class Const
{
    public static string TempDirPath = string.Empty;
}