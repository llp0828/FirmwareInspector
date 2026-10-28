using System.Buffers.Binary;
using System.Text;
using FirmwareInspector.Core.Models;

namespace FirmwareInspector.Core.Parsing;

public static class SmbiosParser
{
    private static readonly UTF8Encoding Utf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: false);

    public static SmbiosParseResult Parse(
        ReadOnlyMemory<byte> rawData,
        string source = "GetSystemFirmwareTable('RSMB')",
        DateTimeOffset? capturedAt = null)
    {
        if (rawData.IsEmpty)
        {
            throw new ArgumentException("SMBIOS 数据为空。", nameof(rawData));
        }

        var warnings = new List<string>();
        var raw = rawData.Span;
        var tableOffset = 0;
        var declaredLength = (uint)raw.Length;
        var version = new SmbiosVersion(0, 0, 0);

        if (TryReadRawSmbiosHeader(raw, out var header, out var headerLength))
        {
            version = new SmbiosVersion(header.Major, header.Minor, header.DmiRevision);
            tableOffset = headerLength;
            declaredLength = header.TableLength;

            var availableLength = raw.Length - tableOffset;
            if (declaredLength > availableLength)
            {
                warnings.Add(
                    $"RawSMBIOSData 声明表长度 {declaredLength} 字节，实际仅有 {availableLength} 字节。");
                declaredLength = (uint)availableLength;
            }
        }
        else
        {
            warnings.Add("未检测到 RawSMBIOSData 头，按纯结构表解析。");
        }

        var tableEnd = checked(tableOffset + (int)declaredLength);
        var table = rawData[tableOffset..tableEnd];
        var structures = new List<SmbiosStructure>();
        var position = 0;
        var sawEndOfTable = false;

        while (position + 4 <= table.Length)
        {
            var type = table.Span[position];
            var length = table.Span[position + 1];

            if (length < 4)
            {
                warnings.Add(
                    $"偏移 0x{position:X4} 的 Type {type} 结构长度 {length} 小于规范最小值 4，已停止解析。");
                break;
            }

            if (position + length > table.Length)
            {
                warnings.Add(
                    $"偏移 0x{position:X4} 的 Type {type} 结构越过表尾，已停止解析。");
                break;
            }

            var handle = BinaryPrimitives.ReadUInt16LittleEndian(
                table.Span.Slice(position + 2, 2));
            var structure = new SmbiosStructure
            {
                Index = structures.Count,
                Type = type,
                Length = length,
                Handle = handle,
                TableOffset = position,
                FormattedArea = table.Slice(position, length)
            };

            var stringStart = position + length;
            var nextPosition = ParseStringArea(
                table.Span,
                stringStart,
                structure,
                warnings);

            structure.StringArea = table[stringStart..nextPosition];
            structure.RawBytes = table[position..nextPosition];
            SmbiosFieldParser.Populate(structure);
            structures.Add(structure);

            if (type == 127)
            {
                sawEndOfTable = true;
                position = nextPosition;
                break;
            }

            position = nextPosition;
        }

        if (!sawEndOfTable)
        {
            warnings.Add("未遇到 End-of-Table (Type 127) 结构。");
        }

        if (position < table.Length)
        {
            var trailing = table.Span[position..];
            if (trailing.IndexOfAnyExcept((byte)0) >= 0)
            {
                warnings.Add($"End-of-Table 后仍有 {trailing.Length} 字节非零数据。");
            }
        }

        return new SmbiosParseResult(
            version,
            declaredLength,
            rawData,
            table,
            structures,
            warnings,
            source,
            capturedAt ?? DateTimeOffset.Now);
    }

    private static bool TryReadRawSmbiosHeader(
        ReadOnlySpan<byte> raw,
        out RawSmbiosHeader header,
        out int headerLength)
    {
        header = default;
        headerLength = 0;

        if (raw.Length < 8)
        {
            return false;
        }

        var major = raw[1];
        var minor = raw[2];
        var tableLength = BinaryPrimitives.ReadUInt32LittleEndian(raw.Slice(4, 4));

        if (raw[0] > 1 ||
            major is < 2 or > 4 ||
            minor > 99 ||
            tableLength < 4 ||
            tableLength > raw.Length - 8)
        {
            return false;
        }

        header = new RawSmbiosHeader(
            raw[0],
            major,
            minor,
            raw[3],
            tableLength);
        headerLength = 8;
        return true;
    }

    private static int ParseStringArea(
        ReadOnlySpan<byte> table,
        int start,
        SmbiosStructure structure,
        List<string> warnings)
    {
        var position = start;
        var index = 1;

        while (position < table.Length)
        {
            if (position + 1 < table.Length &&
                table[position] == 0 &&
                table[position + 1] == 0)
            {
                return position + 2;
            }

            if (table[position] == 0)
            {
                warnings.Add($"偏移 0x{start:X4} 的字符串集以单个 NUL 结束。");
                return position + 1;
            }

            var stringStart = position;
            while (position < table.Length && table[position] != 0)
            {
                position++;
            }

            if (position >= table.Length)
            {
                warnings.Add(
                    $"偏移 0x{start:X4} 的字符串集缺少双 NUL 终止符。");
                return table.Length;
            }

            structure.AddString(new SmbiosString(
                index++,
                Utf8.GetString(table[stringStart..position]),
                stringStart));
            position++;

            if (position < table.Length && table[position] == 0)
            {
                return position + 1;
            }
        }

        warnings.Add($"偏移 0x{start:X4} 的字符串集在表尾前未终止。");
        return table.Length;
    }

    private readonly record struct RawSmbiosHeader(
        byte Used20CallingMethod,
        byte Major,
        byte Minor,
        byte DmiRevision,
        uint TableLength);
}
