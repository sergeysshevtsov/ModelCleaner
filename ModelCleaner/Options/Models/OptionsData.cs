namespace ModelCleaner.Options.Models;
public class OptionsData : BaseModel
{
    public string ElementType { get; set; } = string.Empty;
    public int ElementCount { get; set; } = 0;
    public List<ElementId> ElementIds { get; set; } = [];
}
