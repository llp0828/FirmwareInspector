using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using FirmwareInspector.Core.Exporting;
using FirmwareInspector.Core.Interop;
using FirmwareInspector.Core.Models;
using FirmwareInspector.Core.Parsing;

Console.OutputEncoding = new UTF8Encoding(false);

if (args.Length == 0 || IsHelp(args[0]))
{
    Console.Write(CliHelp.Text);
    return 0;
}

try
{
    var command = args[0].ToLowerInvariant();

    if (command is "list" or "/list")
    {
        return RunList(args[1..]);
    }

    if (command is "export-ini" or "/export-ini")
    {
        return RunExportIni(args[1..]);
    }

    if (command.StartsWith("/export-ini:", StringComparison.OrdinalIgnoreCase))
    {
        return RunExportIni([args[0]["/export-ini:".Length..], .. args[1..]]);
    }

    if (command is "type" or "/type")
    {
        return RunType(args[1..]);
    }

    if (command.StartsWith("/type:", StringComparison.OrdinalIgnoreCase))
    {
        return RunType([args[0]["/type:".Length..], .. args[1..]]);
    }

    throw new ArgumentException($"未知命令：{args[0]}");
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine($"参数错误：{exception.Message}");
    Console.Error.WriteLine("使用 /? 查看命令说明。");
    return 2;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"执行失败：{exception.Message}");
    return 1;
}

static int RunList(string[] args)
{
    var outputPath = ReadSinglePositionalPath(args);
    var result = ReadSmbios();
    var builder = new StringBuilder();
    builder.AppendLine("Index\tType\tHandle\tOffset\tLength\tName\tSummary");

    foreach (var structure in result.Structures)
    {
        builder.Append(structure.Index.ToString(CultureInfo.InvariantCulture)).Append('\t')
            .Append(structure.Type.ToString(CultureInfo.InvariantCulture)).Append('\t')
            .Append($"0x{structure.Handle:X4}").Append('\t')
            .Append($"0x{structure.TableOffset:X4}").Append('\t')
            .Append(structure.Length.ToString(CultureInfo.InvariantCulture)).Append('\t')
            .Append(NormalizeLine(structure.TypeName)).Append('\t')
            .AppendLine(NormalizeLine(structure.PrimaryText));
    }

    WriteOutput(outputPath, builder.ToString(), $"已输出 {result.Structures.Count} 个结构");
    return 0;
}

static int RunExportIni(string[] args)
{
    var outputPath = args.FirstOrDefault(
        argument => !argument.StartsWith('-') &&
                    !argument.StartsWith("/", StringComparison.Ordinal)) ??
        "smbios.ini";
    var result = ReadSmbios();

    SmbiosIniExporter.Write(outputPath, result);
    Console.WriteLine($"已导出 {result.Structures.Count} 个结构：{Path.GetFullPath(outputPath)}");
    return 0;
}

static int RunType(string[] args)
{
    if (args.Length == 0)
    {
        throw new ArgumentException("type 命令需要 Type Number，例如：type 13");
    }

    var optionStart = 1;
    if (!TryParseTypeNumber(args[0], out var typeNumber))
    {
        if (args.Length > 1 &&
            args[0].Equals("type", StringComparison.OrdinalIgnoreCase) &&
            TryParseTypeNumber($"{args[0]} {args[1]}", out typeNumber))
        {
            optionStart = 2;
        }
        else
        {
            throw new ArgumentException($"无效的 Type Number：{args[0]}");
        }
    }

    string? format = null;
    string? outputPath = null;
    ushort? handle = null;

    for (var index = optionStart; index < args.Length; index++)
    {
        var argument = args[index];
        if (TryReadOption(args, ref index, "--format", out var formatValue))
        {
            format = formatValue.ToLowerInvariant();
        }
        else if (TryReadOption(args, ref index, "--output", out var outputValue))
        {
            outputPath = outputValue;
        }
        else if (TryReadOption(args, ref index, "--handle", out var handleValue))
        {
            if (!TryParseHandle(handleValue, out var parsedHandle))
            {
                throw new ArgumentException($"无效的 Handle：{handleValue}");
            }

            handle = parsedHandle;
        }
        else
        {
            throw new ArgumentException($"未知参数：{argument}");
        }
    }

    format ??= "text";
    if (format is not ("text" or "ini" or "json"))
    {
        throw new ArgumentException("--format 仅支持 text、ini 或 json");
    }

    var result = ReadSmbios();
    var matches = result.Structures
        .Where(structure => structure.Type == typeNumber)
        .Where(structure => handle is null || structure.Handle == handle)
        .ToArray();

    if (matches.Length == 0)
    {
        Console.Error.WriteLine(
            handle is null
                ? $"未找到 Type {typeNumber}。"
                : $"未找到 Type {typeNumber} / Handle 0x{handle:X4}。");
        return 3;
    }

    if (format == "ini" &&
        !string.IsNullOrWhiteSpace(outputPath) &&
        outputPath != "-")
    {
        SmbiosIniExporter.Write(outputPath, result, matches);
        Console.WriteLine($"已输出 Type {typeNumber}，共 {matches.Length} 个结构：{Path.GetFullPath(outputPath)}");
        return 0;
    }

    var content = format switch
    {
        "ini" => SmbiosIniExporter.Build(result, matches),
        "json" => BuildJson(result, matches),
        _ => BuildText(result, matches)
    };

    WriteOutput(
        outputPath,
        content,
        $"已输出 Type {typeNumber}，共 {matches.Length} 个结构");
    return 0;
}

