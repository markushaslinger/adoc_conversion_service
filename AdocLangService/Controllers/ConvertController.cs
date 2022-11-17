using System.Text;
using AdocLangService.Processing;
using Microsoft.AspNetCore.Mvc;

namespace AdocLangService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConvertController : ControllerBase
{
    private const string Html = "html";
    private const string Pdf = "pdf";
    private const string Presentation = "presentation";
    
    private readonly AdocService _service;

    public ConvertController(AdocService service)
    {
        _service = service;
    }

    [HttpPost]
    [Route(Html)]
    public async ValueTask<ActionResult<ConvertedFile>> ConvertHtml([FromBody] ToConvert content)
    {
        var (result, _) = await _service.Convert(content.Content, ConversionType.Html) ?? (null, null);
        if (result == null)
        {
            return BadRequest(CreateResult<string>(false, null));
        }

        return Ok(CreateResult(true, result));
    }
    
    [HttpPost]
    [Route(Pdf)]
    public async ValueTask<ActionResult<ConvertedFile>> ConvertPdf([FromBody] ToConvert content)
    {
        var (_, result) = await _service.Convert(content.Content, ConversionType.Pdf) ?? (null, null);
        if (result == null)
        {
            return BadRequest(CreateResult<byte[]>(false, null));
        }

        return Ok(CreateResult(true, result));
    }
    
    [HttpPost]
    [Route(Presentation)]
    public async ValueTask<ActionResult<ConvertedFile>> ConvertPresentation([FromBody] ToConvert content)
    {
        var (result, _) = await _service.Convert(content.Content, ConversionType.Presentation) ?? (null, null);
        if (result == null)
        {
            return BadRequest(CreateResult<string>(false, null));
        }

        return Ok(CreateResult(true, result));
    }

    private static ConvertedFile CreateResult<T>(bool success, T? convertedContent)
    {
        var type = typeof(T);
        if (!success || convertedContent == null)
        {
            return new(false, null, type.ToString());
        }
        
        byte[] bytes;
        if (type == typeof(string))
        {
            bytes = Encoding.Unicode.GetBytes(convertedContent as string ?? throw new InvalidOperationException());
        }
        else
        {
            bytes = convertedContent as byte[] ?? throw new InvalidOperationException();
        }

        var base64 = Convert.ToBase64String(bytes);

        return new(true, base64, type.ToString());
    }

    public record ToConvert(string Content);
    public record ConvertedFile(bool Success, string? ConvertedContent, string Type);
}