using FirmwareInspector.Core.Models;

namespace FirmwareInspector.App.ViewModels;

public sealed class StringItemViewModel
{
    public StringItemViewModel(SmbiosString value)
    {
        Index = value.Index;
        Value = value.Value;
        OffsetText = $"0x{value.Offset:X4}";
    }

    public int Index { get; }
    public string Value { get; }
    public string OffsetText { get; }
}
