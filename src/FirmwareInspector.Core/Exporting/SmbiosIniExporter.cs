using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FirmwareInspector.Core.Models;

namespace FirmwareInspector.Core.Exporting;

public static class SmbiosIniExporter
{
    private static readonly IReadOnlyDictionary<string, string> FieldKeyAliases =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["结构类型"] = "StructureType",
            ["结构长度"] = "StructureLength",
            ["句柄"] = "HandleValue",
            ["状态"] = "Status",
            ["说明"] = "Description"
        };

    public static string Build(
        SmbiosParseResult result,
        IReadOnlyCollection<SmbiosStructure>? structures = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        var exportedStructures = structures ?? result.Structures;
        var builder = new StringBuilder();

        builder.AppendLine("; Firmware Inspector SMBIOS export");
        builder.AppendLine("; Source: GetSystemFirmwareTable('RSMB')");
        builder.AppendLine("; Encoding: UTF-16 LE with BOM (AutoIt IniRead compatible)");
        builder.AppendLine("; Escapes: \\\\, \\r, \\n, \\t, \\uXXXX");
        builder.AppendLine("; Raw.NNNN values contain 16 bytes per line.");
        builder.AppendLine();

        builder.AppendLine("[SMBIOS]");
        AppendValue(builder, "Source", result.Source);
        AppendValue(builder, "CapturedAt", result.CapturedAt.ToString("O", CultureInfo.InvariantCulture));
        AppendValue(builder, "Version", result.Version.ToString());
        AppendValue(builder, "DmiRevision", $"0x{result.Version.DmiRevision:X2}");
        AppendValue(builder, "DeclaredTableLength", result.DeclaredTableLength.ToString(CultureInfo.InvariantCulture));
        AppendValue(builder, "ParsedTableLength", result.StructureTable.Length.ToString(CultureInfo.InvariantCulture));
        AppendValue(builder, "StructureTableSha256", Convert.ToHexString(SHA256.HashData(result.StructureTable.Span)));
        AppendValue(builder, "StructureCount", result.Structures.Count.ToString(CultureInfo.InvariantCulture));
        AppendValue(builder, "ExportedStructureCount", exportedStructures.Count.ToString(CultureInfo.InvariantCulture));
        AppendValue(builder, "WarningCount", result.Warnings.Count.ToString(CultureInfo.InvariantCulture));

        for (var index = 0; index < result.Warnings.Count; index++)
        {
            AppendValue(builder, $"Warning.{index + 1:0000}", result.Warnings[index]);
        }

        builder.AppendLine();
        builder.AppendLine("[Index]");
        AppendValue(builder, "Count", exportedStructures.Count.ToString(CultureInfo.InvariantCulture));
        foreach (var structure in exportedStructures)
        {
            AppendValue(
                builder,
                $"{structure.Index:0000}",
                GetSectionName(structure));
        }

        foreach (var structure in exportedStructures)
        {
            builder.AppendLine();
            AppendStructure(builder, structure);
        }

        return builder.ToString();
    }

    public static void Write(
        string path,
        SmbiosParseResult result,
        IReadOnlyCollection<SmbiosStructure>? structures = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(
            fullPath,
            Build(result, structures),
            new UnicodeEncoding(bigEndian: false, byteOrderMark: true));
    }

    private static void AppendStructure(StringBuilder builder, SmbiosStructure structure)
    {
        builder.Append('[').Append(GetSectionName(structure)).AppendLine("]");

        AppendValue(builder, "Section", GetSectionName(structure));
        AppendValue(builder, "Index", structure.Index.ToString(CultureInfo.InvariantCulture));
        AppendValue(builder, "Type", structure.Type.ToString(CultureInfo.InvariantCulture));
        AppendValue(builder, "TypeHex", $"0x{structure.Type:X2}");
        AppendValue(builder, "TypeName", structure.TypeName);
        AppendValue(builder, "TableOffset", $"0x{structure.TableOffset:X4}");
        AppendValue(builder, "Length", structure.Length.ToString(CultureInfo.InvariantCulture));
        AppendValue(builder, "Handle", $"0x{structure.Handle:X4}");
        AppendValue(builder, "StringCount", structure.Strings.Count.ToString(CultureInfo.InvariantCulture));
        AppendValue(builder, "RawHex", Convert.ToHexString(structure.RawBytes.Span));
        AppendValue(builder, "FieldCount", structure.Fields.Count.ToString(CultureInfo.InvariantCulture));

        var rawBytes = structure.RawBytes.Span;
        for (var offset = 0; offset < rawBytes.Length; offset += 16)
        {
            var lineLength = Math.Min(16, rawBytes.Length - offset);
            AppendValue(
                builder,
                $"Raw.{offset:0000}",
                FormatHexLine(rawBytes.Slice(offset, lineLength)));
        }

        for (var index = 0; index < structure.Strings.Count; index++)
        {
            var item = structure.Strings[index];
            var prefix = $"String.{index + 1:0000}";
            AppendValue(builder, $"{prefix}.Index", item.Index.ToString(CultureInfo.InvariantCulture));
            AppendValue(builder, $"{prefix}.Offset", $"0x{item.Offset:X4}");
            AppendValue(builder, $"{prefix}.Value", item.Value);
            AppendValue(builder, $"String{item.Index}", item.Value);
        }

        var fieldKeys = BuildFieldKeys(structure);
        builder.AppendLine("; Stable field keys for direct INI lookup");
        foreach (var item in fieldKeys)
        {
            AppendFriendlyField(builder, item.Field, item.Key);
        }

        builder.AppendLine("; Detailed field map");
        for (var index = 0; index < structure.Fields.Count; index++)
        {
            var field = structure.Fields[index];
            var prefix = $"Field.{index + 1:0000}";
            var friendlyKey = fieldKeys[index].Key;
            AppendValue(builder, $"{prefix}.Name", field.Name);
            AppendValue(builder, $"{prefix}.Key", friendlyKey);
            AppendValue(builder, $"{prefix}.Offset", $"0x{field.Offset:X2}");
            AppendValue(builder, $"{prefix}.Length", field.Length.ToString(CultureInfo.InvariantCulture));
            AppendValue(builder, $"{prefix}.Value", field.Value);

            if (!string.IsNullOrWhiteSpace(field.Note))
            {
                AppendValue(builder, $"{prefix}.Note", field.Note);
            }

            if (field.BitIndex is { } bitIndex)
            {
                AppendValue(builder, $"{prefix}.BitIndex", bitIndex.ToString(CultureInfo.InvariantCulture));
                AppendValue(builder, $"{prefix}.BitValue", field.BitValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                AppendValue(builder, $"{prefix}.State", field.BitValue == 1 ? "Yes" : "No");
            }
        }
    }

    private static IReadOnlyList<(SmbiosField Field, string Key)> BuildFieldKeys(
        SmbiosStructure structure)
    {
        var usedKeys = new HashSet<string>(
            [
                "Section",
                "Index",
                "Type",
                "TypeHex",
                "TypeName",
                "TableOffset",
                "Length",
                "Handle",
                "StringCount",
                "RawHex",
                "FieldCount"
            ],
            StringComparer.OrdinalIgnoreCase);
        var result = new List<(SmbiosField Field, string Key)>(structure.Fields.Count);

        foreach (var field in structure.Fields)
        {
            var baseKey = FieldKeyAliases.TryGetValue(field.Name, out var alias)
                ? alias
                : NormalizeFieldKey(field.Name);
            if (field.BitIndex is { } bitIndex)
            {
                baseKey = $"{baseKey}_Bit{bitIndex}";
            }

            var key = baseKey;
            for (var suffix = 2; !usedKeys.Add(key); suffix++)
            {
                key = $"{baseKey}_{suffix}";
            }

            result.Add((field, key));
        }

        return result;
    }

    private static void AppendFriendlyField(
        StringBuilder builder,
        SmbiosField field,
        string key)
    {
        AppendValue(builder, key, field.Value);
        AppendValue(builder, $"{key}.Offset", $"0x{field.Offset:X2}");
        AppendValue(builder, $"{key}.Length", field.Length.ToString(CultureInfo.InvariantCulture));

        if (!string.IsNullOrWhiteSpace(field.Note))
        {
            AppendValue(builder, $"{key}.Note", field.Note);
        }

        if (field.BitIndex is { } bitIndex)
        {
            AppendValue(builder, $"{key}.BitIndex", bitIndex.ToString(CultureInfo.InvariantCulture));
            AppendValue(builder, $"{key}.BitValue", field.BitValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
            AppendValue(builder, $"{key}.State", field.BitValue == 1 ? "Yes" : "No");
        }
    }

    private static string GetSectionName(SmbiosStructure structure) =>
        $"Type{structure.Type}_Handle{structure.Handle:X4}";

    private static string NormalizeFieldKey(string name)
    {
        var builder = new StringBuilder(name.Length);
        var capitalizeNext = true;
        foreach (var character in name)
        {
            if (char.IsLetterOrDigit(character) || character == '_')
            {
                builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
                capitalizeNext = character == '_';
            }
            else
            {
                capitalizeNext = true;
            }
        }

        return builder.Length == 0 ? "Field" : builder.ToString();
    }

    private static void AppendValue(StringBuilder builder, string key, string value) =>
        builder.Append(key)
            .Append('=')
            .AppendLine(NormalizeValue(value));

    private static string NormalizeValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            switch (character)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (char.IsControl(character))
                    {
                        builder.Append("\\u")
                            .Append(((int)character).ToString("X4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    private static string FormatHexLine(ReadOnlySpan<byte> bytes)
    {
        var hex = new StringBuilder(47);
        var ascii = new StringBuilder(16);

        for (var index = 0; index < bytes.Length; index++)
        {
            if (index > 0)
            {
                hex.Append(' ');
            }

            hex.Append(bytes[index].ToString("X2", CultureInfo.InvariantCulture));
            ascii.Append(bytes[index] is >= 0x20 and <= 0x7E ? (char)bytes[index] : '.');
        }

        return $"{hex.ToString().PadRight(47)}  |{ascii.ToString().PadRight(16)}|";
    }
}
