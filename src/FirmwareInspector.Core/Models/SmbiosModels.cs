using System.Collections.ObjectModel;
using FirmwareInspector.Core.Parsing;

namespace FirmwareInspector.Core.Models;

public sealed record SmbiosVersion(byte Major, byte Minor, byte DmiRevision)
{
    public override string ToString() => $"{Major}.{Minor}";
}

public sealed record SmbiosField(
    string Name,
    string Value,
    int Offset,
    int Length,
    string? Note = null)
{
    public int? BitIndex { get; init; }
    public int? BitValue { get; init; }
    public bool IsBitField => BitIndex.HasValue;
}

public sealed record SmbiosString(
    int Index,
    string Value,
    int Offset);

public sealed class SmbiosStructure
{
    private readonly List<SmbiosField> _fields = [];
    private readonly List<SmbiosString> _strings = [];

    public required int Index { get; init; }
    public required byte Type { get; init; }
    public required byte Length { get; init; }
    public required ushort Handle { get; init; }
    public required int TableOffset { get; init; }
    public required ReadOnlyMemory<byte> FormattedArea { get; init; }
    public ReadOnlyMemory<byte> StringArea { get; internal set; }
    public ReadOnlyMemory<byte> RawBytes { get; internal set; }

    public IReadOnlyList<SmbiosField> Fields => _fields;
    public ReadOnlyCollection<SmbiosString> Strings => _strings.AsReadOnly();
    public string TypeName => SmbiosTypeCatalog.GetName(Type);

    public string PrimaryText
    {
        get
        {
            string[] preferredFields =
            [
                "Product Name",
                "Firmware Version",
                "Device Locator",
                "Socket Designation",
                "Slot Designation",
                "Manufacturer",
                "Manufacturer Name",
                "Product",
                "Description",
                "Device Name",
                "Reference Designation",
                "Firmware Component Name",
                "Group Name"
            ];

            foreach (var fieldName in preferredFields)
            {
                var candidate = _fields.FirstOrDefault(
                    item => item.Name.Equals(fieldName, StringComparison.Ordinal) &&
                            !item.Value.StartsWith("(", StringComparison.Ordinal));

                if (candidate is not null)
                {
                    return candidate.Value;
                }
            }

            return TypeName;
        }
    }

    internal void AddField(SmbiosField field) => _fields.Add(field);

    internal void AddString(SmbiosString value) => _strings.Add(value);
}

public sealed record SmbiosParseResult(
    SmbiosVersion Version,
    uint DeclaredTableLength,
    ReadOnlyMemory<byte> RawData,
    ReadOnlyMemory<byte> StructureTable,
    IReadOnlyList<SmbiosStructure> Structures,
    IReadOnlyList<string> Warnings,
    string Source,
    DateTimeOffset CapturedAt)
{
    public int TableByteLength => StructureTable.Length;
}
