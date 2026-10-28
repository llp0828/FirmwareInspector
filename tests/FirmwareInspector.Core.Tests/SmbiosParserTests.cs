using System.Buffers.Binary;
using System.Text;
using FirmwareInspector.Core.Parsing;

namespace FirmwareInspector.Core.Tests;

public sealed class SmbiosParserTests
{
    [Fact]
    public void ParsesRawHeaderStructureHeaderStringsAndSystemUuid()
    {
        var system = Structure(
            1,
            0x1B,
            0x0100,
            data =>
            {
                data[0x04] = 1;
                data[0x05] = 2;
                data[0x06] = 3;
                data[0x07] = 4;

                new byte[]
                {
                    0x33, 0x22, 0x11, 0x00,
                    0x55, 0x44,
                    0x77, 0x66,
                    0x88, 0x99,
                    0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
                }.CopyTo(data, 0x08);

                data[0x18] = 0x06;
                data[0x19] = 5;
                data[0x1A] = 6;
            },
            "ACME",
            "Model X",
            "1.0",
            "SN123",
            "SKU-1",
            "ACME Family");

        var raw = RawSmbios(
            major: 3,
            minor: 8,
            dmiRevision: 0,
            system,
            EndOfTable());

        var result = SmbiosParser.Parse(raw);

        Assert.Equal("3.8", result.Version.ToString());
        Assert.Equal(0, result.Version.DmiRevision);
        Assert.True(
            result.Structures.Count == 2,
            $"Structures={result.Structures.Count}; RawLength={raw.Length}; Declared={result.DeclaredTableLength}; TableLength={result.StructureTable.Length}; Warnings={string.Join(" | ", result.Warnings)}; RawTail={Convert.ToHexString(raw.AsSpan(Math.Max(0, raw.Length - 32)))}");
        Assert.Empty(result.Warnings);

        var parsed = result.Structures[0];
        Assert.Equal(1, parsed.Type);
        Assert.Equal(0x0100, parsed.Handle);
        Assert.Equal(6, parsed.Strings.Count);
        Assert.Equal("ACME", parsed.Strings[0].Value);

        Assert.Equal("ACME", Field(parsed, "Manufacturer"));
        Assert.Equal("Model X", Field(parsed, "Product Name"));
        Assert.Equal("00112233-4455-6677-8899-AABBCCDDEEFF", Field(parsed, "UUID"));
        Assert.Equal("Power Switch (0x06)", Field(parsed, "Wake-up Type"));
        Assert.Equal("SKU-1", Field(parsed, "SKU Number"));
        Assert.Equal("ACME Family", Field(parsed, "Family"));

        Assert.Equal(127, result.Structures[1].Type);
        Assert.Equal(result.StructureTable.Length, result.TableByteLength);
    }

