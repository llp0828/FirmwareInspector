using System.Buffers.Binary;
using System.Text;
using FirmwareInspector.Core.Exporting;
using FirmwareInspector.Core.Models;
using FirmwareInspector.Core.Parsing;

namespace FirmwareInspector.Core.Tests;

public sealed class SmbiosIniExporterTests
{
    [Fact]
    public void ExportsMetadataStructuresFieldsStringsAndRawBytes()
    {
        var result = CreateSampleResult();

        var ini = SmbiosIniExporter.Build(result);

        Assert.Contains("[SMBIOS]", ini);
        Assert.Contains("[Index]", ini);
        Assert.Contains("Version=3.8", ini);
        Assert.Contains($"StructureCount={result.Structures.Count}", ini);
        Assert.Contains("[Type0_Handle0000]", ini);
        Assert.Contains("TypeName=Platform Firmware Information", ini);
        Assert.Contains("String.0001.Value=ACME", ini);
        Assert.Contains("Field.0001.Name=结构类型", ini);
        Assert.Contains("Raw.0000=00 1A 00 00", ini);
        Assert.Equal(
            result.Structures.Count,
            ini.Split(Environment.NewLine)
                .Count(line =>
                    line.StartsWith("[Type", StringComparison.Ordinal) &&
                    line.Contains("_Handle", StringComparison.Ordinal)));
    }

    [Fact]
    public void ExportsOnlySelectedTypeAndStableFieldKeys()
    {
        var result = CreateSampleResult();
        var type13 = result.Structures.Where(structure => structure.Type == 13).ToArray();

        var ini = SmbiosIniExporter.Build(result, type13);

        Assert.Single(type13);
        Assert.Contains("ExportedStructureCount=1", ini);
        Assert.Contains("[Type13_Handle0017]", ini);
        Assert.Contains("CurrentLanguage=en|US|iso8859-1", ini);
        Assert.Contains("CurrentLanguage.Offset=0x15", ini);
        Assert.DoesNotContain("[Type1_Handle", ini);
    }

    [Fact]
    public void WritesAutoItCompatibleUtf16LittleEndianWithBom()
    {
        var result = CreateSampleResult();
        var path = Path.Combine(
            Path.GetTempPath(),
            $"firmware-inspector-{Guid.NewGuid():N}.ini");

        try
        {
            SmbiosIniExporter.Write(path, result);

            var bytes = File.ReadAllBytes(path);
            Assert.True(bytes.Length > 2);
            Assert.Equal(0xFF, bytes[0]);
            Assert.Equal(0xFE, bytes[1]);
            Assert.Contains("CurrentLanguage=en|US|iso8859-1", File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static SmbiosParseResult CreateSampleResult()
    {
        var firmware = Structure(
            0,
            0x1A,
            0x0000,
            data =>
            {
                data[0x04] = 1;
                data[0x05] = 2;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x06, 2), 0xF000);
                data[0x08] = 3;
                data[0x09] = 0x01;
            },
            "ACME",
            "1.0",
            "01/02/2026");

        var language = Structure(
            13,
            0x16,
            0x0017,
            data =>
            {
                data[0x04] = 3;
                data[0x05] = 0;
                data[0x15] = 1;
            },
            "en|US|iso8859-1",
            "fr|FR|iso8859-1",
            "zh|CN|unicode");

        return SmbiosParser.Parse(
            RawSmbios(
                3,
                8,
                0,
                firmware,
                language,
                Structure(127, 4, 0x7F00)));
    }

    private static byte[] RawSmbios(byte major, byte minor, byte revision, params byte[][] chunks)
    {
        var table = chunks.SelectMany(chunk => chunk).ToArray();
        var result = new byte[8 + table.Length];
        result[1] = major;
        result[2] = minor;
        result[3] = revision;
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4, 4), (uint)table.Length);
        table.CopyTo(result, 8);
        return result;
    }

    private static byte[] Structure(
        byte type,
        int length,
        ushort handle,
        Action<byte[]>? configure = null,
        params string[] strings)
    {
        var formatted = new byte[length];
        formatted[0] = type;
        formatted[1] = checked((byte)length);
        BinaryPrimitives.WriteUInt16LittleEndian(formatted.AsSpan(2, 2), handle);
        configure?.Invoke(formatted);

        var builder = new List<byte>(formatted);
        foreach (var value in strings)
        {
            builder.AddRange(Encoding.UTF8.GetBytes(value));
            builder.Add(0);
        }

        builder.Add(0);
        return builder.ToArray();
    }
}
