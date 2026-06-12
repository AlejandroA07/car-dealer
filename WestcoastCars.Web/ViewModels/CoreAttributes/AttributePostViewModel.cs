using System.Text.Json.Serialization;
using WestcoastCars.Web.ViewModels;

namespace WestcoastCars.Web.ViewModels.CoreAttributes;

public class AttributePostViewModel : BaseViewModel
{
    [JsonIgnore]
    public string AttributeType { get; set; } = string.Empty;

    [JsonIgnore]
    public string Title { get; set; } = string.Empty;

    [JsonIgnore]
    public string Subtitle { get; set; } = string.Empty;

    [JsonIgnore]
    public string Icon { get; set; } = string.Empty;

    [JsonIgnore]
    public string AddLabel { get; set; } = string.Empty;

    [JsonIgnore]
    public string ExistingLabel { get; set; } = string.Empty;

    [JsonIgnore]
    public IList<AttributeItemViewModel> Items { get; set; } = [];
}
