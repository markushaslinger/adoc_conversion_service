using AdocConversionService.Core.Adoc;

namespace AdocConversionService.Core;

public static class Setup
{
    public static void PerformServiceSetup(this IServiceCollection services)
    {
        EnsureTempDirLocation();

        services.AddScoped<AdocService>();
        
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