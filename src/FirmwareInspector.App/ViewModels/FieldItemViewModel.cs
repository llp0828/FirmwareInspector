using FirmwareInspector.Core.Models;

namespace FirmwareInspector.App.ViewModels;

public sealed class FieldItemViewModel
{
    public FieldItemViewModel(SmbiosField field)
    {
        Name = field.Name;
        Value = field.Value;
        OffsetText = $"0x{field.Offset:X2}";
        Note = field.Note ?? string.Empty;
        IsBitField = field.IsBitField;
        BitValue = field.BitValue ?? 0;
        DisplayName = field.BitIndex is { } bitIndex ? $"Bit {bitIndex}" : field.Name;
    }

    public string Name { get; }
    public string DisplayName { get; }
    public string Value { get; }
    public string OffsetText { get; }
    public string Note { get; }
    public bool HasNote => !string.IsNullOrWhiteSpace(Note);
    public bool IsBitField { get; }
    public int BitValue { get; }
}
