namespace FirmwareInspector.Core.Parsing;

internal static partial class SmbiosEnumCatalog
{
    public static readonly IReadOnlyDictionary<byte, string> MemoryControllerErrorDetectMethods =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "None",
            [0x04] = "8-bit Parity",
            [0x05] = "32-bit ECC",
            [0x06] = "64-bit ECC",
            [0x07] = "128-bit ECC",
            [0x08] = "CRC"
        };

    public static readonly IReadOnlyDictionary<byte, string> InterleaveSupport =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "One-Way Interleave",
            [0x04] = "Two-Way Interleave",
            [0x05] = "Four-Way Interleave",
            [0x06] = "Eight-Way Interleave",
            [0x07] = "Sixteen-Way Interleave"
        };

    public static readonly IReadOnlyDictionary<byte, string> OnboardDeviceTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Video",
            [0x04] = "SCSI Controller",
            [0x05] = "Ethernet",
            [0x06] = "Token Ring",
            [0x07] = "Sound",
            [0x08] = "PATA Controller",
            [0x09] = "SATA Controller",
            [0x0A] = "SAS Controller",
            [0x0B] = "Wireless LAN",
            [0x0C] = "Bluetooth",
            [0x0D] = "WWAN",
            [0x0E] = "eMMC",
            [0x0F] = "NVMe Controller",
            [0x10] = "UFS Controller"
        };

    public static readonly IReadOnlyDictionary<byte, string> PointingDeviceTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Mouse",
            [0x04] = "Track Ball",
            [0x05] = "Track Point",
            [0x06] = "Glide Point",
            [0x07] = "Touch Pad",
            [0x08] = "Touch Screen",
            [0x09] = "Optical Sensor"
        };

    public static readonly IReadOnlyDictionary<byte, string> PointingDeviceInterfaces =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Serial",
            [0x04] = "PS/2",
            [0x05] = "Infrared",
            [0x06] = "HP-HIL",
            [0x07] = "Bus mouse",
            [0x08] = "ADB",
            [0xA0] = "Bus mouse DB-9",
            [0xA1] = "Bus mouse micro-DIN",
            [0xA2] = "USB",
            [0xA3] = "I2C",
            [0xA4] = "SPI"
        };

    public static readonly IReadOnlyDictionary<byte, string> BatteryChemistries =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Lead Acid",
            [0x04] = "Nickel Cadmium",
            [0x05] = "Nickel Metal Hydride",
            [0x06] = "Lithium Ion",
            [0x07] = "Zinc Air",
            [0x08] = "Lithium Polymer"
        };

    public static readonly IReadOnlyDictionary<byte, string> ProbeLocations =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Processor",
            [0x04] = "Disk",
            [0x05] = "Peripheral Bay",
            [0x06] = "System Management Module",
            [0x07] = "Motherboard",
            [0x08] = "Memory Module",
            [0x09] = "Processor Module",
            [0x0A] = "Power Unit",
            [0x0B] = "Add-in Card",
            [0x0C] = "Front Panel Board",
            [0x0D] = "Back Panel Board",
            [0x0E] = "Power System Board",
            [0x0F] = "Drive Back Plane"
        };

    public static readonly IReadOnlyDictionary<byte, string> ProbeStatuses =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "OK",
            [0x04] = "Non-critical",
            [0x05] = "Critical",
            [0x06] = "Non-recoverable"
        };

    public static readonly IReadOnlyDictionary<byte, string> CoolingDeviceTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Fan",
            [0x04] = "Centrifugal Blower",
            [0x05] = "Chip Fan",
            [0x06] = "Cabinet Fan",
            [0x07] = "Power Supply Fan",
            [0x08] = "Heat Pipe",
            [0x09] = "Integrated Refrigeration",
            [0x10] = "Active Cooling",
            [0x11] = "Passive Cooling"
        };

    public static readonly IReadOnlyDictionary<byte, string> ManagementDeviceTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "National Semiconductor LM75",
            [0x04] = "National Semiconductor LM78",
            [0x05] = "National Semiconductor LM79",
            [0x06] = "National Semiconductor LM80",
            [0x07] = "National Semiconductor LM81",
            [0x08] = "Analog Devices ADM9240",
            [0x09] = "Dallas Semiconductor DS1780",
            [0x0A] = "Maxim 1617",
            [0x0B] = "Genesys GL518SM",
            [0x0C] = "Winbond W83781D",
            [0x0D] = "Holtek HT82H791"
        };

    public static readonly IReadOnlyDictionary<byte, string> ManagementAddressTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "I/O Port",
            [0x04] = "Memory",
            [0x05] = "SM Bus"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryChannelTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Rambus",
            [0x04] = "SyncLink"
        };

    public static readonly IReadOnlyDictionary<byte, string> IpmiInterfaceTypes =
        new Dictionary<byte, string>
        {
            [0x00] = "Unknown",
            [0x01] = "KCS: Keyboard Controller Style",
            [0x02] = "SMIC: Server Management Interface Chip",
            [0x03] = "BT: Block Transfer",
            [0x04] = "SSIF: SMBus System Interface"
        };

    public static readonly IReadOnlyDictionary<byte, string> PowerSupplyTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Linear",
            [0x04] = "Switching",
            [0x05] = "Battery",
            [0x06] = "UPS",
            [0x07] = "Converter",
            [0x08] = "Regulator"
        };

    public static readonly IReadOnlyDictionary<byte, string> InputVoltageRangeSwitching =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Manual",
            [0x04] = "Auto-switch",
            [0x05] = "Wide range",
            [0x06] = "Not applicable"
        };

    public static readonly IReadOnlyDictionary<byte, string> FirmwareVersionFormats =
        new Dictionary<byte, string>
        {
            [0x00] = "Free-form string",
            [0x01] = "MAJOR.MINOR",
            [0x02] = "32-bit hexadecimal string",
            [0x03] = "64-bit hexadecimal string"
        };

    public static readonly IReadOnlyDictionary<byte, string> FirmwareIdFormats =
        new Dictionary<byte, string>
        {
            [0x00] = "Free-form string",
            [0x01] = "RFC 4122 UUID string"
        };

    public static readonly IReadOnlyDictionary<byte, string> FirmwareInventoryStates =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Disabled",
            [0x04] = "Enabled",
            [0x05] = "Absent",
            [0x06] = "StandbyOffline",
            [0x07] = "StandbySpare",
            [0x08] = "UnavailableOffline"
        };
}
