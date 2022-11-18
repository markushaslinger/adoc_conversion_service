namespace AdocLangService.Pages.Components;

public partial class Editor
{
    private void HandleContentChanged(string content)
    {
        _preview.Render(content);
    }
}