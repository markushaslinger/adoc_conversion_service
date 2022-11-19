using System.Text;
using CliWrap;

namespace AdocConversionService.Core;

public static class Extensions
{
    public static string ToFullString(this Exception self, StringBuilder? consoleDump = null)
    {
        var msg = self.Message;

        void Append(string? s) => msg += $"{Environment.NewLine}{s}";
        
        if (!string.IsNullOrWhiteSpace(self.StackTrace))
        {
            Append(self.StackTrace);
        }

        if (self.InnerException != null)
        {
            Append(self.InnerException.ToFullString());
        }

        if (consoleDump != null)
        {
            Append(consoleDump.ToString());
        }

        return msg;
    }

    public static Command WithRedirectedOutputs(this Command self, StringBuilder sb)
    {
        return self
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(sb))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(sb));
    }
}