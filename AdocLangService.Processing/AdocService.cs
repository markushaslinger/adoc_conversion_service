using CliWrap;

namespace AdocLangService.Processing;

public sealed class AdocService
{
    private readonly TempDir _tempDir;

    public AdocService(TempDir tempDir)
    {
        _tempDir = tempDir;
    }

    public async ValueTask<(string? ConvertedText, byte[]? ConvertedBytes)?>
        Convert(string adocContent, ConversionType conversionType)
    {
        _tempDir.Prepare();

        var (cmd, args) = GetCmdAndArgs(conversionType);
        var srcFilePath = await WriteSrcFile(adocContent);
        var (success, tgtFilePath) = await RunAdoc(cmd, args, srcFilePath);

        if (!success || tgtFilePath == null)
        {
            return null;
        }

        return conversionType == ConversionType.Pdf
            ? (null, await File.ReadAllBytesAsync(tgtFilePath))
            : (await File.ReadAllTextAsync(tgtFilePath), null);
    }

    private async ValueTask<(bool Success, string? TgtFilePath)>
        RunAdoc(string command, string arguments, string srcFilePath)
    {
        var srcFileName = Path.GetFileName(srcFilePath);
        arguments = $"{arguments} {srcFileName}";
        try
        {
            await Cli.Wrap(command)
                .WithWorkingDirectory(_tempDir.DirPath)
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .WithArguments(arguments)
                .ExecuteAsync();

            var tgtFile = Directory.EnumerateFiles(_tempDir.DirPath)
                .First(fileName => !Path.GetExtension(fileName).Contains("adoc"));
            return (true, tgtFile);
        }
        catch
        {
            // TODO log or something
            return (false, null);
        }
    }

    private async ValueTask<string> WriteSrcFile(string content)
    {
        const string FileName = "document.adoc";
        var path = Path.Combine(_tempDir.DirPath, FileName);

        await File.WriteAllTextAsync(path, content);

        return path;
    }

    private static (string Command, string Arguments) GetCmdAndArgs(ConversionType conversionType)
    {
        var command = conversionType switch
        {
            ConversionType.Html => "asciidoctor",
            ConversionType.Pdf => "asciidoctor-pdf",
            ConversionType.Presentation => "asciidoctor-revealjs",
            _ => throw new ArgumentException("unknown type", nameof(conversionType))
        };

        var arguments = "-r asciidoctor-mathematical -r asciidoctor-diagram";
        if (conversionType == ConversionType.Presentation)
        {
            arguments = $"-a revealjsdir=https://cdnjs.cloudflare.com/ajax/libs/reveal.js/3.9.2 {arguments}";
        }

        return (command, arguments);
    }
}