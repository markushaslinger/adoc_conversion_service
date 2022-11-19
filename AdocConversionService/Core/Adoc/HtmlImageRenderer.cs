using System.Text;
using System.Text.RegularExpressions;
using CliWrap;

namespace AdocConversionService.Core.Adoc;

public sealed class HtmlImageRenderer
{
    private const string TgtFileName = "image.jpeg";
    private const string NodeAppName = "app.js";
    private static readonly Regex sizeParseRegex = new("^(\\d+)x(\\d+)$", RegexOptions.Compiled);
    private readonly string _sourceFile;
    private readonly string _workDirPath;

    public HtmlImageRenderer(string workDirPath, string sourceFile)
    {
        _workDirPath = workDirPath;
        _sourceFile = sourceFile;
    }

    private string SourceFilePath => Path.Combine(_workDirPath, _sourceFile);

    public async ValueTask<ImageResult>
        ConvertHtmlToImage(CancellationToken token)
    {
        const string Command = "node";
        var (tgtFile, nodePath) = await CreateConversionFile(token);

        var sb = new StringBuilder();
        try
        {
            var cmd = Cli.Wrap(Command)
                .WithWorkingDirectory(_workDirPath)
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .WithRedirectedOutputs(sb)
                .WithArguments(nodePath);
            sb.Append(cmd);
            await cmd.ExecuteAsync(token);
        }
        catch (Exception ex)
        {
            return new(false, null, null, 0, 0, ex.ToFullString(sb));
        }

        var bytes = await File.ReadAllBytesAsync(tgtFile, token);
        var name = Path.GetFileName(tgtFile);
        var (sizeSuccess, width, height, sizeErrMsg) = await GetImageSize(tgtFile, token);
        
        return new(sizeSuccess, name, bytes, width, height, sizeErrMsg);
    }

    private async ValueTask<(bool Success, int Width, int Height, string? ErrorMsg)>
        GetImageSize(string imageFilePath, CancellationToken token)
    {
        const string Command = "identify";
        var arguments = $"-format '%wx%h' {imageFilePath}";

        var errorBuffer = new StringBuilder();
        var outBuffer = new StringBuilder();
        try
        {
            var cmd = Cli.Wrap(Command)
                .WithWorkingDirectory(_workDirPath)
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorBuffer))
                .WithArguments(arguments);
            await cmd.ExecuteAsync(token);
        }
        catch (Exception ex)
        {
            return (false, 0, 0, ex.ToFullString(errorBuffer));
        }

        var outStr = outBuffer.ToString();
        var match = sizeParseRegex.Match(outStr);

        if (!match.Success
            || match.Groups.Count < 3
            || !int.TryParse(match.Groups[1].Value, out var width)
            || !int.TryParse(match.Groups[2].Value, out var height))
        {
            return (false, 0, 0, $"Failed to determine image size in output: {outStr}");
        }

        return (true, width, height, null);
    }

    private async ValueTask<(string TgtFilePath, string NodeAppPath)>
        CreateConversionFile(CancellationToken token)
    {
        var tgtFilePath = Path.Combine(_workDirPath, TgtFileName);
        var code = $$"""
        const fs = require('fs');
        const nodeHtmlToImage = require('node-html-to-image');
    
        fs.readFile('{{SourceFilePath}}', 'utf8', function(err, data) {
            if (err) throw err;
            console.log('OK');
            nodeHtmlToImage({
                output: '{{tgtFilePath}}',
                html: data,
                type: 'jpeg',
                puppeteerArgs: 
                {
                    args: ['--no-sandbox']
                }
            })
            .then(() => console.log('The image was created successfully!'));
        });
        """;

        var nodeAppPath = Path.Combine(_workDirPath, NodeAppName);
        await File.WriteAllTextAsync(nodeAppPath, code, token);

        return (tgtFilePath, nodeAppPath);
    }
}