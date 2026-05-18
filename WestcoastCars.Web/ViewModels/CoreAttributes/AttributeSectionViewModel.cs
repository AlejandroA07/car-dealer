namespace WestcoastCars.Web.ViewModels.CoreAttributes;

public class AttributeSectionViewModel
{
    public string AttributeType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string AddLabel { get; set; } = string.Empty;
    public string ExistingLabel { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public IList<AttributeItemViewModel> Items { get; set; } = new List<AttributeItemViewModel>();
}
