namespace AdocConversionService.Core;

internal static class Const
{
    public static string TempDirPath = string.Empty;
    public static readonly TimeSpan MaxConversionDuration = TimeSpan.FromMinutes(1);
    public static readonly (int, int) RenderImageResolution = (1600,1200);
}