    [Fact]
    public void ParsesPlatformFirmwareRomAndCharacteristicsFields()
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
                data[0x09] = 0x0F;
                BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(0x0A, 8), 0x0000000000001090);
                data[0x14] = 1;
                data[0x15] = 17;
                data[0x16] = 1;
                data[0x17] = 15;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x18, 2), 0x4010);
            },
            "Vendor",
            "Version",
            "01/02/2026");

        var result = Parse(firmware, EndOfTable());
        var parsed = result.Structures[0];

        Assert.Equal("Vendor", Field(parsed, "Firmware Vendor"));
        Assert.Equal("0xF000", Field(parsed, "BIOS Starting Address Segment"));
        Assert.Equal("1,024 KiB (1 MiB)", Field(parsed, "Firmware ROM Size"));
        Assert.Equal("1 (Yes)", Field(parsed, "Firmware Characteristics: PCI supported"));
        Assert.Equal("1 (Yes)", Field(parsed, "Firmware Characteristics: Firmware shadowing allowed"));
        Assert.Equal("1", Field(parsed, "Platform Firmware Major Release"));
        Assert.Equal("17", Field(parsed, "Platform Firmware Minor Release"));
    }

    [Fact]
    public void ParsesCacheSizeUsingSpecifiedGranularity()
    {
        var cache = Structure(
            7,
            0x1B,
            0x0700,
            data =>
            {
                data[0x04] = 1;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x05, 2), 0x0181);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x07, 2), 0x0400);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x09, 2), 0x0200);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0B, 2), 0x0020);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0D, 2), 0x0020);
                data[0x0F] = 1;
                data[0x10] = 0x05;
                data[0x11] = 0x05;
                data[0x12] = 0x07;
            },
            "L3 Cache");

        var parsed = Parse(cache, EndOfTable()).Structures[0];

        Assert.Equal("1 MiB", Field(parsed, "Maximum Cache Size"));
        Assert.Equal("512 KiB", Field(parsed, "Installed Cache Size"));
        Assert.Contains("Level 2", Field(parsed, "Cache Configuration"));
        Assert.Equal("Write Back", Field(parsed, "Operational Mode [9:8]"));
        Assert.Equal("1 (Yes)", Field(parsed, "Current SRAM Type: Synchronous"));
    }

    [Fact]
    public void ParsesMemoryDeviceCapacitySpeedAndFormFactor()
    {
        var memory = Structure(
            17,
            0x64,
            0x1100,
            data =>
            {
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x04, 2), 0x1000);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x06, 2), 0xFFFE);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x08, 2), 80);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0A, 2), 64);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0C, 2), 0x4000);
                data[0x0E] = 0x0D;
                data[0x10] = 1;
                data[0x11] = 2;
                data[0x12] = 0x22;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x13, 2), 0x0080);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x15, 2), 6400);
                data[0x17] = 3;
                data[0x18] = 4;
                data[0x19] = 5;
                data[0x1A] = 6;
                data[0x1B] = 1;
                BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x1C, 4), 0);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x20, 2), 5600);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x26, 2), 1100);
                data[0x28] = 0x03;
            },
            "DIMM 0",
            "BANK 0",
            "Samsung",
            "SERIAL",
            "ASSET",
            "PART");

        var parsed = Parse(memory, EndOfTable()).Structures[0];

        Assert.Equal("16 GiB", Field(parsed, "Size"));
        Assert.Equal("SODIMM (0x0D)", Field(parsed, "Form Factor"));
        Assert.Equal("DDR5 (0x22)", Field(parsed, "Memory Type"));
        Assert.Equal("6400 MT/s", Field(parsed, "Speed"));
        Assert.Equal("5600 MT/s", Field(parsed, "Configured Memory Speed"));
        Assert.Equal("1100 mV", Field(parsed, "Configured Voltage"));
        Assert.Equal("DRAM (0x03)", Field(parsed, "Memory Technology"));
        Assert.Equal("1 (Yes)", Field(parsed, "Type Detail: Synchronous"));
    }

    [Fact]
    public void ParsesProcessorFamilyStatusAndExtendedCounts()
    {
        var processor = Structure(
            4,
            0x34,
            0x0400,
            data =>
            {
                data[0x04] = 1;
                data[0x05] = 0x03;
                data[0x06] = 0xC6;
                data[0x07] = 2;
                data[0x10] = 3;
                data[0x11] = 0x92;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x12, 2), 100);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x14, 2), 4900);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x16, 2), 3200);
                data[0x18] = 0x41;
                data[0x19] = 0x3E;
                data[0x23] = 0x08;
                data[0x24] = 0x08;
                data[0x25] = 0x10;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x26, 2), 0x03FC);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x28, 2), 0x00C6);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x2A, 2), 0x0008);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x2C, 2), 0x0008);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x2E, 2), 0x0010);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x30, 2), 0x0010);
                data[0x32] = 7;
            },
            "U3E1",
            "Intel",
            "Test CPU",
            "SERIAL",
            "ASSET",
            "PART",
            "Socket LGA1700");

        var parsed = Parse(processor, EndOfTable()).Structures[0];

        Assert.Equal("Central Processor (0x03)", Field(parsed, "Processor Type"));
        Assert.Equal("Intel Core i7 processor (0x00C6)", Field(parsed, "Processor Family"));
        Assert.Equal("1.8 V", Field(parsed, "Voltage"));
        Assert.Equal("Enabled; socket populated", Field(parsed, "Status"));
        Assert.Equal("8", Field(parsed, "Core Count"));
        Assert.Equal("8", Field(parsed, "Core Enabled"));
        Assert.Equal("16", Field(parsed, "Thread Count"));
        Assert.Equal("16", Field(parsed, "Thread Enabled"));
        Assert.Equal("Socket LGA1700", Field(parsed, "Socket Type"));
        Assert.Equal("1 (Yes)", Field(parsed, "Processor Characteristics: 64-bit capable"));
        Assert.Equal("1 (Yes)", Field(parsed, "Processor Characteristics: Multi-Core"));
        Assert.Equal("1 (Yes)", Field(parsed, "Processor Characteristics: 128-bit Capable"));
        Assert.Equal("1 (Yes)", Field(parsed, "Processor Characteristics: Arm64 SoC ID"));
    }

    [Fact]
    public void ParsesExtendedMemorySizeWhenSizeIs7FFF()
    {
        var memory = Structure(
            17,
            0x28,
            0x1101,
            data =>
            {
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0C, 2), 0x7FFF);
                BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x1C, 4), 0x00008000);
            });

        var parsed = Parse(memory, EndOfTable()).Structures[0];

        Assert.Equal("32 GiB", Field(parsed, "Size"));
    }

    [Fact]
    public void RejectsTruncatedStructureAndRecordsWarning()
    {
        var table = new byte[]
        {
            1, 0x1B, 0x00, 0x01,
            1, 2, 0, 0
        };

        var result = ParseRawTable(table);

        Assert.Empty(result.Structures);
        Assert.Contains(result.Warnings, item => item.Contains("越过表尾", StringComparison.Ordinal));
    }

    [Fact]
    public void ParsesTableWithoutRawHeaderWithWarning()
    {
        var table = Concat(EndOfTable());
        var result = SmbiosParser.Parse(table);

        Assert.Single(result.Structures);
        Assert.Contains(result.Warnings, item => item.Contains("未检测到 RawSMBIOSData 头", StringComparison.Ordinal));
    }

    [Fact]
    public void AllStandardTypesHaveFieldLevelBranches()
    {
        var structures = Enumerable.Range(0, 47)
            .Select(type => Structure(
                (byte)type,
                128,
                (ushort)(0x100 + type)))
            .Append(Structure(126, 4, 0x7E00))
            .Append(EndOfTable())
            .ToArray();

        var result = Parse(structures);

        Assert.Equal(49, result.Structures.Count);

        foreach (var structure in result.Structures)
        {
            Assert.DoesNotContain(
                structure.Fields,
                field => field.Name == "字段级解析");
            Assert.True(
                structure.Fields.Count > 3,
                $"Type {structure.Type} did not add any type-specific fields.");
        }
    }

    [Fact]
    public void ParsesRemainingStandardTypesWithFieldValues()
    {
        var controller = Structure(
            5,
            0x12,
            0x0500,
            data =>
            {
                data[0x04] = 5;
                data[0x05] = 0x08;
                data[0x06] = 3;
                data[0x07] = 3;
                data[0x08] = 5;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x09, 2), 0x14);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0B, 2), 0x00A4);
                data[0x0D] = 0x01;
                data[0x0E] = 1;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0F, 2), 0x0600);
                data[0x11] = 0x08;
            });
        var module = Structure(
            6,
            0x0C,
            0x0600,
            data =>
            {
                data[0x04] = 1;
                data[0x05] = 0x13;
                data[0x06] = 70;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x07, 2), 0x00A4);
                data[0x09] = 0x83;
                data[0x0A] = 0x83;
                data[0x0B] = 0x00;
            },
            "J202");
        var pointing = Structure(
            21,
            7,
            0x1500,
            data =>
            {
                data[0x04] = 0x07;
                data[0x05] = 0xA2;
                data[0x06] = 3;
            });
        var battery = Structure(
            22,
            0x1A,
            0x1600,
            data =>
            {
                data[0x04] = 1;
                data[0x05] = 2;
                data[0x06] = 3;
                data[0x07] = 4;
                data[0x08] = 5;
                data[0x09] = 0x06;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0A, 2), 5000);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0C, 2), 15000);
                data[0x0E] = 7;
                data[0x0F] = 5;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x10, 2), 0x1234);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x12, 2), 0x58EF);
                data[0x14] = 0;
                data[0x15] = 10;
                BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x16, 4), 0x12345678);
            },
            "Bay 1",
            "Battery Co",
            "2020-01-01",
            "SERIAL",
            "Battery Pack",
            "Li-Ion",
            "1.1");
        var reset = Structure(
            23,
            0x0D,
            0x1700,
            data =>
            {
                data[0x04] = 0x2B;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x05, 2), 2);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x07, 2), 3);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x09, 2), 5);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0B, 2), 10);
            });
        var powerControls = Structure(
            25,
            9,
            0x1900,
            data =>
            {
                data[0x04] = 0x12;
                data[0x05] = 0x31;
                data[0x06] = 0x23;
                data[0x07] = 0x59;
                data[0x08] = 0x58;
            });
        var remoteAccess = Structure(
            30,
            6,
            0x1E00,
            data =>
            {
                data[0x04] = 1;
                data[0x05] = 0x03;
            },
            "Remote Manager");
        var memoryError = Structure(
            33,
            0x1F,
            0x2100,
            data =>
            {
                data[0x04] = 0x06;
                data[0x05] = 0x03;
                data[0x06] = 0x03;
                BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x07, 4), 0x11223344);
                BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(0x0B, 8), 0x8000000000000000);
                BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(0x13, 8), 0x0000000100000000);
                BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x1B, 4), 0x1000);
            });
        var component = Structure(
            35,
            0x0B,
            0x2300,
            data =>
            {
                data[0x04] = 1;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x05, 2), 0x0035);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x07, 2), 0x0028);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x09, 2), 0x2400);
            },
            "Current Probe Component");
        var thresholds = Structure(
            36,
            0x10,
            0x2400,
            data =>
            {
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x04, 2), 100);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x06, 2), 900);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x08, 2), 50);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0A, 2), 1000);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0C, 2), 0x8000);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0E, 2), 0x8000);
            });
        var channel = Structure(
            37,
            0x0D,
            0x2500,
            data =>
            {
                data[0x04] = 3;
                data[0x05] = 8;
                data[0x06] = 2;
                data[0x07] = 4;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x08, 2), 0x0042);
                data[0x0A] = 4;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x0B, 2), 0x0044);
            });
        var ipmi = Structure(
            38,
            0x12,
            0x2600,
            data =>
            {
                data[0x04] = 0x01;
                data[0x05] = 0x10;
                data[0x06] = 0x20;
                data[0x07] = 0xFF;
                BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(0x08, 8), 0x1235);
                data[0x10] = 0x43;
                data[0x11] = 5;
            });
        var additional = Structure(
            40,
            0x0B,
            0x2800,
            data =>
            {
                data[0x04] = 1;
                data[0x05] = 6;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x06, 2), 0x003B);
                data[0x08] = 0x0C;
                data[0x09] = 1;
                data[0x0A] = 0x7E;
            },
            "Additional value");
        var mctp = Structure(
            42,
            0x0E,
            0x2A00,
            data =>
            {
                data[0x04] = 0xF0;
                data[0x05] = 4;
                data[0x06] = 0x00;
                data[0x07] = 0x00;
                data[0x08] = 0x1A;
                data[0x09] = 0x2B;
                data[0x0A] = 1;
                data[0x0B] = 0x03;
                data[0x0C] = 1;
                data[0x0D] = 0xAA;
            });
        var firmwareInventory = Structure(
            45,
            0x1A,
            0x2D00,
            data =>
            {
                data[0x04] = 1;
                data[0x05] = 2;
                data[0x06] = 1;
                data[0x07] = 3;
                data[0x08] = 1;
                data[0x09] = 4;
                data[0x0A] = 5;
                data[0x0B] = 6;
                BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(0x0C, 8), 0x1000);
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x14, 2), 0x0003);
                data[0x16] = 4;
                data[0x17] = 1;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x18, 2), 0x0042);
            },
            "BIOS",
            "1.2.3",
            "GUID",
            "2026-01-01T00:00:00Z",
            "Vendor",
            "1.0.0");
        var stringProperty = Structure(
            46,
            9,
            0x2E00,
            data =>
            {
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x04, 2), 1);
                data[0x06] = 1;
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(0x07, 2), 0x005B);
            },
            @"PciRoot(0x0)/Pci(0x1F,0x3)");

        var result = Parse(
            controller,
            module,
            pointing,
            battery,
            reset,
            powerControls,
            remoteAccess,
            memoryError,
            component,
            thresholds,
            channel,
            ipmi,
            additional,
            mctp,
            firmwareInventory,
            stringProperty,
            EndOfTable());

        Assert.Equal("32-bit ECC (0x05)", Field(result.Structures[0], "Error Detecting Method"));
        Assert.Equal("32 MiB", Field(result.Structures[0], "Maximum Memory Module Size"));
        Assert.Equal("0x0600", Field(result.Structures[0], "Memory Module Configuration Handles"));

        Assert.Equal("Double-bank connection", FieldNote(result.Structures[1], "Installed Size"));
        Assert.Equal("Touch Pad (0x07)", Field(result.Structures[2], "Pointing Device Type"));
        Assert.Equal("USB (0xA2)", Field(result.Structures[2], "Interface Type"));
        Assert.Equal("Lithium Ion (0x06)", Field(result.Structures[3], "Device Chemistry"));
        Assert.Equal("2024-07-15 (0x58EF)", Field(result.Structures[3], "SBDS Manufacture Date"));
        Assert.Equal("1 (Yes)", Field(result.Structures[4], "System reset enabled [0]"));
        Assert.Equal("12 (BCD 0x12)", Field(result.Structures[5], "Next Scheduled Power-on Month"));
        Assert.Equal("1 (Yes)", Field(result.Structures[6], "Connections: Inbound connection enabled"));
        Assert.Equal("1 (Yes)", Field(result.Structures[6], "Connections: Outbound connection enabled"));
        Assert.Equal("Unknown (8000 0000 0000 0000h)", Field(result.Structures[7], "Memory Array Error Address"));
        Assert.Equal("Current Probe Component", Field(result.Structures[8], "Description"));
        Assert.Equal("100", Field(result.Structures[9], "Lower Threshold — Non-critical"));
        Assert.Equal("Load 4, Handle 0x0042", Field(result.Structures[10], "Memory Device 1"));
        Assert.Equal("KCS: Keyboard Controller Style (0x01)", Field(result.Structures[11], "Interface Type"));
        Assert.Equal("1.0 (0x10)", Field(result.Structures[11], "IPMI Specification Revision"));
        Assert.Contains("Offset 0x0C", Field(result.Structures[12], "Entry 1"));
        Assert.Contains("OEM-defined", Field(result.Structures[13], "Interface Type"));
        Assert.Contains("Protocol Record 1", result.Structures[13].Fields.Select(item => item.Name));
        Assert.Equal("1.2.3", Field(result.Structures[14], "Firmware Version"));
        Assert.Equal("Enabled (0x04)", Field(result.Structures[14], "State"));
        Assert.Equal("1 · UEFI Device Path", Field(result.Structures[15], "String Property ID"));
        Assert.Equal("0x005B", Field(result.Structures[15], "Parent Handle"));
    }

    private static string Field(FirmwareInspector.Core.Models.SmbiosStructure structure, string name) =>
        structure.Fields.Single(item => item.Name == name).Value;

    private static string FieldNote(FirmwareInspector.Core.Models.SmbiosStructure structure, string name) =>
        structure.Fields.Single(item => item.Name == name).Note ?? string.Empty;

    private static FirmwareInspector.Core.Models.SmbiosParseResult Parse(params byte[][] structures) =>
        SmbiosParser.Parse(Concat(structures));

    private static FirmwareInspector.Core.Models.SmbiosParseResult ParseRawTable(byte[] table) =>
        SmbiosParser.Parse(RawSmbios(3, 8, 0, table));

    private static byte[] RawSmbios(byte major, byte minor, byte dmiRevision, params byte[][] chunks)
    {
        var table = Concat(chunks);
        var result = new byte[8 + table.Length];
        result[0] = 0;
        result[1] = major;
        result[2] = minor;
        result[3] = dmiRevision;
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

        if (strings.Length == 0)
        {
            builder.Add(0);
        }

        builder.Add(0);
        return builder.ToArray();
    }

    private static byte[] EndOfTable() => Structure(127, 4, 0x7F00);

    private static byte[] Concat(params byte[][] chunks)
    {
        var length = chunks.Sum(item => item.Length);
        var result = new byte[length];
        var offset = 0;

        foreach (var chunk in chunks)
        {
            chunk.CopyTo(result, offset);
            offset += chunk.Length;
        }

        return result;
    }
}
