using System.Globalization;
using FirmwareInspector.Core.Models;

namespace FirmwareInspector.Core.Parsing;

internal static partial class SmbiosFieldParser
{
    private static void ParseMemoryController(SmbiosStructure structure)
    {
        AddEnumByte(
            structure,
            "Error Detecting Method",
            0x04,
            SmbiosEnumCatalog.MemoryControllerErrorDetectMethods);
        AddByteFlags(
            structure,
            "Error Correcting Capability",
            0x05,
            [
                (0, "Other"),
                (1, "Unknown"),
                (2, "None"),
                (3, "Single-bit Error Correcting"),
                (4, "Double-bit Error Correcting"),
                (5, "Error Scrubbing")
            ]);
        AddEnumByte(structure, "Supported Interleave", 0x06, SmbiosEnumCatalog.InterleaveSupport);
        AddEnumByte(structure, "Current Interleave", 0x07, SmbiosEnumCatalog.InterleaveSupport);

        if (TryByte(structure, 0x08, out var maximumModuleSize))
        {
            var size = maximumModuleSize >= 63
                ? $"2^{maximumModuleSize} MiB"
                : $"{1UL << maximumModuleSize} MiB";
            Add(structure, "Maximum Memory Module Size", size, 0x08, 1);
        }

        AddWordFlags(
            structure,
            "Supported Speeds",
            0x09,
            [
                (0, "Other"),
                (1, "Unknown"),
                (2, "70 ns"),
                (3, "60 ns"),
                (4, "50 ns")
            ]);
        AddWordFlags(
            structure,
            "Supported Memory Types",
            0x0B,
            MemoryModuleTypeFlags());
        AddByteFlags(
            structure,
            "Memory Module Voltage",
            0x0D,
            [
                (0, "5 V"),
                (1, "3.3 V"),
                (2, "2.9 V")
            ]);

        if (!TryByte(structure, 0x0E, out var slotCount))
        {
            return;
        }

        Add(structure, "Number of Associated Memory Slots", slotCount.ToString(Invariant), 0x0E, 1);
        var handles = new List<string>();

        for (var index = 0; index < slotCount; index++)
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
            Add(structure, "Memory Module Configuration Handles", string.Join(", ", handles), 0x0F, handles.Count * 2);
        }

