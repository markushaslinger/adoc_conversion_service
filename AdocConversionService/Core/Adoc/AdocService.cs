using System.Diagnostics;
using System.Text;
using CliWrap;

namespace AdocConversionService.Core.Adoc;

public sealed class AdocService
{
    private readonly TempDir _tempDir;

    public AdocService(TempDir tempDir)
    {
        _tempDir = tempDir;
    }

    public async ValueTask<ConversionResult> Convert(string adocContent, ConversionType conversionType,
        CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        var (cmd, args) = GetCmdAndArgs(conversionType);
        var srcFilePath = await WriteSrcFile(adocContent);
        var (success, tgtFilePath, errorMsg)
            = await RunAdoc(cmd, args, srcFilePath, token);
        
        if (!success || tgtFilePath == null)
        {
            return new()
            {
                Success = false,
                Duration = sw.Elapsed,
                Data = Array.Empty<byte>(),
                ErrorMessage = errorMsg
            };
        }

        if (conversionType is ConversionType.Html or ConversionType.Pdf)
        {
            // TODO remove
            Console.WriteLine($"Adoc conversion took {sw.Elapsed}");
            return new()
            {
                Success = true,
                Duration = sw.Elapsed,
                Data = await File.ReadAllBytesAsync(tgtFilePath, token)
            };
        }

        var (renderSuccess, imageName, imageBytes, width, height, renderErrorMsg)
            = await RunImageCreation(tgtFilePath, token);
        // TODO remove
        Console.WriteLine($"Image creation took {sw.Elapsed}");
        return new()
        {
            Success = renderSuccess,
            Duration = sw.Elapsed,
            Data = imageBytes ?? Array.Empty<byte>(),
            ImageMetaData = new()
            {
                Height = height,
                Width = width,
                Name = imageName ?? string.Empty
            },
            ErrorMessage = renderErrorMsg
        };
    }

    private ValueTask<ImageResult> RunImageCreation(string htmlDocPath, CancellationToken token)
    {
        var renderer = new HtmlImageRenderer(_tempDir.DirPath, htmlDocPath);
        return renderer.ConvertHtmlToImage(token);
    }

    private async ValueTask<(bool Success, string? TgtFilePath, string? ErrorMessage)>
        RunAdoc(string command, string arguments, string srcFilePath, CancellationToken token)
    {
        var srcFileName = Path.GetFileName(srcFilePath);
        arguments = $"{arguments} {srcFileName}";

        var sb = new StringBuilder();
        try
        {
            var cmd = Cli.Wrap(command)
                .WithWorkingDirectory(_tempDir.DirPath)
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .WithRedirectedOutputs(sb)
                .WithArguments(arguments);
            sb.Append(cmd);
            await cmd.ExecuteAsync(token);

            var tgtFile = Directory.EnumerateFiles(_tempDir.DirPath)
                .First(fileName => !Path.GetExtension(fileName).Contains("adoc"));
            return (true, tgtFile, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.ToFullString(sb));
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