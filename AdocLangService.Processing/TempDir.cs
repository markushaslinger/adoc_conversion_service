namespace AdocLangService.Processing;

public sealed class TempDir : IDisposable
{
    public string DirPath { get; }

    public TempDir()
    {
        var name = Guid.NewGuid().ToString();
        if (char.IsDigit(name[0]))
        {
            name = name[1..];
        }

        var path = Path.Combine(Const.TempDirPath, name);
        DirPath = path;
    }

    public void Prepare()
    {
        Directory.CreateDirectory(DirPath);
    }

    public void Clear()
    {
        if (Directory.Exists(DirPath))
        {
            Directory.Delete(DirPath, true);
        }
        Prepare();
    }
    
    public void Dispose()
    {
        if (Directory.Exists(DirPath))
        {
            Directory.Delete(DirPath, true);
        }
    }
}