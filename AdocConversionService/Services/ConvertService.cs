using AdocConversionService.Core;
using AdocConversionService.Core.Adoc;
using Google.Protobuf;
using Grpc.Core;

namespace AdocConversionService.Services;

public sealed class ConvertService : AdocConvert.AdocConvertBase
{
    private readonly AdocService _adocService;

    public ConvertService(AdocService adocService)
    {
        _adocService = adocService;
    }

    public override async Task<ConvertResponse> Convert(ConvertRequest request, ServerCallContext context)
    {
        var tokenSrc = CreateTokenSource(TimeSpan.FromSeconds(10));
        var type = Map(request.Type);
        var result = await _adocService.Convert(request.AdocContent, type, tokenSrc.Token);

        return new()
        {
            Success = result.Success,
            Image = type == ConversionType.Presentation
                ? new Image
                {
                    Data = ByteString.CopyFrom(result.Data),
                    Name = result.ImageMetaData?.Name ?? string.Empty,
                    Height = (uint)(result.ImageMetaData?.Height ?? 0),
                    Width = (uint)(result.ImageMetaData?.Width ?? 0)
                }
                : null,
            Type = Map(type),
            DurationMilliseconds = (uint)result.Duration.TotalMilliseconds,
            ErrorMessage = result.ErrorMessage ?? string.Empty,
            Document = type == ConversionType.Presentation 
                ? ByteString.Empty 
                : ByteString.CopyFrom(result.Data)
        };
    }

    private static CancellationTokenSource CreateTokenSource(TimeSpan? timeOut = null)
    {
        timeOut ??= TimeSpan.FromSeconds(30);
        timeOut = timeOut > Const.MaxConversionDuration ? Const.MaxConversionDuration : timeOut;
        
        return new(timeOut.Value);
    }

    private static ConversionType Map(ConvertRequest.Types.RequestConversionType requestedType)
    {
        return requestedType switch
        {
            ConvertRequest.Types.RequestConversionType.Html => ConversionType.Html,
            ConvertRequest.Types.RequestConversionType.Pdf => ConversionType.Pdf,
            ConvertRequest.Types.RequestConversionType.Presentation => ConversionType.Presentation,
            _ => throw new ArgumentException("Unknown type", nameof(requestedType))
        };
    }

    private static ConvertResponse.Types.ResultType Map(ConversionType type)
    {
        return type switch
        {
            ConversionType.Html => ConvertResponse.Types.ResultType.Html,
            ConversionType.Pdf => ConvertResponse.Types.ResultType.Pdf,
            ConversionType.Presentation => ConvertResponse.Types.ResultType.Image,
            _ => throw new ArgumentException("Unknown type", nameof(type))
        };
    }
}