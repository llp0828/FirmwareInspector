using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using FirmwareInspector.Core.Models;

namespace FirmwareInspector.Core.Parsing;

internal static partial class SmbiosFieldParser
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static void Populate(SmbiosStructure structure)
    {
        Add(structure, "结构类型", $"{structure.Type} · {structure.TypeName}", 0x00, 1);
        Add(structure, "结构长度", $"{structure.Length} 字节", 0x01, 1);
        Add(structure, "句柄", $"0x{structure.Handle:X4}", 0x02, 2);

        switch (structure.Type)
        {
            case 0:
                ParsePlatformFirmware(structure);
                break;
            case 1:
                ParseSystemInformation(structure);
                break;
            case 2:
                ParseBaseboard(structure);
                break;
            case 3:
                ParseChassis(structure);
                break;
            case 4:
                ParseProcessor(structure);
                break;
            case 5:
                ParseMemoryController(structure);
                break;
            case 6:
                ParseMemoryModule(structure);
                break;
            case 7:
                ParseCache(structure);
                break;
            case 8:
                ParsePortConnector(structure);
                break;
            case 9:
                ParseSystemSlot(structure);
                break;
            case 10:
                ParseOnboardDevices(structure);
                break;
            case 11:
                ParseOemStrings(structure);
                break;
            case 12:
                ParseSystemConfigurationOptions(structure);
                break;
            case 13:
                ParseFirmwareLanguage(structure);
                break;
            case 14:
                ParseGroupAssociations(structure);
                break;
            case 15:
                ParseSystemEventLog(structure);
                break;
            case 16:
                ParsePhysicalMemoryArray(structure);
                break;
            case 17:
                ParseMemoryDevice(structure);
                break;
            case 18:
                ParseMemoryError(structure);
                break;
            case 19:
                ParseMemoryArrayMappedAddress(structure);
                break;
            case 20:
                ParseMemoryDeviceMappedAddress(structure);
                break;
            case 21:
                ParsePointingDevice(structure);
                break;
            case 22:
                ParsePortableBattery(structure);
                break;
            case 23:
                ParseSystemReset(structure);
                break;
            case 24:
                ParseHardwareSecurity(structure);
                break;
            case 25:
                ParseSystemPowerControls(structure);
                break;
            case 26:
                ParseVoltageProbe(structure);
                break;
            case 27:
                ParseCoolingDevice(structure);
                break;
            case 28:
                ParseTemperatureProbe(structure);
                break;
            case 29:
                ParseCurrentProbe(structure);
                break;
            case 30:
                ParseOutOfBandRemoteAccess(structure);
                break;
            case 31:
                ParseBisEntryPoint(structure);
                break;
            case 32:
                ParseBootInformation(structure);
                break;
            case 33:
                ParseMemoryError64(structure);
                break;
            case 34:
                ParseManagementDevice(structure);
                break;
            case 35:
                ParseManagementDeviceComponent(structure);
                break;
            case 36:
                ParseManagementDeviceThresholdData(structure);
                break;
            case 37:
                ParseMemoryChannel(structure);
                break;
            case 38:
                ParseIpmiDevice(structure);
                break;
            case 39:
                ParseSystemPowerSupply(structure);
                break;
            case 40:
                ParseAdditionalInformation(structure);
                break;
            case 41:
                ParseOnboardDevicesExtended(structure);
                break;
            case 42:
                ParseManagementControllerHostInterface(structure);
                break;
            case 43:
                ParseTpmDevice(structure);
                break;
            case 44:
                ParseProcessorAdditionalInformation(structure);
                break;
            case 45:
                ParseFirmwareInventoryInformation(structure);
                break;
            case 46:
                ParseStringProperty(structure);
                break;
            case 126:
                Add(structure, "状态", "Inactive structure，应按未识别结构跳过", 0x00, 1);
                break;
            case 127:
                Add(structure, "说明", "结构表在此结束", 0x00, 1);
                break;
            default:
                Add(
                    structure,
                    "字段级解析",
                    structure.Type >= 0x80
                        ? "OEM 自定义结构，仅显示通用结构和原始字节"
                        : structure.Type is >= 0x2F and <= 0x7D
                            ? "DSP0134 3.8.0 未分配该标准 Type，仅显示通用结构和原始字节"
                            : "该 Type 没有可用的标准字段定义，仅显示通用结构和原始字节",
                    0x00,
                    0);
                break;
        }
    }

    private static void ParsePlatformFirmware(SmbiosStructure structure)
    {
        AddString(structure, "Firmware Vendor", 0x04);
        AddString(structure, "Firmware Version", 0x05);

        if (TryU16(structure, 0x06, out var addressSegment))
        {
            Add(
                structure,
                "BIOS Starting Address Segment",
                $"0x{addressSegment:X4}",
                0x06,
                2,
                addressSegment == 0
                    ? "规范说明 UEFI 系统通常为 0000h"
                    : $"物理地址起点 0x{(uint)addressSegment << 4:X5}");
        }

        AddString(structure, "Firmware Release Date", 0x08);

        if (TryByte(structure, 0x09, out var romSize))
        {
            if (romSize == 0xFF &&
                TryU16(structure, 0x18, out var extendedRomSize))
            {
                Add(
                    structure,
                    "Firmware ROM Size",
                    DecodeExtendedRomSize(extendedRomSize),
                    0x18,
                    2,
                    "ROM Size 为 FFh，使用 Extended Firmware ROM Size");
            }
            else
            {
                Add(
                    structure,
                    "Firmware ROM Size",
                    $"{((int)romSize + 1) * 64:N0} KiB ({FormatByteSize(((ulong)romSize + 1) * 64 * 1024)})",
                    0x09,
                    1,
                    romSize == 0xFF ? "FFh 表示 16 MiB 或更大" : null);
            }
        }

        if (TryU16(structure, 0x18, out var rawExtendedRomSize))
        {
            Add(
                structure,
                "Extended Firmware ROM Size",
                $"0x{rawExtendedRomSize:X4} · {DecodeExtendedRomSize(rawExtendedRomSize)}",
                0x18,
                2,
                "仅当 Firmware ROM Size 为 FFh 时用于表示实际容量");
        }

        if (TryU64(structure, 0x0A, out var characteristics))
        {
            Add(
                structure,
                "Firmware Characteristics",
                $"0x{characteristics:X16}",
                0x0A,
                8,
                "Bits 0-31 are listed below; bits 32-47 are vendor-reserved and bits 48-63 are system-vendor-reserved");
            AddBitFields(
                structure,
                0x0A,
                8,
                characteristics,
                (0, "Reserved"),
                (1, "Reserved"),
                (2, "Unknown"),
                (3, "Firmware characteristics not supported"),
                (4, "ISA supported"),
                (5, "MCA supported"),
                (6, "EISA supported"),
                (7, "PCI supported"),
                (8, "PC Card (PCMCIA) supported"),
                (9, "Plug and Play supported"),
                (10, "APM supported"),
                (11, "Firmware is upgradeable (Flash)"),
                (12, "Firmware shadowing allowed"),
                (13, "VL-VESA supported"),
                (14, "ESCD supported"),
                (15, "Boot from CD supported"),
                (16, "Selectable boot supported"),
                (17, "Firmware ROM is socketed"),
                (18, "Boot from PC Card supported"),
                (19, "EDD supported"),
                (20, "Int 13h Japanese floppy for NEC 9800 1.2 MB supported"),
                (21, "Int 13h Japanese floppy for Toshiba 1.2 MB supported"),
                (22, "Int 13h 5.25-inch 360 KB floppy supported"),
                (23, "Int 13h 5.25-inch 1.2 MB floppy supported"),
                (24, "Int 13h 3.5-inch 720 KB floppy supported"),
                (25, "Int 13h 3.5-inch 2.88 MB floppy supported"),
                (26, "Int 5h print screen supported"),
                (27, "Int 9h 8042 keyboard services supported"),
                (28, "Int 14h serial services supported"),
                (29, "Int 17h printer services supported"),
                (30, "Int 10h CGA/Mono video supported"),
                (31, "NEC PC-98 supported"));
        }

        if (TryByte(structure, 0x12, out var extension1))
        {
            Add(
                structure,
                "Firmware Characteristics Extension 1",
                $"0x{extension1:X2}",
                0x12,
                1);
            AddBitFields(
                structure,
                0x12,
                1,
                extension1,
                (0, "ACPI supported"),
                (1, "USB Legacy supported"),
                (2, "AGP supported"),
                (3, "I2O boot supported"),
                (4, "LS-120 SuperDisk boot supported"),
                (5, "ATAPI ZIP drive boot supported"),
                (6, "1394 boot supported"),
                (7, "Smart battery supported"));
        }

        if (TryByte(structure, 0x13, out var extension2))
        {
            Add(
                structure,
                "Firmware Characteristics Extension 2",
                $"0x{extension2:X2}",
                0x13,
                1);
            AddBitFields(
                structure,
                0x13,
                1,
                extension2,
                (0, "BIOS Boot Specification supported"),
                (1, "Function key-initiated network service boot supported"),
                (2, "Targeted content distribution enabled"),
                (3, "UEFI Specification supported"),
                (4, "SMBIOS describes a virtual machine"),
                (5, "Manufacturing mode supported"),
                (6, "Manufacturing mode enabled"),
                (7, "Reserved"));
        }

        AddReleaseByte(structure, "Platform Firmware Major Release", 0x14);
        AddReleaseByte(structure, "Platform Firmware Minor Release", 0x15);
        AddReleaseByte(structure, "Embedded Controller Firmware Major Release", 0x16);
        AddReleaseByte(structure, "Embedded Controller Firmware Minor Release", 0x17);
    }

    private static void ParseSystemInformation(SmbiosStructure structure)
    {
        AddString(structure, "Manufacturer", 0x04);
        AddString(structure, "Product Name", 0x05);
        AddString(structure, "Version", 0x06);
        AddString(structure, "Serial Number", 0x07);

        if (structure.FormattedArea.Length >= 0x18)
        {
            var uuidBytes = structure.FormattedArea.Span.Slice(0x08, 16);
            string value;

            if (uuidBytes.IndexOfAnyExcept((byte)0) < 0)
            {
                value = "Not present";
            }
            else if (uuidBytes.IndexOfAnyExcept((byte)0xFF) < 0)
            {
                value = "Not present (settable)";
            }
            else
            {
                value = new Guid(uuidBytes).ToString("D").ToUpperInvariant();
            }

            Add(structure, "UUID", value, 0x08, 16, "RFC 4122 字节序，前三字段为 little-endian");
        }

        if (TryByte(structure, 0x18, out var wakeUp))
        {
            Add(
                structure,
                "Wake-up Type",
                Describe(wakeUp, SmbiosEnumCatalog.WakeUpTypes),
                0x18,
                1);
        }

        AddString(structure, "SKU Number", 0x19);
        AddString(structure, "Family", 0x1A);
    }

    private static void ParseBaseboard(SmbiosStructure structure)
    {
        AddString(structure, "Manufacturer", 0x04);
        AddString(structure, "Product", 0x05);
        AddString(structure, "Version", 0x06);
        AddString(structure, "Serial Number", 0x07);
        AddString(structure, "Asset Tag", 0x08);

        if (TryByte(structure, 0x09, out var features))
        {
            Add(
                structure,
                "Feature Flags",
                $"0x{features:X2}",
                0x09,
                1);
            AddBitFields(
                structure,
                0x09,
                1,
                features,
                (0, "Hosting board"),
                (1, "Requires daughter board or auxiliary card"),
                (2, "Removable"),
                (3, "Replaceable"),
                (4, "Hot swappable"),
                (5, "Reserved"),
                (6, "Reserved"),
                (7, "Reserved"));
        }

        AddString(structure, "Location in Chassis", 0x0A);

        if (TryU16(structure, 0x0B, out var chassisHandle))
        {
            Add(structure, "Chassis Handle", $"0x{chassisHandle:X4}", 0x0B, 2);
        }

        if (TryByte(structure, 0x0D, out var boardType))
        {
            Add(
                structure,
                "Board Type",
                Describe(boardType, SmbiosEnumCatalog.BoardTypes),
                0x0D,
                1);
        }

        if (TryByte(structure, 0x0E, out var count))
        {
            Add(structure, "Contained Object Handle Count", count.ToString(Invariant), 0x0E, 1);

            var handles = new List<string>();
            for (var index = 0; index < count; index++)
            {
                var offset = 0x0F + (index * 2);
                if (!TryU16(structure, offset, out var handle))
                {
                    break;
                }

                handles.Add($"0x{handle:X4}");
            }

            if (handles.Count > 0)
            {
                Add(structure, "Contained Object Handles", string.Join(", ", handles), 0x0F, handles.Count * 2);
            }
        }
    }

    private static void ParseChassis(SmbiosStructure structure)
    {
        AddString(structure, "Manufacturer", 0x04);

        if (TryByte(structure, 0x05, out var chassisTypeRaw))
        {
            var chassisType = (byte)(chassisTypeRaw & 0x7F);
            Add(
                structure,
                "Chassis Type",
                Describe(chassisType, SmbiosEnumCatalog.ChassisTypes),
                0x05,
                1);
            var lockPresent = (chassisTypeRaw & 0x80) != 0 ? 1 : 0;
            structure.AddField(new SmbiosField(
                "Chassis lock is present",
                $"{lockPresent} ({(lockPresent == 1 ? "Yes" : "No")})",
                0x05,
                1,
                lockPresent == 1 ? "Chassis lock is present" : "Chassis lock is not present or unknown")
            {
                BitIndex = 7,
                BitValue = lockPresent
            });
        }

        AddString(structure, "Version", 0x06);
        AddString(structure, "Serial Number", 0x07);
        AddString(structure, "Asset Tag Number", 0x08);

        AddEnumByte(structure, "Boot-up State", 0x09, SmbiosEnumCatalog.ChassisStates);
        AddEnumByte(structure, "Power Supply State", 0x0A, SmbiosEnumCatalog.ChassisStates);
        AddEnumByte(structure, "Thermal State", 0x0B, SmbiosEnumCatalog.ChassisStates);
        AddEnumByte(structure, "Security Status", 0x0C, SmbiosEnumCatalog.SecurityStatuses);

        if (TryU32(structure, 0x0D, out var oemDefined))
        {
            Add(structure, "OEM-defined", $"0x{oemDefined:X8}", 0x0D, 4);
        }

        if (TryByte(structure, 0x11, out var height))
        {
            Add(
                structure,
                "Height",
                height == 0 ? "Unspecified" : $"{height} U",
                0x11,
                1);
        }

        if (TryByte(structure, 0x12, out var powerCords))
        {
            Add(
                structure,
                "Number of Power Cords",
                powerCords == 0 ? "Unspecified" : powerCords.ToString(Invariant),
                0x12,
                1);
        }

        if (TryByte(structure, 0x13, out var count) &&
            TryByte(structure, 0x14, out var recordLength))
        {
            Add(structure, "Contained Element Count", count.ToString(Invariant), 0x13, 1);
            Add(structure, "Contained Element Record Length", recordLength.ToString(Invariant), 0x14, 1);

            var skuOffset = 0x15 + (count * recordLength);
            if (skuOffset < structure.FormattedArea.Length)
            {
                AddString(structure, "SKU Number", skuOffset);
            }
        }
    }

    private static void ParseProcessor(SmbiosStructure structure)
    {
        AddString(structure, "Socket Designation", 0x04);
        AddEnumByte(structure, "Processor Type", 0x05, SmbiosEnumCatalog.ProcessorTypes);

        if (TryByte(structure, 0x06, out var familyByte))
        {
            ushort family = familyByte;
            if (familyByte == 0xFE &&
                TryU16(structure, 0x28, out var family2))
            {
                family = family2;
            }

            Add(
                structure,
                "Processor Family",
                Describe(family, SmbiosEnumCatalog.ProcessorFamilies),
                0x06,
                1);
        }

        AddString(structure, "Processor Manufacturer", 0x07);

        if (TryU64(structure, 0x08, out var processorId))
        {
            Add(
                structure,
                "Processor ID",
                $"0x{processorId:X16}",
                0x08,
                8,
                "按规范保留原始 CPUID/厂商标识，不做跨厂商推断");
        }

        AddString(structure, "Processor Version", 0x10);

        if (TryByte(structure, 0x11, out var voltage))
        {
            Add(
                structure,
                "Voltage",
                DecodeProcessorVoltage(voltage),
                0x11,
                1,
                "SMBIOS 3.8 已废弃该字段");
        }

        AddFrequency(structure, "External Clock", 0x12);
        AddFrequency(structure, "Max Speed", 0x14);
        AddFrequency(structure, "Current Speed", 0x16);

        if (TryByte(structure, 0x18, out var status))
        {
            Add(
                structure,
                "Status",
                DecodeProcessorStatus(status),
                0x18,
                1);
        }

        AddEnumByte(structure, "Processor Upgrade", 0x19, SmbiosEnumCatalog.ProcessorUpgrades);
        AddHandle(structure, "L1 Cache Handle", 0x1A);
        AddHandle(structure, "L2 Cache Handle", 0x1C);
        AddHandle(structure, "L3 Cache Handle", 0x1E);
        AddString(structure, "Serial Number", 0x20);
        AddString(structure, "Asset Tag", 0x21);
        AddString(structure, "Part Number", 0x22);

        AddCountByte(structure, "Core Count", 0x23);
        AddExtendedCount(structure, "Core Count 2", 0x2A);
        AddCountByte(structure, "Core Enabled", 0x24);
        AddExtendedCount(structure, "Core Enabled 2", 0x2C);
        AddCountByte(structure, "Thread Count", 0x25);
        AddExtendedCount(structure, "Thread Count 2", 0x2E);

        if (TryU16(structure, 0x26, out var characteristics))
        {
            Add(
                structure,
                "Processor Characteristics",
                $"0x{characteristics:X4}",
                0x26,
                2);
            AddBitFields(
                structure,
                0x26,
                2,
                characteristics,
                (0, "Reserved"),
                (1, "Unknown"),
                (2, "64-bit capable"),
                (3, "Multi-Core"),
                (4, "Hardware Thread"),
                (5, "Execute Protection"),
                (6, "Enhanced Virtualization"),
                (7, "Power/Performance Control"),
                (8, "128-bit Capable"),
                (9, "Arm64 SoC ID"),
                (10, "Reserved"),
                (11, "Reserved"),
                (12, "Reserved"),
                (13, "Reserved"),
                (14, "Reserved"),
                (15, "Reserved"));
        }

        if (TryU16(structure, 0x28, out var rawFamily2))
        {
            Add(structure, "Processor Family 2", $"0x{rawFamily2:X4}", 0x28, 2);
        }

        if (TryU16(structure, 0x30, out var threadEnabled))
        {
            Add(
                structure,
                "Thread Enabled",
                threadEnabled switch
                {
                    0 => "Unknown",
                    0xFFFF => "Reserved (FFFFh)",
                    _ => threadEnabled.ToString(Invariant)
                },
                0x30,
                2);
        }

        AddString(structure, "Socket Type", 0x32);
    }

    private static void ParseCache(SmbiosStructure structure)
    {
        AddString(structure, "Socket Designation", 0x04);

        if (TryU16(structure, 0x05, out var configuration))
        {
            var level = (configuration & 0x07) + 1;
            var location = (configuration >> 5) & 0x03;
            var operationalMode = (configuration >> 8) & 0x03;

            Add(
                structure,
                "Cache Configuration",
                $"Level {level}, {(((configuration & 0x80) != 0) ? "Enabled" : "Disabled")}",
                0x05,
                2);
            Add(structure, "Cache Level [2:0]", $"Level {level}", 0x05, 2, "Bits 2:0 plus one");

            var socketed = (configuration & 0x08) != 0 ? 1 : 0;
            structure.AddField(new SmbiosField(
                "Cache Socketed [3]",
                $"{socketed} ({(socketed == 1 ? "Yes" : "No")})",
                0x05,
                2,
                "Cache is socketed")
            {
                BitIndex = 3,
                BitValue = socketed
            });
            Add(structure, "Reserved [4]", "0", 0x05, 2);
            Add(
                structure,
                "Location [6:5]",
                location switch
                {
                    0 => "Internal",
                    1 => "External",
                    2 => "Reserved",
                    _ => "Unknown"
                },
                0x05,
                2);

            var enabled = (configuration & 0x80) != 0 ? 1 : 0;
            structure.AddField(new SmbiosField(
                "Enabled at boot time [7]",
                $"{enabled} ({(enabled == 1 ? "Yes" : "No")})",
                0x05,
                2,
                "Cache enabled at boot time")
            {
                BitIndex = 7,
                BitValue = enabled
            });
            Add(
                structure,
                "Operational Mode [9:8]",
                operationalMode switch
                {
                    0 => "Write Through",
                    1 => "Write Back",
                    2 => "Varies with Memory Address",
                    _ => "Unknown"
                },
                0x05,
                2);
            Add(structure, "Reserved [15:10]", "0", 0x05, 2);
        }

        AddCacheSize(structure, "Maximum Cache Size", 0x07, 0x13);
        AddCacheSize(structure, "Installed Cache Size", 0x09, 0x17);

        if (TryU16(structure, 0x0B, out var supportedSram))
        {
            Add(structure, "Supported SRAM Type", $"0x{supportedSram:X4}", 0x0B, 2);
            AddBitFields(
                structure,
                0x0B,
                2,
                supportedSram,
                (0, "Other"),
                (1, "Unknown"),
                (2, "Non-Burst"),
                (3, "Burst"),
                (4, "Pipeline Burst"),
                (5, "Synchronous"),
                (6, "Asynchronous"));
        }

        if (TryU16(structure, 0x0D, out var currentSram))
        {
            Add(structure, "Current SRAM Type", $"0x{currentSram:X4}", 0x0D, 2);
            AddBitFields(
                structure,
                0x0D,
                2,
                currentSram,
                (0, "Other"),
                (1, "Unknown"),
                (2, "Non-Burst"),
                (3, "Burst"),
                (4, "Pipeline Burst"),
                (5, "Synchronous"),
                (6, "Asynchronous"));
        }

        if (TryByte(structure, 0x0F, out var speed))
        {
            Add(
                structure,
                "Cache Speed",
                speed == 0 ? "0 ns · Unknown" : $"{speed} ns",
                0x0F,
                1);
        }

        AddEnumByte(structure, "Error Correction Type", 0x10, SmbiosEnumCatalog.CacheErrorCorrections);
        AddEnumByte(structure, "System Cache Type", 0x11, SmbiosEnumCatalog.CacheTypes);
        AddEnumByte(structure, "Associativity", 0x12, SmbiosEnumCatalog.CacheAssociativity);
    }

    private static void ParsePortConnector(SmbiosStructure structure)
    {
        AddString(structure, "Internal Reference Designator", 0x04);
        AddEnumByte(structure, "Internal Connector Type", 0x05, SmbiosEnumCatalog.ConnectorTypes);
        AddString(structure, "External Reference Designator", 0x06);
        AddEnumByte(structure, "External Connector Type", 0x07, SmbiosEnumCatalog.ConnectorTypes);
        AddEnumByte(structure, "Port Type", 0x08, SmbiosEnumCatalog.PortTypes);
    }

    private static void ParseSystemSlot(SmbiosStructure structure)
    {
        AddString(structure, "Slot Designation", 0x04);
        AddEnumByte(structure, "Slot Type", 0x05, SmbiosEnumCatalog.SlotTypes);
        AddEnumByte(structure, "Slot Data Bus Width", 0x06, SmbiosEnumCatalog.SlotWidths);
        AddEnumByte(structure, "Current Usage", 0x07, SmbiosEnumCatalog.SlotUsages);
        AddEnumByte(structure, "Slot Length", 0x08, SmbiosEnumCatalog.SlotLengths);

        if (TryU16(structure, 0x09, out var slotId))
        {
            Add(structure, "Slot ID", $"0x{slotId:X4}", 0x09, 2);
        }

        if (TryByte(structure, 0x0B, out var characteristics1))
        {
            Add(
                structure,
                "Slot Characteristics 1",
                $"0x{characteristics1:X2}",
                0x0B,
                1);
            AddBitFields(
                structure,
                0x0B,
                1,
                characteristics1,
                (0, "Characteristics unknown"),
                (1, "Provides 5.0 volts"),
                (2, "Provides 3.3 volts"),
                (3, "Slot opening shared with another slot"),
                (4, "PC Card-16 supported"),
                (5, "CardBus supported"),
                (6, "Zoom Video supported"),
                (7, "Modem Ring Resume supported"));
        }

        if (TryByte(structure, 0x0C, out var characteristics2))
        {
            Add(
                structure,
                "Slot Characteristics 2",
                $"0x{characteristics2:X2}",
                0x0C,
                1);
            AddBitFields(
                structure,
                0x0C,
                1,
                characteristics2,
                (0, "PME# supported"),
                (1, "Hot-plug supported"),
                (2, "SMBus supported"),
                (3, "PCIe bifurcation supported"),
                (4, "Async/surprise removal supported"),
                (5, "CXL 1.0 capable"),
                (6, "CXL 2.0 capable"),
                (7, "CXL 3.0 capable"));
        }

        if (TryU16(structure, 0x0D, out var segment) &&
            TryByte(structure, 0x0F, out var bus) &&
            TryByte(structure, 0x10, out var deviceFunction))
        {
            Add(
                structure,
                "PCI Segment / Bus / Device / Function",
                $"Segment {segment}, Bus {bus}, Device {deviceFunction >> 3}, Function {deviceFunction & 0x07}",
                0x0D,
                4);
        }

        if (TryByte(structure, 0x11, out var baseWidth))
        {
            Add(
                structure,
                "Data Bus Width (Base)",
                Describe(baseWidth, SmbiosEnumCatalog.SlotWidths),
                0x11,
                1);
        }

        if (TryByte(structure, 0x12, out var peerCount))
        {
            Add(structure, "Peer Group Count", peerCount.ToString(Invariant), 0x12, 1);

            var dynamicOffset = 0x13 + (peerCount * 5);
            if (TryByte(structure, dynamicOffset, out var slotInformation))
            {
                Add(structure, "Slot Information", slotInformation.ToString(Invariant), dynamicOffset, 1);
            }

            if (TryByte(structure, dynamicOffset + 1, out var physicalWidth))
            {
                Add(
                    structure,
                    "Slot Physical Width",
                    Describe(physicalWidth, SmbiosEnumCatalog.SlotWidths),
                    dynamicOffset + 1,
                    1);
            }

            if (TryU16(structure, dynamicOffset + 2, out var pitch))
            {
                Add(
                    structure,
                    "Slot Pitch",
                    pitch == 0 ? "Not given" : $"{pitch / 100m:0.##} mm",
                    dynamicOffset + 2,
                    2);
            }

            AddEnumByte(
                structure,
                "Slot Height",
                dynamicOffset + 4,
                SmbiosEnumCatalog.SlotHeights);
        }
    }

    private static void ParseOemStrings(SmbiosStructure structure)
    {
        if (!TryByte(structure, 0x04, out var count))
        {
            return;
        }

        Add(structure, "Declared String Count", count.ToString(Invariant), 0x04, 1);
        Add(
            structure,
            "Parsed String Count",
            structure.Strings.Count.ToString(Invariant),
            0x04,
            1,
            count == structure.Strings.Count ? null : "声明数量与字符串集实际数量不一致");

        for (var index = 0; index < Math.Min(count, structure.Strings.Count); index++)
        {
            var item = structure.Strings[index];
            Add(structure, $"OEM String {index + 1}", item.Value, item.Offset, item.Value.Length + 1);
        }
    }

    private static void ParseFirmwareLanguage(SmbiosStructure structure)
    {
        if (!TryByte(structure, 0x04, out var installableLanguages))
        {
            return;
        }

        Add(
            structure,
            "Installable Languages",
            installableLanguages.ToString(Invariant),
            0x04,
            1);

        var abbreviated = false;
        if (TryByte(structure, 0x05, out var flags))
        {
            abbreviated = (flags & 0x01) != 0;
            Add(
                structure,
                "Flags",
                $"0x{flags:X2}",
                0x05,
                1);
            AddBitFields(
                structure,
                0x05,
                1,
                flags,
                (0, "Abbreviated language string format"),
                (1, "Reserved"),
                (2, "Reserved"),
                (3, "Reserved"),
                (4, "Reserved"),
                (5, "Reserved"),
                (6, "Reserved"),
                (7, "Reserved"));
        }

        if (structure.Length >= 0x15)
        {
            Add(structure, "Reserved", "15 bytes", 0x06, 15);
        }

        AddString(structure, "Current Language", 0x15);

        var languageCount = Math.Min(installableLanguages, structure.Strings.Count);
        for (var index = 0; index < languageCount; index++)
        {
            var item = structure.Strings[index];
            Add(
                structure,
                $"Language {index + 1}",
                item.Value,
                item.Offset,
                item.Value.Length + 1,
                DecodeLanguage(item.Value, abbreviated));
        }

        if (installableLanguages != structure.Strings.Count)
        {
            Add(
                structure,
                "Language String Count",
                $"Declared {installableLanguages}, parsed {structure.Strings.Count}",
                0x04,
                1,
                "Declared count does not match the number of strings in the structure");
        }
    }

    private static string DecodeLanguage(string value, bool abbreviated)
    {
        if (abbreviated)
        {
            return value.Length == 4
                ? $"{value[..2]}-{value[2..4]}"
                : "Malformed abbreviated language string";
        }

        var parts = value.Split('|');
        return parts.Length >= 3
            ? $"{parts[0]}-{parts[1]} · {parts[2]}"
            : "Malformed long-format language string";
    }

    private static void ParsePhysicalMemoryArray(SmbiosStructure structure)
    {
        AddEnumByte(structure, "Location", 0x04, SmbiosEnumCatalog.MemoryArrayLocations);
        AddEnumByte(structure, "Use", 0x05, SmbiosEnumCatalog.MemoryArrayUses);
        AddEnumByte(structure, "Memory Error Correction", 0x06, SmbiosEnumCatalog.MemoryErrorCorrections);

        if (TryU32(structure, 0x07, out var maximumCapacityKiB))
        {
            if (maximumCapacityKiB == 0x80000000 &&
                TryU64(structure, 0x0F, out var extendedCapacityBytes))
            {
                Add(
                    structure,
                    "Maximum Capacity",
                    FormatByteSize(extendedCapacityBytes),
                    0x0F,
                    8,
                    "Maximum Capacity 为 8000 0000h，使用扩展容量");
            }
            else
            {
                Add(
                    structure,
                    "Maximum Capacity",
                    FormatByteSize((ulong)maximumCapacityKiB * 1024),
                    0x07,
                    4);
            }
        }

        if (TryU16(structure, 0x0B, out var errorHandle))
        {
            Add(
                structure,
                "Memory Error Information Handle",
                FormatErrorHandle(errorHandle),
                0x0B,
                2);
        }

        if (TryU16(structure, 0x0D, out var deviceCount))
        {
            Add(structure, "Number of Memory Devices", deviceCount.ToString(Invariant), 0x0D, 2);
        }
    }

    private static void ParseMemoryDevice(SmbiosStructure structure)
    {
        if (TryU16(structure, 0x04, out var arrayHandle))
        {
            Add(structure, "Physical Memory Array Handle", $"0x{arrayHandle:X4}", 0x04, 2);
        }

        if (TryU16(structure, 0x06, out var errorHandle))
        {
            Add(structure, "Memory Error Information Handle", FormatErrorHandle(errorHandle), 0x06, 2);
        }

        if (TryU16(structure, 0x08, out var totalWidth))
        {
            Add(structure, "Total Width", totalWidth == 0xFFFF ? "Unknown" : $"{totalWidth} bits", 0x08, 2);
        }

        if (TryU16(structure, 0x0A, out var dataWidth))
        {
            Add(structure, "Data Width", dataWidth == 0xFFFF ? "Unknown" : $"{dataWidth} bits", 0x0A, 2);
        }

        if (TryU16(structure, 0x0C, out var size))
        {
            Add(
                structure,
                "Size",
                DecodeMemoryDeviceSize(structure, size),
                0x0C,
                2);
        }

        if (TryU32(structure, 0x1C, out var extendedSize))
        {
            Add(
                structure,
                "Extended Size",
                $"0x{extendedSize:X8} · {FormatByteSize((ulong)(extendedSize & 0x7FFFFFFF) * 1024UL * 1024UL)}",
                0x1C,
                4,
                "仅当 Size 为 7FFFh 时用于表示实际容量");
        }

        AddEnumByte(structure, "Form Factor", 0x0E, SmbiosEnumCatalog.MemoryFormFactors);

        if (TryByte(structure, 0x0F, out var deviceSet))
        {
            Add(
                structure,
                "Device Set",
                deviceSet switch
                {
                    0 => "Not part of a set",
                    0xFF => "Unknown",
                    _ => deviceSet.ToString(Invariant)
                },
                0x0F,
                1);
        }

        AddString(structure, "Device Locator", 0x10);
        AddString(structure, "Bank Locator", 0x11);
        AddEnumByte(structure, "Memory Type", 0x12, SmbiosEnumCatalog.MemoryTypes);

        if (TryU16(structure, 0x13, out var typeDetail))
        {
            Add(
                structure,
                "Type Detail",
                $"0x{typeDetail:X4}",
                0x13,
                2);
            AddBitFields(
                structure,
                0x13,
                2,
                typeDetail,
                (0, "Reserved"),
                (1, "Other"),
                (2, "Unknown"),
                (3, "Fast-paged"),
                (4, "Static column"),
                (5, "Pseudo-static"),
                (6, "RAMBUS"),
                (7, "Synchronous"),
                (8, "CMOS"),
                (9, "EDO"),
                (10, "Window DRAM"),
                (11, "Cache DRAM"),
                (12, "Non-volatile"),
                (13, "Registered (Buffered)"),
                (14, "Unbuffered (Unregistered)"),
                (15, "LRDIMM"));
        }

        if (TryU16(structure, 0x15, out var speed))
        {
            Add(
                structure,
                "Speed",
                DecodeMemorySpeed(structure, speed, 0x54),
                0x15,
                2);
        }

        if (TryU32(structure, 0x54, out var extendedSpeed))
        {
            Add(
                structure,
                "Extended Speed",
                $"0x{extendedSpeed:X8} · {(extendedSpeed & 0x7FFFFFFF)} MT/s",
                0x54,
                4,
                "仅当 Speed 为 FFFFh 时用于表示实际速度");
        }

        AddString(structure, "Manufacturer", 0x17);
        AddString(structure, "Serial Number", 0x18);
        AddString(structure, "Asset Tag", 0x19);
        AddString(structure, "Part Number", 0x1A);

        if (TryByte(structure, 0x1B, out var attributes))
        {
            var rank = attributes & 0x0F;
            Add(
                structure,
                "Attributes",
                $"0x{attributes:X2}",
                0x1B,
                1,
                $"Rank: {(rank == 0 ? "Unknown" : rank.ToString(Invariant))}; Bits 7:4 reserved");
        }

        if (TryU16(structure, 0x20, out var configuredSpeed))
        {
            Add(
                structure,
                "Configured Memory Speed",
                DecodeMemorySpeed(structure, configuredSpeed, 0x58),
                0x20,
                2);
        }

        if (TryU32(structure, 0x58, out var extendedConfiguredSpeed))
        {
            Add(
                structure,
                "Extended Configured Memory Speed",
                $"0x{extendedConfiguredSpeed:X8} · {(extendedConfiguredSpeed & 0x7FFFFFFF)} MT/s",
                0x58,
                4,
                "仅当 Configured Memory Speed 为 FFFFh 时用于表示实际速度");
        }

        AddVoltage(structure, "Minimum Voltage", 0x22);
        AddVoltage(structure, "Maximum Voltage", 0x24);
        AddVoltage(structure, "Configured Voltage", 0x26);
        AddEnumByte(structure, "Memory Technology", 0x28, SmbiosEnumCatalog.MemoryTechnologies);

        if (TryU16(structure, 0x29, out var operatingModes))
        {
            Add(
                structure,
                "Memory Operating Mode Capability",
                $"0x{operatingModes:X4}",
                0x29,
                2);
            AddBitFields(
                structure,
                0x29,
                2,
                operatingModes,
                (0, "Reserved"),
                (1, "Other"),
                (2, "Unknown"),
                (3, "Volatile memory"),
                (4, "Byte-accessible persistent memory"),
                (5, "Block-accessible persistent memory"),
                (6, "Reserved"),
                (7, "Reserved"),
                (8, "Reserved"),
                (9, "Reserved"),
                (10, "Reserved"),
                (11, "Reserved"),
                (12, "Reserved"),
                (13, "Reserved"),
                (14, "Reserved"),
                (15, "Reserved"));
        }

        AddString(structure, "Firmware Version", 0x2B);
        AddId(structure, "Module Manufacturer ID", 0x2C);
        AddId(structure, "Module Product ID", 0x2E);
        AddId(structure, "Memory Subsystem Controller Manufacturer ID", 0x30);
        AddId(structure, "Memory Subsystem Controller Product ID", 0x32);
        AddSize64(structure, "Non-volatile Size", 0x34);
        AddSize64(structure, "Volatile Size", 0x3C);
        AddSize64(structure, "Cache Size", 0x44);
        AddSize64(structure, "Logical Size", 0x4C);
        AddId(structure, "PMIC0 Manufacturer ID", 0x5C);
        AddId(structure, "PMIC0 Revision Number", 0x5E);
        AddId(structure, "RCD Manufacturer ID", 0x60);
        AddId(structure, "RCD Revision Number", 0x62);
    }

    private static void ParseMemoryError(SmbiosStructure structure)
    {
        AddEnumByte(structure, "Error Type", 0x04, SmbiosEnumCatalog.MemoryErrorTypes);
        AddEnumByte(structure, "Error Granularity", 0x05, SmbiosEnumCatalog.MemoryErrorGranularities);
        AddEnumByte(structure, "Error Operation", 0x06, SmbiosEnumCatalog.MemoryErrorOperations);

        if (TryU32(structure, 0x07, out var syndrome))
        {
            Add(structure, "Vendor Syndrome", $"0x{syndrome:X8}", 0x07, 4);
        }

        AddMemoryErrorAddress(structure, "Memory Array Error Address", 0x0B);
        AddMemoryErrorAddress(structure, "Device Error Address", 0x0F);
        AddMemoryErrorAddress(structure, "Error Resolution", 0x13);
    }

    private static void ParseBootInformation(SmbiosStructure structure)
    {
        if (structure.Length >= 0x0A)
        {
            Add(structure, "Reserved", "6 bytes", 0x04, 6);
        }

        if (!TryByte(structure, 0x0A, out var status))
        {
            return;
        }

        Add(
            structure,
            "Boot Status",
            Describe(status, SmbiosEnumCatalog.BootStatuses),
            0x0A,
            1);

        var additionalOffset = 0x0B;
        var additionalLength = structure.Length - additionalOffset;
        if (additionalLength > 0)
        {
            Add(
                structure,
                "Additional Data",
                Convert.ToHexString(
                    structure.FormattedArea.Span.Slice(additionalOffset, additionalLength)),
                additionalOffset,
                additionalLength);
        }
    }

    private static void ParseTpmDevice(SmbiosStructure structure)
    {
        if (structure.FormattedArea.Length >= 0x08)
        {
            var bytes = structure.FormattedArea.Span.Slice(0x04, 4);
            var terminator = bytes.IndexOf((byte)0);
            if (terminator < 0)
            {
                terminator = bytes.Length;
            }

            Add(
                structure,
                "Vendor ID",
                Encoding.ASCII.GetString(bytes[..terminator]),
                0x04,
                4);
        }

        if (TryByte(structure, 0x08, out var major))
        {
            Add(structure, "Major Spec Version", major.ToString(Invariant), 0x08, 1);
        }

        if (TryByte(structure, 0x09, out var minor))
        {
            Add(structure, "Minor Spec Version", minor.ToString(Invariant), 0x09, 1);
        }

        if (TryU32(structure, 0x0A, out var firmware1))
        {
            Add(structure, "Firmware Version 1", $"0x{firmware1:X8}", 0x0A, 4);
        }

        if (TryU32(structure, 0x0E, out var firmware2))
        {
            Add(structure, "Firmware Version 2", $"0x{firmware2:X8}", 0x0E, 4);
        }

        AddString(structure, "Description", 0x12);

        if (TryU64(structure, 0x13, out var characteristics))
        {
            Add(
                structure,
                "Characteristics",
                $"0x{characteristics:X16}",
                0x13,
                8);
            AddBitFields(
                structure,
                0x13,
                8,
                characteristics,
                (0, "Reserved"),
                (1, "Reserved"),
                (2, "TPM Device Characteristics are not supported"),
                (3, "Family configurable via firmware update"),
                (4, "Family configurable via platform software support"),
                (5, "Family configurable via OEM proprietary mechanism"));
        }

        if (TryU32(structure, 0x1B, out var oemDefined))
        {
            Add(structure, "OEM-defined", $"0x{oemDefined:X8}", 0x1B, 4);
        }
    }

    private static void AddCacheSize(
        SmbiosStructure structure,
        string name,
        int offset,
        int extendedOffset)
    {
        if (!TryU16(structure, offset, out var raw))
        {
            return;
        }

        if (raw == 0xFFFF && TryU32(structure, extendedOffset, out var extended))
        {
            Add(
                structure,
                name,
                "0xFFFF · Use 32-bit extended field",
                offset,
                2,
                $"扩展字段解析为 {DecodeCacheSize(extended, isExtended: true)}");
        }
        else
        {
            Add(structure, name, DecodeCacheSize(raw, isExtended: false), offset, 2);
        }

        if (TryU32(structure, extendedOffset, out var rawExtended))
        {
            Add(
                structure,
                $"{name} 2",
                $"0x{rawExtended:X8} · {DecodeCacheSize(rawExtended, isExtended: true)}",
                extendedOffset,
                4,
                "32-bit 扩展字段；仅当对应的 16-bit 字段为 FFFFh 时用于表示实际容量");
        }
    }

    private static string DecodeCacheSize(uint raw, bool isExtended)
    {
        var granularity = isExtended ? (raw >> 31) & 1 : (raw >> 15) & 1;
        var count = isExtended ? raw & 0x7FFFFFFF : raw & 0x7FFF;
        var unit = granularity == 0 ? 1024UL : 64UL * 1024UL;
        return FormatByteSize(count * unit);
    }

    private static string DecodeProcessorVoltage(byte value)
    {
        if (value == 0)
        {
            return "Unused";
        }

        if ((value & 0x80) != 0)
        {
            return $"{(value & 0x7F) / 10m:0.0} V";
        }

        return DescribeBits(value,
        [
            (0, "5 V supported"),
            (1, "3.3 V supported"),
            (2, "2.9 V supported")
        ]);
    }

    private static string DecodeProcessorStatus(byte status)
    {
        var state = status & 0x07;
        var populated = (status & 0x40) != 0;
        var stateText = state switch
        {
            0 => "Unknown",
            1 => "Enabled",
            2 => "Disabled by user through firmware setup",
            3 => "Disabled by firmware (POST error)",
            4 => "Idle, waiting to be enabled",
            7 => "Other",
            _ => $"Reserved ({state})"
        };

        return $"{stateText}; socket {(populated ? "populated" : "unpopulated")}";
    }

    private static string DecodeMemoryDeviceSize(SmbiosStructure structure, ushort size)
    {
        if (size == 0)
        {
            return "No memory device installed";
        }

        if (size == 0xFFFF)
        {
            return "Unknown";
        }

        if (size == 0x7FFF &&
            TryU32(structure, 0x1C, out var extendedSize))
        {
            return FormatByteSize((ulong)(extendedSize & 0x7FFFFFFF) * 1024UL * 1024UL);
        }

        var megabytes = (size & 0x8000) != 0
            ? (size & 0x7FFF) / 1024d
            : size & 0x7FFF;

        if ((size & 0x8000) != 0)
        {
            return $"{FormatByteSize((ulong)(size & 0x7FFF) * 1024UL)} (KiB granularity)";
        }

        return FormatByteSize((ulong)(size & 0x7FFF) * 1024UL * 1024UL);
    }

    private static string DecodeMemorySpeed(
        SmbiosStructure structure,
        ushort speed,
        int extendedOffset)
    {
        if (speed == 0)
        {
            return "0 MT/s · Unknown";
        }

        if (speed == 0xFFFF &&
            TryU32(structure, extendedOffset, out var extended))
        {
            return $"{(extended & 0x7FFFFFFF).ToString(Invariant)} MT/s";
        }

        return $"{speed.ToString(Invariant)} MT/s";
    }

    private static string DecodeExtendedRomSize(ushort raw)
    {
        var unit = (raw >> 14) & 0x03;
        var size = raw & 0x3FFF;

        return unit switch
        {
            0 => $"{size} MiB",
            1 => $"{size} GiB",
            _ => $"Reserved unit {unit}, size {size}"
        };
    }

    private static string DescribeFirmwareCharacteristics(ulong value) =>
        DescribeBits(value,
        [
            (2, "Unknown"),
            (3, "Firmware characteristics not supported"),
            (4, "ISA supported"),
            (5, "MCA supported"),
            (6, "EISA supported"),
            (7, "PCI supported"),
            (8, "PC Card (PCMCIA) supported"),
            (9, "Plug and Play supported"),
            (10, "APM supported"),
            (11, "Firmware is upgradeable"),
            (12, "Firmware shadowing allowed"),
            (13, "VL-VESA supported"),
            (14, "ESCD supported"),
            (15, "Boot from CD supported"),
            (16, "Selectable boot supported"),
            (17, "Firmware ROM is socketed"),
            (18, "Boot from PC Card supported"),
            (19, "EDD supported"),
            (31, "NEC PC-98")
        ]);

    private static string DescribeSram(ushort value) =>
        DescribeBits(value,
        [
            (0, "Other"),
            (1, "Unknown"),
            (2, "Non-Burst"),
            (3, "Burst"),
            (4, "Pipeline Burst"),
            (5, "Synchronous"),
            (6, "Asynchronous")
        ]);

    private static string DescribeTpmCharacteristics(ulong value) =>
        DescribeBits(value,
        [
            (2, "TPM device characteristics are not supported"),
            (3, "Family configurable via firmware update"),
            (4, "Family configurable via platform software support"),
            (5, "Family configurable via OEM proprietary mechanism")
        ]);

    private static string DescribeBits(ulong value, params (int Bit, string Name)[] bits)
    {
        var names = bits
            .Where(item => (value & (1UL << item.Bit)) != 0)
            .Select(item => item.Name)
            .ToArray();

        return names.Length == 0 ? "None" : string.Join(", ", names);
    }

    private static void AddBitFields(
        SmbiosStructure structure,
        int offset,
        int length,
        ulong value,
        params (int Bit, string Description)[] bits)
    {
        var scope = structure.Fields
            .LastOrDefault(field => field.Offset == offset && !field.IsBitField)
            ?.Name ?? "Bit field";

        foreach (var (bit, description) in bits)
        {
            var bitValue = (value & (1UL << bit)) == 0 ? 0 : 1;
            structure.AddField(new SmbiosField(
                $"{scope}: {description}",
                $"{bitValue} ({(bitValue == 0 ? "No" : "Yes")})",
                offset,
                length,
                description)
            {
                BitIndex = bit,
                BitValue = bitValue
            });
        }
    }

    private static string Describe(byte value, IReadOnlyDictionary<byte, string> values) =>
        values.TryGetValue(value, out var name)
            ? $"{name} (0x{value:X2})"
            : $"Unrecognized (0x{value:X2})";

    private static string Describe(ushort value, IReadOnlyDictionary<ushort, string> values) =>
        values.TryGetValue(value, out var name)
            ? $"{name} (0x{value:X4})"
            : $"Unrecognized (0x{value:X4})";

    private static string FormatErrorHandle(ushort handle) =>
        handle switch
        {
            0xFFFE => "0xFFFE · No error-information structure provided",
            0xFFFF => "0xFFFF · No error detected",
            _ => $"0x{handle:X4}"
        };

    private static string FormatByteSize(ulong bytes)
    {
        string[] units = ["B", "KiB", "MiB", "GiB", "TiB"];
        var unit = 0;

        while (bytes >= (ulong)Math.Pow(1024, unit + 1) &&
               unit < units.Length - 1)
        {
            unit++;
        }

        var scaled = bytes / Math.Pow(1024, unit);
        return unit == 0
            ? $"{bytes.ToString(Invariant)} B"
            : $"{scaled.ToString("0.###", Invariant)} {units[unit]}";
    }

    private static void AddMemoryErrorAddress(
        SmbiosStructure structure,
        string name,
        int offset)
    {
        if (!TryU32(structure, offset, out var value))
        {
            return;
        }

        Add(
            structure,
            name,
            value == 0x80000000
                ? "Unknown (8000 0000h)"
                : $"0x{value:X8}",
            offset,
            4);
    }

    private static void AddId(SmbiosStructure structure, string name, int offset)
    {
        if (!TryU16(structure, offset, out var value))
        {
            return;
        }

        Add(
            structure,
            name,
            value == 0 ? "Unknown (0000h)" : $"0x{value:X4}",
            offset,
            2);
    }

    private static void AddSize64(SmbiosStructure structure, string name, int offset)
    {
        if (!TryU64(structure, offset, out var value))
        {
            return;
        }

        Add(
            structure,
            name,
            value == ulong.MaxValue ? "Unknown" : FormatByteSize(value),
            offset,
            8);
    }

    private static void AddVoltage(SmbiosStructure structure, string name, int offset)
    {
        if (!TryU16(structure, offset, out var millivolts))
        {
            return;
        }

        Add(
            structure,
            name,
            millivolts == 0 ? "0 mV · Unknown" : $"{millivolts} mV",
            offset,
            2);
    }

    private static void AddFrequency(SmbiosStructure structure, string name, int offset)
    {
        if (!TryU16(structure, offset, out var mhz))
        {
            return;
        }

        Add(
            structure,
            name,
            mhz == 0 ? "Unknown" : $"{mhz} MHz",
            offset,
            2);
    }

    private static void AddHandle(SmbiosStructure structure, string name, int offset)
    {
        if (!TryU16(structure, offset, out var handle))
        {
            return;
        }

        Add(
            structure,
            name,
            handle == 0xFFFF ? "0xFFFF · Not provided" : $"0x{handle:X4}",
            offset,
            2);
    }

    private static void AddReleaseByte(SmbiosStructure structure, string name, int offset)
    {
        if (!TryByte(structure, offset, out var value))
        {
            return;
        }

        Add(
            structure,
            name,
            value == 0xFF ? "Not supported (FFh)" : value.ToString(Invariant),
            offset,
            1);
    }

    private static void AddCountByte(SmbiosStructure structure, string name, int offset)
    {
        if (!TryByte(structure, offset, out var value))
        {
            return;
        }

        Add(structure, name, value == 0 ? "Unknown" : value.ToString(Invariant), offset, 1);
    }

    private static void AddExtendedCount(
        SmbiosStructure structure,
        string name,
        int offset)
    {
        if (!TryU16(structure, offset, out var value))
        {
            return;
        }

        Add(
            structure,
            name,
            value == 0xFFFF
                ? "Reserved (FFFFh)"
                : value == 0
                    ? "Unknown"
                    : value.ToString(Invariant),
            offset,
            2,
            "支持超过 255 的计数；主字段为 FFh 时使用该字段");
    }

    private static void AddEnumByte(
        SmbiosStructure structure,
        string name,
        int offset,
        IReadOnlyDictionary<byte, string> values)
    {
        if (TryByte(structure, offset, out var value))
        {
            Add(structure, name, Describe(value, values), offset, 1);
        }
    }

    private static void AddString(SmbiosStructure structure, string name, int offset)
    {
        if (!TryByte(structure, offset, out var stringIndex))
        {
            return;
        }

        var value = stringIndex switch
        {
            0 => "(not set)",
            _ when stringIndex <= structure.Strings.Count =>
                structure.Strings[stringIndex - 1].Value,
            _ => $"Invalid string index {stringIndex}"
        };

        Add(
            structure,
            name,
            value,
            offset,
            1,
            stringIndex == 0
                ? "String index 0"
                : $"String index {stringIndex}");
    }

    private static void Add(
        SmbiosStructure structure,
        string name,
        string value,
        int offset,
        int length,
        string? note = null,
        int? bitIndex = null,
        int? bitValue = null)
    {
        structure.AddField(new SmbiosField(name, value, offset, length, note)
        {
            BitIndex = bitIndex,
            BitValue = bitValue
        });
    }

    private static bool TryByte(SmbiosStructure structure, int offset, out byte value)
    {
        if (offset >= 0 && offset < structure.FormattedArea.Length)
        {
            value = structure.FormattedArea.Span[offset];
            return true;
        }

        value = 0;
        return false;
    }

    private static bool TryU16(SmbiosStructure structure, int offset, out ushort value)
    {
        if (offset >= 0 && offset + 2 <= structure.FormattedArea.Length)
        {
            value = BinaryPrimitives.ReadUInt16LittleEndian(
                structure.FormattedArea.Span.Slice(offset, 2));
            return true;
        }

        value = 0;
        return false;
    }

    private static bool TryU32(SmbiosStructure structure, int offset, out uint value)
    {
        if (offset >= 0 && offset + 4 <= structure.FormattedArea.Length)
        {
            value = BinaryPrimitives.ReadUInt32LittleEndian(
                structure.FormattedArea.Span.Slice(offset, 4));
            return true;
        }

        value = 0;
        return false;
    }

    private static bool TryU64(SmbiosStructure structure, int offset, out ulong value)
    {
        if (offset >= 0 && offset + 8 <= structure.FormattedArea.Length)
        {
            value = BinaryPrimitives.ReadUInt64LittleEndian(
                structure.FormattedArea.Span.Slice(offset, 8));
            return true;
        }

        value = 0;
        return false;
    }
}