static SmbiosParseResult ReadSmbios()
{
    if (!OperatingSystem.IsWindows())
    {
        throw new PlatformNotSupportedException("GetSystemFirmwareTable 仅支持 Windows。");
    }

    return SmbiosParser.Parse(FirmwareTableReader.ReadRawSmbios());
}

static string BuildText(
    SmbiosParseResult result,
    IReadOnlyCollection<SmbiosStructure> structures)
{
    var builder = new StringBuilder();
    builder.AppendLine($"SMBIOS {result.Version}  Captured {result.CapturedAt:yyyy-MM-dd HH:mm:ss}");
    builder.AppendLine($"Matched structures: {structures.Count}");

    foreach (var structure in structures)
    {
        builder.AppendLine();
        builder.AppendLine(
            $"Type {structure.Type} / Handle 0x{structure.Handle:X4} / Index {structure.Index}");
        builder.AppendLine(structure.TypeName);
        builder.AppendLine(
            $"TableOffset=0x{structure.TableOffset:X4}  Length={structure.Length}  Strings={structure.Strings.Count}");

        builder.AppendLine("Fields:");
        foreach (var field in structure.Fields)
        {
            var bitText = field.BitIndex is { } bitIndex
                ? $"  Bit{bitIndex}"
                : string.Empty;
            builder.Append("  ")
                .Append($"0x{field.Offset:X2}")
                .Append(bitText.PadRight(8))
                .Append("  ")
                .Append(field.Name)
                .Append(" = ")
                .AppendLine(NormalizeLine(field.Value));

            if (!string.IsNullOrWhiteSpace(field.Note))
            {
                builder.Append("          ")
                    .AppendLine(NormalizeLine(field.Note));
            }
        }

        builder.AppendLine("Strings:");
        foreach (var item in structure.Strings)
        {
            builder.Append("  String")
                .Append(item.Index.ToString(CultureInfo.InvariantCulture))
                .Append(" @ 0x")
                .Append(item.Offset.ToString("X4", CultureInfo.InvariantCulture))
                .Append(" = ")
                .AppendLine(NormalizeLine(item.Value));
        }

        builder.AppendLine("Raw:");
        var rawBytes = structure.RawBytes.Span;
        for (var offset = 0; offset < rawBytes.Length; offset += 16)
        {
            builder.Append("  ")
                .Append(offset.ToString("X4", CultureInfo.InvariantCulture))
                .Append("  ")
                .AppendLine(FormatHexLine(rawBytes.Slice(offset, Math.Min(16, rawBytes.Length - offset))));
        }
    }

    return builder.ToString();
}

