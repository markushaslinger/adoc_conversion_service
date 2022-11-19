namespace AdocConversionService.Core.Adoc;

public sealed class ConversionResult
{
    public required bool Success { get; init; }
    public required TimeSpan Duration { get; init; }
    public required byte[] Data { get; init; }
    public ImageData? ImageMetaData { get; init; }
    public string? ErrorMessage { get; init; }
    
    public sealed class ImageData
    {
        public required string Name { get; init; }
        public required int Width { get; init; }
        public required int Height { get; init; }
    }
}