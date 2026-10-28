using System.Text;

namespace FirmwareInspector.App.ViewModels;

public sealed class HexLineViewModel
{
    public HexLineViewModel(int offset, ReadOnlySpan<byte> bytes)
    {
        OffsetText = $"{offset:X4}";
        FirstHalf = FormatBytes(bytes[..Math.Min(8, bytes.Length)]);
        SecondHalf = bytes.Length > 8
            ? FormatBytes(bytes[8..Math.Min(16, bytes.Length)])
            : string.Empty;
        Ascii = FormatAscii(bytes);
    }

    public string OffsetText { get; }
    public string FirstHalf { get; }
    public string SecondHalf { get; }
    public string Ascii { get; }

    private static string FormatBytes(ReadOnlySpan<byte> bytes) =>
        string.Join(" ", bytes.ToArray().Select(value => value.ToString("X2")));

    private static string FormatAscii(ReadOnlySpan<byte> bytes)
    {
        var builder = new StringBuilder(bytes.Length);

        foreach (var value in bytes)
        {
            builder.Append(value is >= 32 and <= 126 ? (char)value : '.');
        }

        return builder.ToString();
    }
}