        var enabledEccOffset = 0x0F + (slotCount * 2);
        if (TryByte(structure, enabledEccOffset, out var enabledEcc))
        {
            Add(
                structure,
                "Enabled Error Correcting Capabilities",
                $"0x{enabledEcc:X2}",
                enabledEccOffset,
                1,
                DescribeBits(enabledEcc,
                [
                    (0, "Other"),
                    (1, "Unknown"),
                    (2, "None"),
                    (3, "Single-bit Error Correcting"),
                    (4, "Double-bit Error Correcting"),
                    (5, "Error Scrubbing")
                ]));
        }
    }

    private static void ParseMemoryModule(SmbiosStructure structure)
    {
        AddString(structure, "Socket Designation", 0x04);

        if (TryByte(structure, 0x05, out var bankConnections))
        {
            Add(
                structure,
                "Bank Connections",
                $"0x{bankConnections:X2}",
                0x05,
                1,
                $"RAS bank A: {Nibble(bankConnections & 0x0F)}, RAS bank B: {Nibble((bankConnections >> 4) & 0x0F)}");
        }

        if (TryByte(structure, 0x06, out var speed))
        {
            Add(structure, "Current Speed", speed == 0 ? "Unknown" : $"{speed} ns", 0x06, 1);
        }

        AddWordFlags(
            structure,
            "Current Memory Type",
            0x07,
            MemoryModuleTypeFlags());
        AddModuleSize(structure, "Installed Size", 0x09);
        AddModuleSize(structure, "Enabled Size", 0x0A);

        if (TryByte(structure, 0x0B, out var errorStatus))
        {
            Add(
                structure,
                "Error Status",
                $"0x{errorStatus:X2}",
                0x0B,
                1);
            AddBitFields(
                structure,
                0x0B,
                1,
                errorStatus,
                (0, "Uncorrectable errors received"),
                (1, "Correctable errors received"),
                (2, "Error information available from event log"),
                (3, "Reserved"),
                (4, "Reserved"),
                (5, "Reserved"),
                (6, "Reserved"),
                (7, "Reserved"));
        }
    }

    private static void ParseOnboardDevices(SmbiosStructure structure)
    {
        var count = Math.Max(0, (structure.Length - 4) / 2);
        Add(structure, "Number of Devices", count.ToString(Invariant), 0x04, 0);

        for (var index = 0; index < count; index++)
        {
            var typeOffset = 0x04 + (index * 2);
            var stringOffset = typeOffset + 1;

            if (!TryByte(structure, typeOffset, out var rawType))
            {
                break;
            }

            var deviceType = (byte)(rawType & 0x7F);
            var enabled = (rawType & 0x80) != 0;
            Add(
                structure,
                $"Device {index + 1} Type",
                Describe(deviceType, SmbiosEnumCatalog.OnboardDeviceTypes),
                typeOffset,
                1);
            structure.AddField(new SmbiosField(
                "Device enabled [7]",
                $"{(enabled ? 1 : 0)} ({(enabled ? "Yes" : "No")})",
                typeOffset,
                1,
                enabled ? "Device enabled" : "Device disabled")
            {
                BitIndex = 7,
                BitValue = enabled ? 1 : 0
            });
            AddString(structure, $"Device {index + 1} Description", stringOffset);
        }
    }

    private static void ParseSystemConfigurationOptions(SmbiosStructure structure)
    {
        if (!TryByte(structure, 0x04, out var count))
        {
            return;
        }

        Add(structure, "Count", count.ToString(Invariant), 0x04, 1);
        var parsed = Math.Min(count, structure.Strings.Count);
        for (var index = 0; index < parsed; index++)
        {
            var item = structure.Strings[index];
            Add(structure, $"Option {index + 1}", item.Value, item.Offset, item.Value.Length + 1);
        }
    }

    private static void ParseGroupAssociations(SmbiosStructure structure)
    {
        AddString(structure, "Group Name", 0x04);
        var count = Math.Max(0, (structure.Length - 5) / 3);
        Add(structure, "Item Count", count.ToString(Invariant), 0x05, 0);

        for (var index = 0; index < count; index++)
        {
            var offset = 0x05 + (index * 3);
            if (!TryByte(structure, offset, out var itemType) ||
                !TryU16(structure, offset + 1, out var itemHandle))
            {
                break;
            }

            Add(
                structure,
                $"Item {index + 1}",
                $"Type {itemType} · 0x{itemType:X2}, Handle 0x{itemHandle:X4}",
                offset,
                3,
                SmbiosTypeCatalog.GetName(itemType));
        }
    }

    private static void ParseSystemEventLog(SmbiosStructure structure)
    {
        if (TryU16(structure, 0x04, out var areaLength))
        {
            Add(structure, "Log Area Length", $"{areaLength} bytes", 0x04, 2);
        }

        if (TryU16(structure, 0x06, out var headerStart))
        {
            Add(structure, "Log Header Start Offset", $"0x{headerStart:X4}", 0x06, 2);
        }

        if (TryU16(structure, 0x08, out var dataStart))
        {
            Add(structure, "Log Data Start Offset", $"0x{dataStart:X4}", 0x08, 2);
        }

        if (TryByte(structure, 0x0A, out var accessMethod))
        {
            Add(structure, "Access Method", DescribeEventLogAccessMethod(accessMethod), 0x0A, 1);
        }

        if (TryByte(structure, 0x0B, out var status))
        {
            Add(
                structure,
                "Log Status",
                $"0x{status:X2}",
                0x0B,
                1);
            AddBitFields(
                structure,
                0x0B,
                1,
                status,
                (0, "Log area valid"),
                (1, "Log area full"),
                (2, "Reserved"),
                (3, "Reserved"),
                (4, "Reserved"),
                (5, "Reserved"),
                (6, "Reserved"),
                (7, "Reserved"));
        }

        if (TryU32(structure, 0x0C, out var token))
        {
            Add(structure, "Log Change Token", $"0x{token:X8}", 0x0C, 4);
        }

        if (TryU32(structure, 0x10, out var accessAddress))
        {
            Add(structure, "Access Method Address", $"0x{accessAddress:X8}", 0x10, 4, DescribeAccessAddress(accessMethod, accessAddress));
        }

        if (TryByte(structure, 0x14, out var headerFormat))
        {
            Add(structure, "Log Header Format", DescribeLogHeaderFormat(headerFormat), 0x14, 1);
        }

        if (!TryByte(structure, 0x15, out var descriptorCount) ||
            !TryByte(structure, 0x16, out var descriptorLength))
        {
            return;
        }

        Add(structure, "Supported Log Type Descriptors", descriptorCount.ToString(Invariant), 0x15, 1);
        Add(structure, "Descriptor Length", descriptorLength.ToString(Invariant), 0x16, 1);

        for (var index = 0; index < descriptorCount; index++)
        {
            var offset = 0x17 + (index * descriptorLength);
            if (descriptorLength < 2 ||
                !TryByte(structure, offset, out var logType) ||
                !TryByte(structure, offset + 1, out var format))
            {
                break;
            }

            Add(
                structure,
                $"Log Type {index + 1}",
                $"Type 0x{logType:X2} · Data format 0x{format:X2}",
                offset,
                descriptorLength,
                $"{DescribeEventLogType(logType)}; {DescribeEventLogDataFormat(format)}");
        }
    }

    private static void ParseMemoryArrayMappedAddress(SmbiosStructure structure)
    {
        AddAddressRange(structure, "Starting Address", "Ending Address", 0x04, 0x08, 0x0F, 0x17);

        if (TryU16(structure, 0x0C, out var arrayHandle))
        {
            Add(structure, "Memory Array Handle", $"0x{arrayHandle:X4}", 0x0C, 2);
        }

        if (TryByte(structure, 0x0E, out var partitionWidth))
        {
            Add(structure, "Partition Width", partitionWidth.ToString(Invariant), 0x0E, 1);
        }
    }

    private static void ParseMemoryDeviceMappedAddress(SmbiosStructure structure)
    {
        AddAddressRange(structure, "Starting Address", "Ending Address", 0x04, 0x08, 0x13, 0x1B);
        AddHandle(structure, "Memory Device Handle", 0x0C);
        AddHandle(structure, "Memory Array Mapped Address Handle", 0x0E);

        AddByteUnknownSpecial(structure, "Partition Row Position", 0x10, 0xFF);
        AddByteUnknownSpecial(structure, "Interleave Position", 0x11, 0xFF);
        AddByteUnknownSpecial(structure, "Interleaved Data Depth", 0x12, 0xFF);
    }

    private static void ParsePointingDevice(SmbiosStructure structure)
    {
        AddEnumByte(structure, "Pointing Device Type", 0x04, SmbiosEnumCatalog.PointingDeviceTypes);
        AddEnumByte(structure, "Interface Type", 0x05, SmbiosEnumCatalog.PointingDeviceInterfaces);

        if (TryByte(structure, 0x06, out var buttons))
        {
            Add(structure, "Number of Buttons", buttons.ToString(Invariant), 0x06, 1);
        }
    }

    private static void ParsePortableBattery(SmbiosStructure structure)
    {
        AddString(structure, "Location", 0x04);
        AddString(structure, "Manufacturer", 0x05);
        AddString(structure, "Manufacture Date", 0x06);
        AddString(structure, "Serial Number", 0x07);
        AddString(structure, "Device Name", 0x08);
        AddEnumByte(structure, "Device Chemistry", 0x09, SmbiosEnumCatalog.BatteryChemistries);

        if (TryU16(structure, 0x0A, out var designCapacity))
        {
            Add(structure, "Design Capacity", designCapacity == 0 ? "Unknown" : $"{designCapacity} mWh", 0x0A, 2);
        }

        if (TryU16(structure, 0x0C, out var designVoltage))
        {
            Add(structure, "Design Voltage", designVoltage == 0 ? "Unknown" : $"{designVoltage} mV", 0x0C, 2);
        }

        AddString(structure, "SBDS Version Number", 0x0E);

        if (TryByte(structure, 0x0F, out var maximumError))
        {
            Add(structure, "Maximum Error in Battery Data", maximumError == 0xFF ? "Unknown" : $"{maximumError}%", 0x0F, 1);
        }

        if (TryU16(structure, 0x10, out var sbdsSerial))
        {
            Add(structure, "SBDS Serial Number", $"0x{sbdsSerial:X4}", 0x10, 2);
        }

        if (TryU16(structure, 0x12, out var sbdsDate))
        {
            var year = 1980 + ((sbdsDate >> 9) & 0x7F);
            var month = (sbdsDate >> 5) & 0x0F;
            var day = sbdsDate & 0x1F;
            var value = month is >= 1 and <= 12 && day is >= 1 and <= 31
                ? $"{year:D4}-{month:D2}-{day:D2}"
                : "Invalid packed date";
            Add(structure, "SBDS Manufacture Date", $"{value} (0x{sbdsDate:X4})", 0x12, 2);
        }

        AddString(structure, "SBDS Device Chemistry", 0x14);

        if (TryByte(structure, 0x15, out var capacityMultiplier))
        {
            Add(
                structure,
                "Design Capacity Multiplier",
                capacityMultiplier == 0 ? "0 (invalid; default is 1)" : capacityMultiplier.ToString(Invariant),
                0x15,
                1);
        }

        if (TryU32(structure, 0x16, out var oemDefined))
        {
            Add(structure, "OEM-specific", $"0x{oemDefined:X8}", 0x16, 4);
        }
    }

    private static void ParseSystemReset(SmbiosStructure structure)
    {
        if (TryByte(structure, 0x04, out var capabilities))
        {
            Add(
                structure,
                "Capabilities",
                $"0x{capabilities:X2}",
                0x04,
                1);
            var resetEnabled = capabilities & 0x01;
            structure.AddField(new SmbiosField(
                "System reset enabled [0]",
                $"{((resetEnabled != 0) ? 1 : 0)} ({((resetEnabled != 0) ? "Yes" : "No")})",
                0x04,
                1,
                "System reset is enabled by the user")
            {
                BitIndex = 0,
                BitValue = resetEnabled != 0 ? 1 : 0
            });
            Add(structure, "Boot Option [2:1]", ResetBootOption((capabilities >> 1) & 0x03), 0x04, 1);
            Add(structure, "Boot Option on Limit [4:3]", ResetBootOption((capabilities >> 3) & 0x03), 0x04, 1);
            var watchdog = (capabilities & 0x20) != 0 ? 1 : 0;
            structure.AddField(new SmbiosField(
                "Watchdog timer present [5]",
                $"{watchdog} ({(watchdog == 1 ? "Yes" : "No")})",
                0x04,
                1,
                "System contains a watchdog timer")
            {
                BitIndex = 5,
                BitValue = watchdog
            });
            Add(structure, "Reserved [7:6]", "0", 0x04, 1);
        }

        AddWordUnknownSpecial(structure, "Reset Count", 0x05, 0xFFFF);
        AddWordUnknownSpecial(structure, "Reset Limit", 0x07, 0xFFFF);
        AddWordUnknownSpecial(structure, "Timer Interval", 0x09, 0xFFFF);
        AddWordUnknownSpecial(structure, "Timeout", 0x0B, 0xFFFF);
    }

    private static void ParseHardwareSecurity(SmbiosStructure structure)
    {
        if (!TryByte(structure, 0x04, out var settings))
        {
            return;
        }

        Add(
            structure,
            "Hardware Security Settings",
            $"0x{settings:X2}",
            0x04,
            1);
        Add(structure, "Power-on Password Status [1:0]", PasswordStatus(settings & 0x03), 0x04, 1);
        Add(structure, "Keyboard Password Status [3:2]", PasswordStatus((settings >> 2) & 0x03), 0x04, 1);
        Add(structure, "Administrator Password Status [5:4]", PasswordStatus((settings >> 4) & 0x03), 0x04, 1);
        Add(structure, "Front Panel Reset Status [7:6]", PasswordStatus((settings >> 6) & 0x03), 0x04, 1);
    }

    private static void ParseSystemPowerControls(SmbiosStructure structure)
    {
        AddBcd(structure, "Next Scheduled Power-on Month", 0x04, 1, 12);
        AddBcd(structure, "Next Scheduled Power-on Day-of-month", 0x05, 1, 31);
        AddBcd(structure, "Next Scheduled Power-on Hour", 0x06, 0, 23);
        AddBcd(structure, "Next Scheduled Power-on Minute", 0x07, 0, 59);
        AddBcd(structure, "Next Scheduled Power-on Second", 0x08, 0, 59);
    }

    private static void ParseVoltageProbe(SmbiosStructure structure)
    {
        AddProbeCommon(structure, "mV", false, false, "V");
    }

    private static void ParseCoolingDevice(SmbiosStructure structure)
    {
        AddHandle(structure, "Temperature Probe Handle", 0x04);

        if (TryByte(structure, 0x06, out var typeStatus))
        {
            var status = (byte)((typeStatus >> 5) & 0x07);
            var type = (byte)(typeStatus & 0x1F);
            Add(
                structure,
                "Device Type and Status",
                Describe(type, SmbiosEnumCatalog.CoolingDeviceTypes),
                0x06,
                1);
            Add(structure, "Status [7:5]", Describe(status, SmbiosEnumCatalog.ProbeStatuses), 0x06, 1);
        }

        if (TryByte(structure, 0x07, out var group))
        {
            Add(structure, "Cooling Unit Group", group.ToString(Invariant), 0x07, 1);
        }

        if (TryU32(structure, 0x08, out var oemDefined))
        {
            Add(structure, "OEM-defined", $"0x{oemDefined:X8}", 0x08, 4);
        }

        AddWordUnknownSpecial(structure, "Nominal Speed", 0x0C, 0x8000, "rpm");
        AddString(structure, "Description", 0x0E);
    }

    private static void ParseTemperatureProbe(SmbiosStructure structure)
    {
        AddProbeCommon(structure, "°C", true, true, "T");
    }

    private static void ParseCurrentProbe(SmbiosStructure structure)
    {
        AddProbeCommon(structure, "mA", false, false, "I");
    }

    private static void ParseOutOfBandRemoteAccess(SmbiosStructure structure)
    {
        AddString(structure, "Manufacturer Name", 0x04);

        if (TryByte(structure, 0x05, out var connections))
        {
            Add(
                structure,
                "Connections",
                $"0x{connections:X2}",
                0x05,
                1);
            AddBitFields(
                structure,
                0x05,
                1,
                connections,
                (0, "Inbound connection enabled"),
                (1, "Outbound connection enabled"),
                (2, "Reserved"),
                (3, "Reserved"),
                (4, "Reserved"),
                (5, "Reserved"),
                (6, "Reserved"),
                (7, "Reserved"));
        }
    }

    private static void ParseBisEntryPoint(SmbiosStructure structure)
    {
        Add(
            structure,
            "字段定义",
            "由 Boot Integrity Services API Specification 定义，DSP0134 仅规定 Type 31",
            0x00,
            0);
    }

    private static void ParseMemoryError64(SmbiosStructure structure)
    {
        AddEnumByte(structure, "Error Type", 0x04, SmbiosEnumCatalog.MemoryErrorTypes);
        AddEnumByte(structure, "Error Granularity", 0x05, SmbiosEnumCatalog.MemoryErrorGranularities);
        AddEnumByte(structure, "Error Operation", 0x06, SmbiosEnumCatalog.MemoryErrorOperations);

        if (TryU32(structure, 0x07, out var syndrome))
        {
            Add(structure, "Vendor Syndrome", $"0x{syndrome:X8}", 0x07, 4);
        }

        AddMemoryErrorAddress64(structure, "Memory Array Error Address", 0x0B);
        AddMemoryErrorAddress64(structure, "Device Error Address", 0x13);
        AddMemoryErrorAddress(structure, "Error Resolution", 0x1B);
    }

    private static void ParseManagementDevice(SmbiosStructure structure)
    {
        AddString(structure, "Description", 0x04);
        AddEnumByte(structure, "Type", 0x05, SmbiosEnumCatalog.ManagementDeviceTypes);

        if (TryU32(structure, 0x06, out var address))
        {
            Add(structure, "Address", $"0x{address:X8}", 0x06, 4);
        }

        AddEnumByte(structure, "Address Type", 0x0A, SmbiosEnumCatalog.ManagementAddressTypes);
    }

    private static void ParseManagementDeviceComponent(SmbiosStructure structure)
    {
        AddString(structure, "Description", 0x04);
        AddHandle(structure, "Management Device Handle", 0x05);
        AddHandle(structure, "Component Handle", 0x07);
        AddHandle(structure, "Threshold Handle", 0x09);
    }

    private static void ParseManagementDeviceThresholdData(SmbiosStructure structure)
    {
        AddThreshold(structure, "Lower Threshold — Non-critical", 0x04);
        AddThreshold(structure, "Upper Threshold — Non-critical", 0x06);
        AddThreshold(structure, "Lower Threshold — Critical", 0x08);
        AddThreshold(structure, "Upper Threshold — Critical", 0x0A);
        AddThreshold(structure, "Lower Threshold — Non-recoverable", 0x0C);
        AddThreshold(structure, "Upper Threshold — Non-recoverable", 0x0E);
    }

    private static void ParseMemoryChannel(SmbiosStructure structure)
    {
        AddEnumByte(structure, "Channel Type", 0x04, SmbiosEnumCatalog.MemoryChannelTypes);

        if (TryByte(structure, 0x05, out var maximumLoad))
        {
            Add(structure, "Maximum Channel Load", maximumLoad.ToString(Invariant), 0x05, 1);
        }

        if (!TryByte(structure, 0x06, out var count))
        {
            return;
        }

        Add(structure, "Memory Device Count", count.ToString(Invariant), 0x06, 1);
        for (var index = 0; index < count; index++)
        {
            var offset = 0x07 + (index * 3);
            if (!TryByte(structure, offset, out var load) ||
                !TryU16(structure, offset + 1, out var handle))
            {
                break;
            }

            Add(
                structure,
                $"Memory Device {index + 1}",
                $"Load {load}, Handle 0x{handle:X4}",
                offset,
                3);
        }
    }

    private static void ParseIpmiDevice(SmbiosStructure structure)
    {
        AddEnumByte(structure, "Interface Type", 0x04, SmbiosEnumCatalog.IpmiInterfaceTypes);

        if (TryByte(structure, 0x05, out var revision))
        {
            Add(
                structure,
                "IPMI Specification Revision",
                $"{(revision >> 4) & 0x0F}.{revision & 0x0F} (0x{revision:X2})",
                0x05,
                1);
        }

        if (TryByte(structure, 0x06, out var targetAddress))
        {
            Add(structure, "I2C Target Address", $"0x{targetAddress:X2}", 0x06, 1);
        }

        if (TryByte(structure, 0x07, out var nvStorage))
        {
            Add(
                structure,
                "NV Storage Device Address",
                nvStorage == 0xFF ? "0xFF · No storage device" : $"0x{nvStorage:X2}",
                0x07,
                1);
        }

        if (TryU64(structure, 0x08, out var baseAddress))
        {
            Add(
                structure,
                "Base Address",
                $"0x{baseAddress:X16} · {((baseAddress & 1) != 0 ? "I/O space" : "Memory mapped")}",
                0x08,
                8);
        }

        if (TryByte(structure, 0x10, out var modifier))
        {
            Add(
                structure,
                "Base Address Modifier / Interrupt Info",
                $"0x{modifier:X2}",
                0x10,
                1);
            AddBitFields(
                structure,
                0x10,
                1,
                modifier,
                (4, "Address bit 0"),
                (3, "Interrupt information specified"),
                (2, "Reserved"),
                (1, "Interrupt polarity (1 = active high)"),
                (0, "Interrupt trigger mode (1 = level)"));
            Add(structure, "Register Spacing [7:6]", RegisterSpacing((modifier >> 6) & 0x03), 0x10, 1);
            Add(structure, "Reserved [5]", "0", 0x10, 1);
        }

        if (TryByte(structure, 0x11, out var interrupt))
        {
            Add(
                structure,
                "Interrupt Number",
                interrupt == 0 ? "0 · Unspecified/unsupported" : interrupt.ToString(Invariant),
                0x11,
                1);
        }
    }

    private static void ParseSystemPowerSupply(SmbiosStructure structure)
    {
        if (TryByte(structure, 0x04, out var group))
        {
            Add(structure, "Power Unit Group", group.ToString(Invariant), 0x04, 1);
        }

        AddString(structure, "Location", 0x05);
        AddString(structure, "Device Name", 0x06);
        AddString(structure, "Manufacturer", 0x07);
        AddString(structure, "Serial Number", 0x08);
        AddString(structure, "Asset Tag Number", 0x09);
        AddString(structure, "Model Part Number", 0x0A);
        AddString(structure, "Revision Level", 0x0B);

        if (TryU16(structure, 0x0C, out var maximumPower))
        {
            Add(
                structure,
                "Max Power Capacity",
                maximumPower == 0x8000 ? "Unknown (8000h)" : $"{maximumPower} W",
                0x0C,
                2);
        }

        if (TryU16(structure, 0x0E, out var characteristics))
        {
            var type = (byte)((characteristics >> 10) & 0x0F);
            var status = (byte)((characteristics >> 7) & 0x07);
            var range = (byte)((characteristics >> 3) & 0x0F);
            Add(
                structure,
                "Power Supply Characteristics",
                $"0x{characteristics:X4}",
                0x0E,
                2);
            AddBitFields(
                structure,
                0x0E,
                2,
                characteristics,
                (2, "Power supply unplugged from wall"),
                (1, "Power supply present"),
                (0, "Power supply hot-replaceable"));
            Add(structure, "Power Supply Type [13:10]", Describe(type, SmbiosEnumCatalog.PowerSupplyTypes), 0x0E, 2);
            Add(structure, "Status [9:7]", Describe(status, SmbiosEnumCatalog.ProbeStatuses), 0x0E, 2);
            Add(structure, "Input Voltage Range [6:3]", Describe(range, SmbiosEnumCatalog.InputVoltageRangeSwitching), 0x0E, 2);
        }

        AddHandle(structure, "Input Voltage Probe Handle", 0x10);
        AddHandle(structure, "Cooling Device Handle", 0x12);
        AddHandle(structure, "Input Current Probe Handle", 0x14);
    }

    private static void ParseAdditionalInformation(SmbiosStructure structure)
    {
        if (!TryByte(structure, 0x04, out var count))
        {
            return;
        }

        Add(structure, "Additional Information Entry Count", count.ToString(Invariant), 0x04, 1);
        var offset = 0x05;

        for (var index = 0; index < count; index++)
        {
            if (!TryByte(structure, offset, out var entryLength) || entryLength < 6)
            {
                Add(structure, $"Entry {index + 1}", "Malformed or truncated entry", offset, 1);
                break;
            }

            if (!TryU16(structure, offset + 1, out var referencedHandle) ||
                !TryByte(structure, offset + 3, out var referencedOffset) ||
                !TryByte(structure, offset + 4, out var stringIndex))
            {
                break;
            }

            var valueLength = Math.Max(0, entryLength - 5);
            var value = valueLength > 0 && offset + 5 + valueLength <= structure.FormattedArea.Length
                ? Convert.ToHexString(structure.FormattedArea.Span.Slice(offset + 5, valueLength))
                : string.Empty;
            var stringValue = stringIndex == 0 || stringIndex > structure.Strings.Count
                ? "(not set)"
                : structure.Strings[stringIndex - 1].Value;
            Add(
                structure,
                $"Entry {index + 1}",
                $"Handle 0x{referencedHandle:X4}, Offset 0x{referencedOffset:X2}, Value {value}",
                offset,
                entryLength,
                $"String {stringIndex}: {stringValue}");
            offset += entryLength;
        }
    }

    private static void ParseOnboardDevicesExtended(SmbiosStructure structure)
    {
        AddString(structure, "Reference Designation", 0x04);

        if (TryByte(structure, 0x05, out var rawType))
        {
            var type = (byte)(rawType & 0x7F);
            Add(
                structure,
                "Device Type",
                Describe(type, SmbiosEnumCatalog.OnboardDeviceTypes),
                0x05,
                1);
            var enabled = (rawType & 0x80) != 0 ? 1 : 0;
            structure.AddField(new SmbiosField(
                "Device enabled [7]",
                $"{enabled} ({(enabled == 1 ? "Yes" : "No")})",
                0x05,
                1,
                enabled == 1 ? "Device enabled" : "Device disabled")
            {
                BitIndex = 7,
                BitValue = enabled
            });
        }

        if (TryByte(structure, 0x06, out var instance))
        {
            Add(structure, "Device Type Instance", instance.ToString(Invariant), 0x06, 1);
        }

        if (TryU16(structure, 0x07, out var segment) &&
            TryByte(structure, 0x09, out var bus) &&
            TryByte(structure, 0x0A, out var deviceFunction))
        {
            Add(
                structure,
                "Segment / Bus / Device / Function",
                $"Segment {segment}, Bus {bus}, Device {deviceFunction >> 3}, Function {deviceFunction & 0x07}",
                0x07,
                4);
        }
    }

    private static void ParseManagementControllerHostInterface(SmbiosStructure structure)
    {
        if (!TryByte(structure, 0x04, out var interfaceType) ||
            !TryByte(structure, 0x05, out var dataLength))
        {
            return;
        }

        Add(structure, "Interface Type", DescribeManagementControllerInterface(interfaceType), 0x04, 1);
        Add(structure, "Interface Type Specific Data Length", dataLength.ToString(Invariant), 0x05, 1);

        if (dataLength > 0 && 0x06 + dataLength <= structure.FormattedArea.Length)
        {
            Add(
                structure,
                "Interface Type Specific Data",
                Convert.ToHexString(structure.FormattedArea.Span.Slice(0x06, dataLength)),
                0x06,
                dataLength,
                interfaceType == 0xF0 ? "First four bytes are IANA enterprise number, MSB first" : null);
        }

        var protocolCountOffset = 0x06 + dataLength;
        if (!TryByte(structure, protocolCountOffset, out var protocolCount))
        {
            return;
        }

        Add(structure, "Protocol Record Count", protocolCount.ToString(Invariant), protocolCountOffset, 1);
        var offset = protocolCountOffset + 1;

        for (var index = 0; index < protocolCount; index++)
        {
            if (!TryByte(structure, offset, out var protocolType) ||
                !TryByte(structure, offset + 1, out var protocolLength) ||
                offset + 2 + protocolLength > structure.FormattedArea.Length)
            {
                break;
            }

            var data = protocolLength > 0
                ? Convert.ToHexString(structure.FormattedArea.Span.Slice(offset + 2, protocolLength))
                : "(empty)";
            Add(
                structure,
                $"Protocol Record {index + 1}",
                $"Type {DescribeManagementProtocol(protocolType)}, Data {data}",
                offset,
                protocolLength + 2);
            offset += protocolLength + 2;
        }
    }

    private static void ParseProcessorAdditionalInformation(SmbiosStructure structure)
    {
        AddHandle(structure, "Referenced Processor Handle", 0x04);

        if (!TryByte(structure, 0x06, out var blockLength) ||
            !TryByte(structure, 0x07, out var architecture))
        {
            return;
        }

        Add(structure, "Processor-Specific Block Length", blockLength.ToString(Invariant), 0x06, 1);
        Add(
            structure,
            "Processor Architecture",
            DescribeProcessorArchitecture(architecture),
            0x07,
            1);

        if (structure.FormattedArea.Length > 0x08)
        {
            Add(
                structure,
                "Processor-Specific Data",
                Convert.ToHexString(structure.FormattedArea.Span[0x08..]),
                0x08,
                structure.FormattedArea.Length - 0x08,
                "架构专用内容由 DSP0134 引用的架构文档或厂商文档定义");
        }
    }

    private static void ParseFirmwareInventoryInformation(SmbiosStructure structure)
    {
        AddString(structure, "Firmware Component Name", 0x04);
        AddString(structure, "Firmware Version", 0x05);
        AddEnumByte(structure, "Version Format", 0x06, SmbiosEnumCatalog.FirmwareVersionFormats);
        AddString(structure, "Firmware ID", 0x07);
        AddEnumByte(structure, "Firmware ID Format", 0x08, SmbiosEnumCatalog.FirmwareIdFormats);
        AddString(structure, "Release Date", 0x09);
        AddString(structure, "Manufacturer", 0x0A);
        AddString(structure, "Lowest Supported Firmware Version", 0x0B);

        if (TryU64(structure, 0x0C, out var imageSize))
        {
            Add(
                structure,
                "Image Size",
                imageSize == ulong.MaxValue ? "Unknown" : FormatByteSize(imageSize),
                0x0C,
                8);
        }

        if (TryU16(structure, 0x14, out var characteristics))
        {
            Add(
                structure,
                "Characteristics",
                $"0x{characteristics:X4}",
                0x14,
                2);
            AddBitFields(
                structure,
                0x14,
                2,
                characteristics,
                (0, "Updatable"),
                (1, "Write-Protect"));
        }

        AddEnumByte(structure, "State", 0x16, SmbiosEnumCatalog.FirmwareInventoryStates);

        if (!TryByte(structure, 0x17, out var count))
        {
            return;
        }

        Add(structure, "Associated Component Count", count.ToString(Invariant), 0x17, 1);
        var handles = new List<string>();

        for (var index = 0; index < count; index++)
        {
            var offset = 0x18 + (index * 2);
            if (!TryU16(structure, offset, out var handle))
            {
                break;
            }

            handles.Add($"0x{handle:X4}");
        }

        if (handles.Count > 0)
        {
            Add(structure, "Associated Component Handles", string.Join(", ", handles), 0x18, handles.Count * 2);
        }
    }

    private static void ParseStringProperty(SmbiosStructure structure)
    {
        if (TryU16(structure, 0x04, out var propertyId))
        {
            Add(
                structure,
                "String Property ID",
                propertyId switch
                {
                    0 => "0 · Reserved",
                    1 => "1 · UEFI Device Path",
                    _ => $"0x{propertyId:X4} · {(propertyId < 0x8000 ? "DMTF reserved" : propertyId < 0xC000 ? "Firmware vendor" : "OEM")}"
                },
                0x04,
                2);
        }

        AddString(structure, "String Property Value", 0x06);
        AddHandle(structure, "Parent Handle", 0x07);
    }

    private static void AddProbeCommon(
        SmbiosStructure structure,
        string unit,
        bool signed,
        bool tenths,
        string _)
    {
        AddString(structure, "Description", 0x04);

        if (TryByte(structure, 0x05, out var locationStatus))
        {
            var status = (byte)((locationStatus >> 5) & 0x07);
            var location = (byte)(locationStatus & 0x1F);
            Add(
                structure,
                "Location and Status",
                $"0x{locationStatus:X2}",
                0x05,
                1);
            Add(structure, "Status [7:5]", Describe(status, SmbiosEnumCatalog.ProbeStatuses), 0x05, 1);
            Add(structure, "Location [4:0]", Describe(location, SmbiosEnumCatalog.ProbeLocations), 0x05, 1);
        }

        AddProbeValue(structure, "Maximum Value", 0x06, unit, signed, tenths, 1m);
        AddProbeValue(structure, "Minimum Value", 0x08, unit, signed, tenths, 1m);
        AddProbeValue(structure, "Resolution", 0x0A, unit, signed, tenths, tenths ? 0.001m : 0.1m);
        AddProbeValue(structure, "Tolerance", 0x0C, unit, signed, tenths, 1m);

        if (TryU16(structure, 0x0E, out var accuracy))
        {
            Add(
                structure,
                "Accuracy",
                accuracy == 0x8000 ? "Unknown (8000h)" : $"±{(accuracy / 100m).ToString("0.##", Invariant)}%",
                0x0E,
                2);
        }

        if (TryU32(structure, 0x10, out var oemDefined))
        {
            Add(structure, "OEM-defined", $"0x{oemDefined:X8}", 0x10, 4);
        }

        AddProbeValue(structure, "Nominal Value", 0x14, unit, signed, tenths, 1m);
    }

    private static void AddProbeValue(
        SmbiosStructure structure,
        string name,
        int offset,
        string unit,
        bool signed,
        bool tenths,
        decimal scale)
    {
        if (!TryU16(structure, offset, out var raw))
        {
            return;
        }

        if (raw == 0x8000)
        {
            Add(structure, name, "Unknown (8000h)", offset, 2);
            return;
        }

        var numeric = signed ? unchecked((short)raw) : (int)raw;
        var value = numeric * scale;
        var text = tenths
            ? $"{value.ToString("0.###", Invariant)} {unit}"
            : $"{value.ToString("0.##", Invariant)} {unit}";
        Add(structure, name, text, offset, 2, $"0x{raw:X4}");
    }

    private static void AddAddressRange(
        SmbiosStructure structure,
        string startName,
        string endName,
        int startOffset,
        int endOffset,
        int extendedStartOffset,
        int extendedEndOffset)
    {
        AddMemoryMappedAddress(structure, startName, startOffset, extendedStartOffset);
        AddMemoryMappedAddress(structure, endName, endOffset, extendedEndOffset);
    }

    private static void AddMemoryMappedAddress(
        SmbiosStructure structure,
        string name,
        int offset,
        int extendedOffset)
    {
        if (!TryU32(structure, offset, out var kilobytes))
        {
            return;
        }

        if (kilobytes == uint.MaxValue &&
            TryU64(structure, extendedOffset, out var extendedBytes))
        {
            Add(
                structure,
                name,
                $"{FormatByteSize(extendedBytes)} (0x{extendedBytes:X16})",
                extendedOffset,
                8,
                "32-bit 字段为 FFFFFFFFh，使用 64-bit 扩展地址");
            return;
        }

        Add(
            structure,
            name,
            FormatByteSize((ulong)kilobytes * 1024UL),
            offset,
            4,
            $"0x{kilobytes:X8} KiB");
    }

    private static void AddMemoryErrorAddress64(SmbiosStructure structure, string name, int offset)
    {
        if (!TryU64(structure, offset, out var value))
        {
            return;
        }

        Add(
            structure,
            name,
            value == 0x8000000000000000
                ? "Unknown (8000 0000 0000 0000h)"
                : $"0x{value:X16}",
            offset,
            8);
    }

    private static void AddThreshold(SmbiosStructure structure, string name, int offset)
    {
        if (!TryU16(structure, offset, out var value))
        {
            return;
        }

        Add(
            structure,
            name,
            value == 0x8000 ? "Unknown (8000h)" : value.ToString(Invariant),
            offset,
            2,
            "单位由关联的 probe/cooling component 决定");
    }

    private static void AddBcd(
        SmbiosStructure structure,
        string name,
        int offset,
        int minimum,
        int maximum)
    {
        if (!TryByte(structure, offset, out var raw))
        {
            return;
        }

        var value = DecodeBcd(raw);
        var text = value is not null && value >= minimum && value <= maximum
            ? $"{value:D2} (BCD 0x{raw:X2})"
            : $"0x{raw:X2} · does not contribute";
        Add(structure, name, text, offset, 1);
    }

    private static void AddByteFlags(
        SmbiosStructure structure,
        string name,
        int offset,
        params (int Bit, string Name)[] bits)
    {
        if (TryByte(structure, offset, out var value))
        {
            Add(structure, name, $"0x{value:X2}", offset, 1);
            AddBitFields(structure, offset, 1, value, bits);
        }
    }

    private static void AddWordFlags(
        SmbiosStructure structure,
        string name,
        int offset,
        params (int Bit, string Name)[] bits)
    {
        if (TryU16(structure, offset, out var value))
        {
            Add(structure, name, $"0x{value:X4}", offset, 2);
            AddBitFields(structure, offset, 2, value, bits);
        }
    }

    private static void AddByteUnknownSpecial(
        SmbiosStructure structure,
        string name,
        int offset,
        byte unknownValue)
    {
        if (TryByte(structure, offset, out var value))
        {
            Add(
                structure,
                name,
                value == unknownValue ? $"0x{value:X2} · Unknown" : value.ToString(Invariant),
                offset,
                1);
        }
    }

    private static void AddWordUnknownSpecial(
        SmbiosStructure structure,
        string name,
        int offset,
        ushort unknownValue,
        string? unit = null)
    {
        if (!TryU16(structure, offset, out var value))
        {
            return;
        }

        var display = value == unknownValue
            ? $"0x{value:X4} · Unknown"
            : unit is null
                ? value.ToString(Invariant)
                : $"{value} {unit}";
        Add(structure, name, display, offset, 2);
    }

    private static void AddModuleSize(SmbiosStructure structure, string name, int offset)
    {
        if (!TryByte(structure, offset, out var raw))
        {
            return;
        }

        var sizeBits = raw & 0x7F;
        var doubleBank = (raw & 0x80) != 0;
        var value = sizeBits switch
        {
            0x7D => "Not determinable",
            0x7E => "Module installed but no memory enabled",
            0x7F => "Not installed",
            _ => sizeBits < 63 ? $"{1UL << sizeBits} MiB" : $"2^{sizeBits} MiB"
        };
        Add(
            structure,
            name,
            value,
            offset,
            1,
            doubleBank ? "Double-bank connection" : "Single-bank connection");
    }

    private static (int Bit, string Name)[] MemoryModuleTypeFlags() =>
    [
        (0, "Other"),
        (1, "Unknown"),
        (2, "Standard"),
        (3, "Fast Page Mode"),
        (4, "EDO"),
        (5, "Parity"),
        (6, "ECC"),
        (7, "SIMM"),
        (8, "DIMM"),
        (9, "Burst EDO"),
        (10, "SDRAM")
    ];

    private static string Nibble(int value) =>
        value == 0x0F ? "None" : value.ToString(Invariant);

    private static string ResetBootOption(int value) =>
        value switch
        {
            0 => "Reserved",
            1 => "Operating system",
            2 => "System utilities",
            3 => "Do not reboot",
            _ => "Unknown"
        };

    private static string PasswordStatus(int value) =>
        value switch
        {
            0 => "Disabled",
            1 => "Enabled",
            2 => "Not implemented",
            3 => "Unknown",
            _ => "Unknown"
        };

    private static int? DecodeBcd(byte value)
    {
        var high = (value >> 4) & 0x0F;
        var low = value & 0x0F;
        return high <= 9 && low <= 9 ? (high * 10) + low : null;
    }

    private static string DescribeEventLogAccessMethod(byte value) =>
        value switch
        {
            0x00 => "Indexed I/O: one 8-bit index, one 8-bit data",
            0x01 => "Indexed I/O: two 8-bit indexes, one 8-bit data",
            0x02 => "Indexed I/O: one 16-bit index, one 8-bit data",
            0x03 => "Memory-mapped physical 32-bit address",
            0x04 => "General-Purpose NonVolatile Data",
            >= 0x80 => $"Firmware/OEM-specific (0x{value:X2})",
            _ => $"Reserved (0x{value:X2})"
        };

    private static string DescribeAccessAddress(byte method, uint address) =>
        method switch
        {
            0x00 => $"Index port 0x{address & 0xFFFF:X4}, Data port 0x{(address >> 16) & 0xFFFF:X4}",
            0x01 => $"Index ports 0x{address & 0xFFFF:X4}, Data port 0x{(address >> 16) & 0xFFFF:X4}",
            0x02 => $"16-bit index port 0x{address & 0xFFFF:X4}, Data port 0x{(address >> 16) & 0xFFFF:X4}",
            0x03 => $"Physical address 0x{address:X8}",
            0x04 => $"GPNV handle 0x{address & 0xFFFF:X4}",
            _ => "Firmware/OEM-specific"
        };

    private static string DescribeLogHeaderFormat(byte value) =>
        value switch
        {
            0x00 => "No header",
            0x01 => "Type 1 log header",
            >= 0x80 => $"Firmware/OEM-specific (0x{value:X2})",
            _ => $"Reserved (0x{value:X2})"
        };

    private static string DescribeEventLogType(byte value) =>
        value switch
        {
            0x00 => "Reserved",
            0x01 => "Single-bit ECC memory error",
            0x02 => "Multi-bit ECC memory error",
            0x03 => "Parity memory error",
            0x04 => "Bus time-out",
            0x05 => "I/O Channel Check",
            0x06 => "Software NMI",
            0x07 => "POST Memory Resize",
            0x08 => "POST Error",
            0x09 => "PCI Parity Error",
            0x0A => "PCI System Error",
            0x0B => "CPU Failure",
            0x0C => "EISA Failsafe Timer time-out",
            0x0D => "Correctable memory log disabled",
            0x0E => "Logging disabled for a specific Event Type",
            0x10 => "System Limit Exceeded",
            0x11 => "Asynchronous hardware timer expired",
            0x12 => "System configuration information",
            0x13 => "Hard-disk information",
            0x14 => "System reconfigured",
            0x15 => "Uncorrectable CPU-complex error",
            0x16 => "Log Area Reset/Cleared",
            0x17 => "System boot",
            0xFF => "End of log",
            >= 0x80 => "System/OEM-specific",
            _ => "Unused/available"
        };

    private static string DescribeEventLogDataFormat(byte value) =>
        value switch
        {
            0x00 => "None",
            0x01 => "Handle",
            0x02 => "Multiple-Event",
            0x03 => "Multiple-Event Handle",
            0x04 => "POST Results Bitmap",
            0x05 => "System Management Type",
            0x06 => "Multiple-Event System Management Type",
            >= 0x80 => "OEM assigned",
            _ => "Unused/available"
        };

    private static string RegisterSpacing(int value) =>
        value switch
        {
            0 => "successive byte boundaries",
            1 => "32-bit boundaries",
            2 => "16-byte boundaries",
            _ => "reserved"
        };

    private static string DescribeManagementControllerInterface(byte value) =>
        value switch
        {
            <= 0x3F => $"MCTP Host Interface (0x{value:X2})",
            0x40 => "Network Host Interface",
            0xF0 => "OEM-defined",
            _ => $"Reserved (0x{value:X2})"
        };

    private static string DescribeManagementProtocol(byte value) =>
        value switch
        {
            0x02 => "IPMI",
            0x03 => "MCTP",
            0x04 => "Redfish over IP",
            0xF0 => "OEM-defined",
            _ => $"Reserved/unknown (0x{value:X2})"
        };

    private static string DescribeProcessorArchitecture(byte value) =>
        value switch
        {
            0x00 => "Reserved",
            0x01 => "IA32 (x86)",
            0x02 => "x64 (x86-64, Intel64, AMD64, EM64T)",
            0x03 => "Intel Itanium architecture",
            0x04 => "32-bit ARM (AArch32)",
            0x05 => "64-bit ARM (AArch64)",
            0x06 => "32-bit RISC-V (RV32)",
            0x07 => "64-bit RISC-V (RV64)",
            0x08 => "128-bit RISC-V (RV128)",
            0x09 => "32-bit LoongArch",
            0x0A => "64-bit LoongArch",
            _ => $"Reserved (0x{value:X2})"
        };
}
