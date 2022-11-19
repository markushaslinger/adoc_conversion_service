namespace AdocConversionService.Core;

public sealed class TempDir : IDisposable
{
    private readonly string _dirPath;
    private bool _prepared;

    public TempDir()
    {
        _dirPath = CreatePath();
        _prepared = false;
    }

    public string DirPath
    {
        get
        {
            if (!_prepared)
            {
                Prepare();
            }

            return _dirPath;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(DirPath))
        {
            // TODO re-enable!
            //Directory.Delete(DirPath, true);
        }
    }

    private static string CreatePath()
    {
        var name = Guid.NewGuid().ToString().Replace("-", string.Empty);
        if (char.IsDigit(name[0]))
        {
            name = $"t{name}";
        }

        return Path.Combine(Const.TempDirPath, name);
    }

    private void Prepare()
    {
        _prepared = true;
        Directory.CreateDirectory(DirPath);
    }

    public void Clear()
    {
        if (_prepared && Directory.Exists(DirPath))
        {
            Directory.Delete(DirPath, true);
        }

        Prepare();
    }
}