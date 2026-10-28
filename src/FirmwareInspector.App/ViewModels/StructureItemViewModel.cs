using FirmwareInspector.Core.Models;

namespace FirmwareInspector.App.ViewModels;

public sealed class StructureItemViewModel
{
    public StructureItemViewModel(SmbiosStructure structure)
    {
        Structure = structure;
        SearchHaystack = string.Join(
            '\n',
            new[]
            {
                structure.Type.ToString(),
                structure.Type.ToString("X2"),
                $"Type {structure.Type}",
                $"0x{structure.Type:X2}",
                structure.TypeName,
                structure.Handle.ToString("X4"),
                structure.PrimaryText
            }.Concat(structure.Fields.SelectMany(field => new[]
            {
                field.Name,
                field.Value,
                field.Note ?? string.Empty
            })).Concat(structure.Strings.Select(value => value.Value)));
    }

    public SmbiosStructure Structure { get; }

    public string TypeLabel => Structure.Type.ToString("X2");
    public string TypeNumberLabel => $"Type {Structure.Type}";
    public string TypeName => Structure.TypeName;
    public string Summary => Structure.PrimaryText;
    public string HandleText => $"0x{Structure.Handle:X4}";
    public string LengthText => Structure.Length.ToString();
    public string SearchHaystack { get; }
}
