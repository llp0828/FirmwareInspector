namespace FirmwareInspector.Core.Parsing;

internal static partial class SmbiosEnumCatalog
{
    public static readonly IReadOnlyDictionary<byte, string> WakeUpTypes =
        new Dictionary<byte, string>
        {
            [0x00] = "Reserved",
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "APM Timer",
            [0x04] = "Modem Ring",
            [0x05] = "LAN Remote",
            [0x06] = "Power Switch",
            [0x07] = "PCI PME#",
            [0x08] = "AC Power Restored"
        };

    public static readonly IReadOnlyDictionary<byte, string> ChassisTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Desktop",
            [0x04] = "Low Profile Desktop",
            [0x05] = "Pizza Box",
            [0x06] = "Mini Tower",
            [0x07] = "Tower",
            [0x08] = "Portable",
            [0x09] = "Laptop",
            [0x0A] = "Notebook",
            [0x0B] = "Hand Held",
            [0x0C] = "Docking Station",
            [0x0D] = "All in One",
            [0x0E] = "Sub Notebook",
            [0x0F] = "Space-saving",
            [0x10] = "Lunch Box",
            [0x11] = "Main Server Chassis",
            [0x12] = "Expansion Chassis",
            [0x13] = "SubChassis",
            [0x14] = "Bus Expansion Chassis",
            [0x15] = "Peripheral Chassis",
            [0x16] = "RAID Chassis",
            [0x17] = "Rack Mount Chassis",
            [0x18] = "Sealed-case PC",
            [0x19] = "Multi-system Chassis",
            [0x1A] = "Compact PCI",
            [0x1B] = "Advanced TCA",
            [0x1C] = "Blade",
            [0x1D] = "Blade Enclosure",
            [0x1E] = "Tablet",
            [0x1F] = "Convertible",
            [0x20] = "Detachable",
            [0x21] = "IoT Gateway",
            [0x22] = "Embedded PC",
            [0x23] = "Mini PC",
            [0x24] = "Stick PC"
        };

    public static readonly IReadOnlyDictionary<byte, string> ChassisStates =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Safe",
            [0x04] = "Warning",
            [0x05] = "Critical",
            [0x06] = "Non-recoverable"
        };

    public static readonly IReadOnlyDictionary<byte, string> SecurityStatuses =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "None",
            [0x04] = "External interface locked out",
            [0x05] = "External interface enabled"
        };

    public static readonly IReadOnlyDictionary<byte, string> BoardTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Unknown",
            [0x02] = "Other",
            [0x03] = "Server Blade",
            [0x04] = "Connectivity Switch",
            [0x05] = "System Management Module",
            [0x06] = "Processor Module",
            [0x07] = "I/O Module",
            [0x08] = "Memory Module",
            [0x09] = "Daughter board",
            [0x0A] = "Motherboard",
            [0x0B] = "Processor/Memory Module",
            [0x0C] = "Processor/IO Module",
            [0x0D] = "Interconnect board"
        };

    public static readonly IReadOnlyDictionary<byte, string> ProcessorTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Central Processor",
            [0x04] = "Math Processor",
            [0x05] = "DSP Processor",
            [0x06] = "Video Processor"
        };

    public static readonly IReadOnlyDictionary<ushort, string> ProcessorFamilies =
        new Dictionary<ushort, string>
        {
            [0x0001] = "Other",
            [0x0002] = "Unknown",
            [0x0003] = "8086",
            [0x0004] = "80286",
            [0x0005] = "Intel386 processor",
            [0x0006] = "Intel486 processor",
            [0x000B] = "Intel Pentium processor",
            [0x000C] = "Pentium Pro processor",
            [0x000D] = "Pentium II processor",
            [0x000E] = "Pentium processor with MMX technology",
            [0x000F] = "Intel Celeron processor",
            [0x0010] = "Pentium II Xeon processor",
            [0x0011] = "Pentium III processor",
            [0x0014] = "Intel Celeron M processor",
            [0x0015] = "Intel Pentium 4 HT processor",
            [0x0016] = "Intel Processor",
            [0x0018] = "AMD Duron Processor Family",
            [0x001D] = "AMD Athlon Processor Family",
            [0x0028] = "Intel Core Duo processor",
            [0x0029] = "Intel Core Duo mobile processor",
            [0x002A] = "Intel Core Solo mobile processor",
            [0x002B] = "Intel Atom processor",
            [0x002C] = "Intel Core M processor",
            [0x003F] = "AMD FX Series Processor",
            [0x0046] = "AMD C-Series Processor",
            [0x0047] = "AMD E-Series Processor",
            [0x0048] = "AMD A-Series Processor",
            [0x0049] = "AMD G-Series Processor",
            [0x004A] = "AMD Z-Series Processor",
            [0x004B] = "AMD R-Series Processor",
            [0x006B] = "AMD Zen Processor Family",
            [0x0082] = "Itanium processor",
            [0x0083] = "AMD Athlon 64 Processor Family",
            [0x0084] = "AMD Opteron Processor Family",
            [0x0085] = "AMD Sempron Processor Family",
            [0x0086] = "AMD Turion 64 Mobile Technology",
            [0x0087] = "Dual-Core AMD Opteron Processor Family",
            [0x0088] = "AMD Athlon 64 X2 Dual-Core Processor Family",
            [0x0089] = "AMD Turion 64 X2 Mobile Technology",
            [0x008A] = "Quad-Core AMD Opteron Processor Family",
            [0x008B] = "Third-Generation AMD Opteron Processor Family",
            [0x008C] = "AMD Phenom FX Quad-Core Processor Family",
            [0x008D] = "AMD Phenom X4 Quad-Core Processor Family",
            [0x008E] = "AMD Phenom X2 Dual-Core Processor Family",
            [0x008F] = "AMD Athlon X2 Dual-Core Processor Family",
            [0x00A1] = "Quad-Core Intel Xeon processor 3200 Series",
            [0x00A2] = "Dual-Core Intel Xeon processor 3000 Series",
            [0x00A3] = "Quad-Core Intel Xeon processor 5300 Series",
            [0x00A4] = "Dual-Core Intel Xeon processor 5100 Series",
            [0x00A5] = "Dual-Core Intel Xeon processor 5000 Series",
            [0x00A6] = "Dual-Core Intel Xeon processor LV",
            [0x00A7] = "Dual-Core Intel Xeon processor ULV",
            [0x00A8] = "Dual-Core Intel Xeon processor 7100 Series",
            [0x00A9] = "Quad-Core Intel Xeon processor 5400 Series",
            [0x00AA] = "Quad-Core Intel Xeon processor",
            [0x00AB] = "Dual-Core Intel Xeon processor 5200 Series",
            [0x00AC] = "Dual-Core Intel Xeon processor 7200 Series",
            [0x00AD] = "Quad-Core Intel Xeon processor 7300 Series",
            [0x00AE] = "Quad-Core Intel Xeon processor 7400 Series",
            [0x00AF] = "Multi-Core Intel Xeon processor 7400 Series",
            [0x00B0] = "Pentium III Xeon processor",
            [0x00B1] = "Pentium III Processor with Intel SpeedStep Technology",
            [0x00B2] = "Pentium 4 Processor",
            [0x00B3] = "Intel Xeon processor",
            [0x00B5] = "Intel Xeon processor MP",
            [0x00B6] = "AMD Athlon XP Processor Family",
            [0x00B7] = "AMD Athlon MP Processor Family",
            [0x00B8] = "Intel Itanium 2 processor",
            [0x00B9] = "Intel Pentium M processor",
            [0x00BA] = "Intel Celeron D processor",
            [0x00BB] = "Intel Pentium D processor",
            [0x00BC] = "Intel Pentium Processor Extreme Edition",
            [0x00BD] = "Intel Core Solo Processor",
            [0x00BF] = "Intel Core 2 Duo Processor",
            [0x00C0] = "Intel Core 2 Solo processor",
            [0x00C1] = "Intel Core 2 Extreme processor",
            [0x00C2] = "Intel Core 2 Quad processor",
            [0x00C3] = "Intel Core 2 Extreme mobile processor",
            [0x00C4] = "Intel Core 2 Duo mobile processor",
            [0x00C5] = "Intel Core 2 Solo mobile processor",
            [0x00C6] = "Intel Core i7 processor",
            [0x00C7] = "Dual-Core Intel Celeron processor",
            [0x00CD] = "Intel Core i5 processor",
            [0x00CE] = "Intel Core i3 processor",
            [0x00CF] = "Intel Core i9 processor",
            [0x00D0] = "Intel Xeon D Processor family",
            [0x00D6] = "Multi-Core Intel Xeon processor",
            [0x00D7] = "Dual-Core Intel Xeon processor 3xxx Series",
            [0x00D8] = "Quad-Core Intel Xeon processor 3xxx Series",
            [0x00DA] = "Dual-Core Intel Xeon processor 5xxx Series",
            [0x00DB] = "Quad-Core Intel Xeon processor 5xxx Series",
            [0x00DD] = "Dual-Core Intel Xeon processor 7xxx Series",
            [0x00DE] = "Quad-Core Intel Xeon processor 7xxx Series",
            [0x00DF] = "Multi-Core Intel Xeon processor 7xxx Series",
            [0x00E0] = "Multi-Core Intel Xeon processor 3400 Series",
            [0x00EA] = "AMD Athlon Dual-Core Processor Family",
            [0x00EC] = "AMD Phenom II Processor Family",
            [0x00ED] = "AMD Athlon II Processor Family",
            [0x00EE] = "Six-Core AMD Opteron Processor Family",
            [0x00EF] = "AMD Sempron M Processor Family",
            [0x00FE] = "Use Processor Family 2",
            [0x0100] = "ARMv7",
            [0x0101] = "ARMv8",
            [0x0102] = "ARMv9",
            [0x0200] = "RISC-V RV32",
            [0x0201] = "RISC-V RV64",
            [0x0202] = "RISC-V RV128",
            [0x0300] = "Intel Core 3",
            [0x0301] = "Intel Core 5",
            [0x0302] = "Intel Core 7",
            [0x0303] = "Intel Core 9",
            [0x0304] = "Intel Core Ultra 3",
            [0x0305] = "Intel Core Ultra 5",
            [0x0306] = "Intel Core Ultra 7",
            [0x0307] = "Intel Core Ultra 9"
        };

    public static readonly IReadOnlyDictionary<byte, string> ProcessorUpgrades =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Daughter Board",
            [0x04] = "ZIF Socket",
            [0x05] = "Replaceable Piggy Back",
            [0x06] = "None",
            [0x07] = "LIF Socket",
            [0x08] = "Slot 1",
            [0x09] = "Slot 2",
            [0x0A] = "370-pin socket",
            [0x0B] = "Slot A",
            [0x0C] = "Slot M",
            [0x0D] = "Socket 423",
            [0x0E] = "Socket A (Socket 462)",
            [0x0F] = "Socket 478",
            [0x10] = "Socket 754",
            [0x11] = "Socket 940",
            [0x12] = "Socket 939",
            [0x13] = "Socket mPGA604",
            [0x14] = "Socket LGA771",
            [0x15] = "Socket LGA775",
            [0x16] = "Socket S1",
            [0x17] = "Socket AM2",
            [0x18] = "Socket F (1207)",
            [0x19] = "Socket LGA1366",
            [0x1A] = "Socket G34",
            [0x1B] = "Socket AM3",
            [0x1C] = "Socket C32",
            [0x1D] = "Socket LGA1156",
            [0x1E] = "Socket LGA1567",
            [0x1F] = "Socket PGA988A",
            [0x20] = "Socket BGA1288",
            [0x21] = "Socket rPGA988B",
            [0x22] = "Socket BGA1023",
            [0x23] = "Socket BGA1224",
            [0x24] = "Socket LGA1155",
            [0x25] = "Socket LGA1356",
            [0x26] = "Socket LGA2011",
            [0x27] = "Socket FS1",
            [0x28] = "Socket FS2",
            [0x29] = "Socket FM1",
            [0x2A] = "Socket FM2",
            [0x2B] = "Socket LGA2011-3",
            [0x2C] = "Socket LGA1356-3",
            [0x2D] = "Socket LGA1150",
            [0x2E] = "Socket BGA1168",
            [0x2F] = "Socket BGA1234",
            [0x30] = "Socket BGA1364",
            [0x31] = "Socket AM4",
            [0x32] = "Socket LGA1151",
            [0x33] = "Socket BGA1356",
            [0x34] = "Socket BGA1440",
            [0x35] = "Socket BGA1515",
            [0x36] = "Socket LGA3647-1",
            [0x37] = "Socket SP3",
            [0x38] = "Socket SP3r2",
            [0x39] = "Socket LGA2066",
            [0x3A] = "Socket BGA1392",
            [0x3B] = "Socket BGA1510",
            [0x3C] = "Socket BGA1528",
            [0x3D] = "Socket LGA4189",
            [0x3E] = "Socket LGA1200",
            [0x3F] = "Socket LGA4677",
            [0x40] = "Socket LGA1700",
            [0x41] = "Socket BGA1744",
            [0x42] = "Socket BGA1781",
            [0x43] = "Socket BGA1211",
            [0x44] = "Socket BGA2422",
            [0x45] = "Socket LGA1211",
            [0x46] = "Socket LGA2422",
            [0x47] = "Socket LGA5773",
            [0x48] = "Socket BGA5773",
            [0x49] = "Socket AM5",
            [0x4A] = "Socket SP5",
            [0x4B] = "Socket SP6",
            [0x4C] = "Socket BGA883",
            [0x4D] = "Socket BGA1190",
            [0x4E] = "Socket BGA4129",
            [0x4F] = "Socket LGA4710",
            [0x50] = "Socket LGA7529",
            [0x51] = "Socket BGA1964",
            [0x52] = "Socket BGA1792",
            [0x53] = "Socket BGA2049",
            [0x54] = "Socket BGA2551",
            [0x55] = "Socket LGA1851",
            [0x56] = "Socket BGA2114",
            [0x57] = "Socket BGA2833",
            [0xFF] = "Use Socket Type string"
        };

    public static readonly IReadOnlyDictionary<byte, string> CacheErrorCorrections =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "None",
            [0x04] = "Parity",
            [0x05] = "Single-bit ECC",
            [0x06] = "Multi-bit ECC"
        };

    public static readonly IReadOnlyDictionary<byte, string> CacheTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Instruction",
            [0x04] = "Data",
            [0x05] = "Unified"
        };

    public static readonly IReadOnlyDictionary<byte, string> CacheAssociativity =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Direct Mapped",
            [0x04] = "2-way Set-Associative",
            [0x05] = "4-way Set-Associative",
            [0x06] = "Fully Associative",
            [0x07] = "8-way Set-Associative",
            [0x08] = "16-way Set-Associative",
            [0x09] = "12-way Set-Associative",
            [0x0A] = "24-way Set-Associative",
            [0x0B] = "32-way Set-Associative",
            [0x0C] = "48-way Set-Associative",
            [0x0D] = "64-way Set-Associative",
            [0x0E] = "20-way Set-Associative"
        };

    public static readonly IReadOnlyDictionary<byte, string> ConnectorTypes =
        new Dictionary<byte, string>
        {
            [0x00] = "None",
            [0x01] = "Centronics",
            [0x02] = "Mini Centronics",
            [0x03] = "Proprietary",
            [0x04] = "DB-25 pin male",
            [0x05] = "DB-25 pin female",
            [0x06] = "DB-15 pin male",
            [0x07] = "DB-15 pin female",
            [0x08] = "DB-9 pin male",
            [0x09] = "DB-9 pin female",
            [0x0A] = "RJ-11",
            [0x0B] = "RJ-45",
            [0x0C] = "50-pin MiniSCSI",
            [0x0D] = "Mini-DIN",
            [0x0E] = "Micro-DIN",
            [0x0F] = "PS/2",
            [0x10] = "Infrared",
            [0x11] = "HP-HIL",
            [0x12] = "Access Bus (USB)",
            [0x13] = "SSA SCSI",
            [0x14] = "Circular DIN-8 male",
            [0x15] = "Circular DIN-8 female",
            [0x16] = "On Board IDE",
            [0x17] = "On Board Floppy",
            [0x18] = "9-pin Dual Inline",
            [0x19] = "25-pin Dual Inline",
            [0x1A] = "50-pin Dual Inline",
            [0x1B] = "68-pin Dual Inline",
            [0x1C] = "On Board Sound Input from CD-ROM",
            [0x1D] = "Mini-Centronics Type-14",
            [0x1E] = "Mini-Centronics Type-26",
            [0x1F] = "Mini-jack (headphones)",
            [0x20] = "BNC",
            [0x21] = "1394",
            [0x22] = "SAS/SATA Plug Receptacle",
            [0x23] = "USB Type-C Receptacle",
            [0xA0] = "PC-98",
            [0xA1] = "PC-98Hireso",
            [0xA2] = "PC-H98",
            [0xA3] = "PC-98Note",
            [0xA4] = "PC-98Full",
            [0xFF] = "Other"
        };

    public static readonly IReadOnlyDictionary<byte, string> PortTypes =
        new Dictionary<byte, string>
        {
            [0x00] = "None",
            [0x01] = "Parallel Port XT/AT Compatible",
            [0x02] = "Parallel Port PS/2",
            [0x03] = "Parallel Port ECP",
            [0x04] = "Parallel Port EPP",
            [0x05] = "Parallel Port ECP/EPP",
            [0x06] = "Serial Port XT/AT Compatible",
            [0x07] = "Serial Port 16450 Compatible",
            [0x08] = "Serial Port 16550 Compatible",
            [0x09] = "Serial Port 16550A Compatible",
            [0x0A] = "SCSI Port",
            [0x0B] = "MIDI Port",
            [0x0C] = "Joy Stick Port",
            [0x0D] = "Keyboard Port",
            [0x0E] = "Mouse Port",
            [0x0F] = "SSA SCSI",
            [0x10] = "USB",
            [0x11] = "FireWire (IEEE P1394)",
            [0x12] = "PCMCIA Type I",
            [0x13] = "PCMCIA Type II",
            [0x14] = "PCMCIA Type III",
            [0x15] = "Card bus",
            [0x16] = "Access Bus Port",
            [0x17] = "SCSI II",
            [0x18] = "SCSI Wide",
            [0x19] = "PC-98",
            [0x1A] = "PC-98-Hireso",
            [0x1B] = "PC-H98",
            [0x1C] = "Video Port",
            [0x1D] = "Audio Port",
            [0x1E] = "Modem Port",
            [0x1F] = "Network Port",
            [0x20] = "SATA",
            [0x21] = "SAS",
            [0x22] = "MFDP (Multi-Function Display Port)",
            [0x23] = "Thunderbolt",
            [0xA0] = "8251 Compatible",
            [0xA1] = "8251 FIFO Compatible",
            [0xFF] = "Other"
        };

    public static readonly IReadOnlyDictionary<byte, string> SlotTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "ISA",
            [0x04] = "MCA",
            [0x05] = "EISA",
            [0x06] = "PCI",
            [0x07] = "PC Card (PCMCIA)",
            [0x08] = "VL-VESA",
            [0x09] = "Proprietary",
            [0x0A] = "Processor Card Slot",
            [0x0B] = "Proprietary Memory Card Slot",
            [0x0C] = "I/O Riser Card Slot",
            [0x0D] = "NuBus",
            [0x0E] = "PCI - 66MHz Capable",
            [0x0F] = "AGP",
            [0x10] = "AGP 2X",
            [0x11] = "AGP 4X",
            [0x12] = "PCI-X",
            [0x13] = "AGP 8X",
            [0x14] = "M.2 Socket 1-DP (Key A)",
            [0x15] = "M.2 Socket 1-SD (Key E)",
            [0x16] = "M.2 Socket 2 (Key B)",
            [0x17] = "M.2 Socket 3 (Key M)",
            [0x18] = "MXM Type I",
            [0x19] = "MXM Type II",
            [0x1A] = "MXM Type III",
            [0x1B] = "MXM Type III HE",
            [0x1C] = "MXM Type IV",
            [0x1D] = "MXM 3.0 Type A",
            [0x1E] = "MXM 3.0 Type B",
            [0x1F] = "PCI Express Gen 2 U.2",
            [0x20] = "PCI Express Gen 3 U.2",
            [0x21] = "PCI Express Mini 52-pin with bottom-side keep-outs",
            [0x22] = "PCI Express Mini 52-pin without bottom-side keep-outs",
            [0x23] = "PCI Express Mini 76-pin",
            [0x24] = "PCI Express Gen 4 U.2",
            [0x25] = "PCI Express Gen 5 U.2",
            [0x26] = "OCP NIC 3.0 SFF",
            [0x27] = "OCP NIC 3.0 LFF",
            [0x28] = "OCP NIC Prior to 3.0",
            [0x30] = "CXL Flexbus 1.0",
            [0xA5] = "PCI Express",
            [0xA6] = "PCI Express x1",
            [0xA7] = "PCI Express x2",
            [0xA8] = "PCI Express x4",
            [0xA9] = "PCI Express x8",
            [0xAA] = "PCI Express x16",
            [0xAB] = "PCI Express Gen 2",
            [0xAC] = "PCI Express Gen 2 x1",
            [0xAD] = "PCI Express Gen 2 x2",
            [0xAE] = "PCI Express Gen 2 x4",
            [0xAF] = "PCI Express Gen 2 x8",
            [0xB0] = "PCI Express Gen 2 x16",
            [0xB1] = "PCI Express Gen 3",
            [0xB2] = "PCI Express Gen 3 x1",
            [0xB3] = "PCI Express Gen 3 x2",
            [0xB4] = "PCI Express Gen 3 x4",
            [0xB5] = "PCI Express Gen 3 x8",
            [0xB6] = "PCI Express Gen 3 x16",
            [0xB8] = "PCI Express Gen 4",
            [0xB9] = "PCI Express Gen 4 x1",
            [0xBA] = "PCI Express Gen 4 x2",
            [0xBB] = "PCI Express Gen 4 x4",
            [0xBC] = "PCI Express Gen 4 x8",
            [0xBD] = "PCI Express Gen 4 x16",
            [0xBE] = "PCI Express Gen 5",
            [0xBF] = "PCI Express Gen 5 x1",
            [0xC0] = "PCI Express Gen 5 x2",
            [0xC1] = "PCI Express Gen 5 x4",
            [0xC2] = "PCI Express Gen 5 x8",
            [0xC3] = "PCI Express Gen 5 x16",
            [0xC4] = "PCI Express Gen 6 and Beyond",
            [0xC5] = "EDSFF E1",
            [0xC6] = "EDSFF E3"
        };

    public static readonly IReadOnlyDictionary<byte, string> SlotWidths =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "8 bit",
            [0x04] = "16 bit",
            [0x05] = "32 bit",
            [0x06] = "64 bit",
            [0x07] = "128 bit",
            [0x08] = "x1",
            [0x09] = "x2",
            [0x0A] = "x4",
            [0x0B] = "x8",
            [0x0C] = "x12",
            [0x0D] = "x16",
            [0x0E] = "x32"
        };

    public static readonly IReadOnlyDictionary<byte, string> SlotUsages =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Available",
            [0x04] = "In use",
            [0x05] = "Unavailable"
        };

    public static readonly IReadOnlyDictionary<byte, string> SlotLengths =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Short Length",
            [0x04] = "Long Length",
            [0x05] = "2.5-inch drive form factor",
            [0x06] = "3.5-inch drive form factor"
        };

    public static readonly IReadOnlyDictionary<byte, string> SlotHeights =
        new Dictionary<byte, string>
        {
            [0x00] = "Not applicable",
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Full height",
            [0x04] = "Low-profile"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryArrayLocations =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "System board or motherboard",
            [0x04] = "ISA add-on card",
            [0x05] = "EISA add-on card",
            [0x06] = "PCI add-on card",
            [0x07] = "MCA add-on card",
            [0x08] = "PCMCIA add-on card",
            [0x09] = "Proprietary add-on card",
            [0x0A] = "NuBus",
            [0xA0] = "PC-98/C20 add-on card",
            [0xA1] = "PC-98/C24 add-on card",
            [0xA2] = "PC-98/E add-on card",
            [0xA3] = "PC-98/Local bus add-on card",
            [0xA4] = "CXL add-on card"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryArrayUses =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "System memory",
            [0x04] = "Video memory",
            [0x05] = "Flash memory",
            [0x06] = "Non-volatile RAM",
            [0x07] = "Cache memory"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryErrorCorrections =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "None",
            [0x04] = "Parity",
            [0x05] = "Single-bit ECC",
            [0x06] = "Multi-bit ECC",
            [0x07] = "CRC"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryFormFactors =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "SIMM",
            [0x04] = "SIP",
            [0x05] = "Chip",
            [0x06] = "DIP",
            [0x07] = "ZIP",
            [0x08] = "Proprietary Card",
            [0x09] = "DIMM",
            [0x0A] = "TSOP",
            [0x0B] = "Row of chips",
            [0x0C] = "RIMM",
            [0x0D] = "SODIMM",
            [0x0E] = "SRIMM",
            [0x0F] = "FB-DIMM",
            [0x10] = "Die",
            [0x11] = "CAMM"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "DRAM",
            [0x04] = "EDRAM",
            [0x05] = "VRAM",
            [0x06] = "SRAM",
            [0x07] = "RAM",
            [0x08] = "ROM",
            [0x09] = "FLASH",
            [0x0A] = "EEPROM",
            [0x0B] = "FEPROM",
            [0x0C] = "EPROM",
            [0x0D] = "CDRAM",
            [0x0E] = "3DRAM",
            [0x0F] = "SDRAM",
            [0x10] = "SGRAM",
            [0x11] = "RDRAM",
            [0x12] = "DDR",
            [0x13] = "DDR2",
            [0x14] = "DDR2 FB-DIMM",
            [0x18] = "DDR3",
            [0x19] = "FBD2",
            [0x1A] = "DDR4",
            [0x1B] = "LPDDR",
            [0x1C] = "LPDDR2",
            [0x1D] = "LPDDR3",
            [0x1E] = "LPDDR4",
            [0x1F] = "Logical non-volatile device",
            [0x20] = "HBM",
            [0x21] = "HBM2",
            [0x22] = "DDR5",
            [0x23] = "LPDDR5",
            [0x24] = "HBM3"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryTechnologies =
        new Dictionary<byte, string>
        {
            [0x00] = "Unknown",
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "DRAM",
            [0x04] = "NVDIMM-N",
            [0x05] = "NVDIMM-F",
            [0x06] = "NVDIMM-P",
            [0x07] = "Intel Optane persistent memory",
            [0x08] = "MRDIMM"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryErrorTypes =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "OK",
            [0x04] = "Bad read",
            [0x05] = "Parity error",
            [0x06] = "Single-bit error",
            [0x07] = "Double-bit error",
            [0x08] = "Multi-bit error",
            [0x09] = "Nibble error",
            [0x0A] = "Checksum error",
            [0x0B] = "CRC error",
            [0x0C] = "Corrected single-bit error",
            [0x0D] = "Corrected error",
            [0x0E] = "Uncorrectable error"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryErrorGranularities =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Device level",
            [0x04] = "Memory partition level"
        };

    public static readonly IReadOnlyDictionary<byte, string> MemoryErrorOperations =
        new Dictionary<byte, string>
        {
            [0x01] = "Other",
            [0x02] = "Unknown",
            [0x03] = "Read",
            [0x04] = "Write",
            [0x05] = "Partial write"
        };

    public static readonly IReadOnlyDictionary<byte, string> BootStatuses =
        new Dictionary<byte, string>
        {
            [0] = "No errors detected",
            [1] = "No bootable media",
            [2] = "Normal operating system failed to load",
            [3] = "Firmware-detected hardware failure",
            [4] = "Operating system-detected hardware failure",
            [5] = "User-requested boot",
            [6] = "System security violation",
            [7] = "Previously requested image",
            [8] = "System watchdog timer expired"
        };
}
