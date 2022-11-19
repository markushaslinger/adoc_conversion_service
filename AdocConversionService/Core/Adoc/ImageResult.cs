namespace AdocConversionService.Core.Adoc;

public record ImageResult(bool Success, string? ImageName, byte[]? ImageBytes, 
    int Width, int Height, string? ErrorMsg);