static string BuildJson(
    SmbiosParseResult result,
    IReadOnlyCollection<SmbiosStructure> structures)
{
    var data = new
    {
        source = result.Source,
        capturedAt = result.CapturedAt,
        version = result.Version.ToString(),
        dmiRevision = result.Version.DmiRevision,
        declaredTableLength = result.DeclaredTableLength,
        parsedTableLength = result.StructureTable.Length,
        warnings = result.Warnings,
        structures = structures.Select(structure => new
        {
            index = structure.Index,
            type = structure.Type,
            typeName = structure.TypeName,
            length = structure.Length,
            handle = $"0x{structure.Handle:X4}",
            offset = $"0x{structure.TableOffset:X4}",
            fields = structure.Fields,
            strings = structure.Strings,
            rawHex = Convert.ToHexString(structure.RawBytes.Span)
        })
    };

    return JsonSerializer.Serialize(
        data,
        new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
}

static void WriteOutput(string? outputPath, string content, string statusText)
{
    if (string.IsNullOrWhiteSpace(outputPath) || outputPath == "-")
    {
        Console.Write(content);
        if (!content.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            Console.WriteLine();
        }

        return;
    }

    var fullPath = Path.GetFullPath(outputPath);
    var directory = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.WriteAllText(fullPath, content, new UTF8Encoding(false));
    Console.WriteLine($"{statusText}：{fullPath}");
}

static string? ReadSinglePositionalPath(string[] args)
{
    if (args.Length == 0)
    {
        return "-";
    }

    if (args.Length == 1 && args[0] is not "-")
    {
        return args[0];
    }

    throw new ArgumentException("list 仅接受一个可选输出路径，或使用 - 输出到标准输出。");
}

static bool TryReadOption(
    string[] args,
    ref int index,
    string option,
    out string value)
{
    var argument = args[index];
    if (argument.Equals(option, StringComparison.OrdinalIgnoreCase))
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{option} 缺少参数值。");
        }

        value = args[++index];
        return true;
    }

    var prefix = $"{option}=";
    if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
    {
        value = argument[prefix.Length..];
        return true;
    }

    value = string.Empty;
    return false;
}

static bool TryParseTypeNumber(string value, out byte typeNumber)
{
    var normalized = value.Trim();
    if (normalized.StartsWith("type", StringComparison.OrdinalIgnoreCase))
    {
        normalized = normalized[4..].Trim();
    }

    if (normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
    {
        return byte.TryParse(
            normalized[2..],
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out typeNumber);
    }

    return byte.TryParse(
        normalized,
        NumberStyles.Integer,
        CultureInfo.InvariantCulture,
        out typeNumber);
}

static bool TryParseHandle(string value, out ushort handle)
{
    var normalized = value.Trim();
    if (normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
    {
        return ushort.TryParse(
            normalized[2..],
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture,
            out handle);
    }

    return ushort.TryParse(
        normalized,
        NumberStyles.Integer,
        CultureInfo.InvariantCulture,
        out handle);
}

static string NormalizeLine(string value) =>
    value.Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal);

static string FormatHexLine(ReadOnlySpan<byte> bytes)
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

static bool IsHelp(string value) =>
    value is "/?" or "-?" or "/h" or "-h" or "--help" ||
    value.Equals("help", StringComparison.OrdinalIgnoreCase);

internal static class CliHelp
{
    internal const string Text = """
Firmware Inspector CLI
通过 GetSystemFirmwareTable('RSMB') 读取并解析本机 SMBIOS 数据，不启动 WPF UI。

用法：
  FirmwareInspector.Cli.exe /?
  FirmwareInspector.Cli.exe list [output.tsv]
  FirmwareInspector.Cli.exe export-ini [output.ini]
  FirmwareInspector.Cli.exe type <type-number> [options]

type 命令参数：
  --format text|ini|json   输出格式，默认 text
  --handle <handle>        仅输出指定 Handle，支持 0x0017 或 23
  --output <path>          输出到文件；省略时写入标准输出

兼容写法：
  /list
  /export-ini
  /export-ini:<path>
  /type <type-number>
  /type:<type-number>

Type Number 支持十进制、Type 13 和 0x0D 三种写法。

示例：
  FirmwareInspector.Cli.exe list
  FirmwareInspector.Cli.exe export-ini D:\temp\smbios.ini
  FirmwareInspector.Cli.exe type 13
  FirmwareInspector.Cli.exe type 0 --format ini
  FirmwareInspector.Cli.exe type 17 --handle 0x0042 --format json --output D:\temp\type17.json

退出码：
  0  成功
  1  读取或写入失败
  2  命令参数错误
  3  未找到匹配的 Type
""";
}
