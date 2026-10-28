# INI Format

The INI export is designed for human inspection and AutoIt `IniRead`.

## Encoding

- UTF-16 LE
- Byte order mark included
- CRLF line endings

This avoids the multi-byte decoding problems that AutoIt and Windows profile
APIs can encounter when reading UTF-8 content containing Chinese values.

## Metadata

```ini
[SMBIOS]
Source=GetSystemFirmwareTable('RSMB')
Version=3.8
DmiRevision=0x00
DeclaredTableLength=1234
ParsedTableLength=1234
StructureTableSha256=...
StructureCount=42
ExportedStructureCount=42
WarningCount=0
```

## Structure Index

```ini
[Index]
Count=42
0000=Type0_Handle0000
0001=Type1_Handle0001
```

The index maps structure order to a unique section name.

## Structure Section

Section names use Type Number and Handle:

```ini
[Type13_Handle0017]
```

There may be multiple structures with the same Type. Handle is part of the
section name so that sections remain unique.

Common keys:

```ini
Section=Type13_Handle0017
Index=17
Type=13
TypeHex=0x0D
TypeName=BIOS Language Information
TableOffset=0x040B
Length=22
Handle=0x0017
StringCount=3
RawHex=0D161700...
FieldCount=18
```

## Stable Field Keys

Every parsed field is accessible by a normalized field key:

```ini
CurrentLanguage=en|US|iso8859-1
CurrentLanguage.Offset=0x15
CurrentLanguage.Length=1
CurrentLanguage.Note=String index 1
```

Bit fields include the bit number:

```ini
FirmwareCharacteristicsPCISupported_Bit7=1 (Yes)
FirmwareCharacteristicsPCISupported_Bit7.Offset=0x0A
FirmwareCharacteristicsPCISupported_Bit7.BitIndex=7
FirmwareCharacteristicsPCISupported_Bit7.BitValue=1
FirmwareCharacteristicsPCISupported_Bit7.State=Yes
```

Keys are unique within a section. Metadata keys such as `Type`, `Length`, and
`Handle` are reserved and are not overwritten by normalized field names.

## Detailed Field Map

The exporter also preserves a stable-numbered map:

```ini
Field.0001.Name=结构类型
Field.0001.Key=StructureType
Field.0001.Offset=0x00
Field.0001.Length=1
Field.0001.Value=13 · BIOS Language Information
```

This map is useful when a consumer needs every field in order, including fields
whose friendly key differs from the original display name.

## Strings

Both structured and direct string keys are emitted:

```ini
String.0001.Index=1
String.0001.Offset=0x0421
String.0001.Value=en|US|iso8859-1
String1=en|US|iso8859-1
```

## Raw Bytes

`RawHex` contains the complete structure byte sequence. `Raw.NNNN` provides
readable 16-byte lines:

```ini
Raw.0000=0D 16 17 00 03 00 00 00 00 00 00 00 00 00 00 00  |................|
```

The raw bytes include the formatted area, string area, and final double NUL.

## AutoIt Read

```autoit
Local $sIni = @ScriptDir & "\smbios.ini"
Local $sLanguage = IniRead($sIni, "Type13_Handle0017", "CurrentLanguage", "")

If @error Then
    ConsoleWrite("CurrentLanguage not found" & @CRLF)
Else
    ConsoleWrite($sLanguage & @CRLF)
EndIf
```

Enumerate all Type 13 structures:

```autoit
Local $aSections = IniReadSectionNames($sIni)

If Not @error Then
    For $i = 1 To $aSections[0]
        If StringRegExp($aSections[$i], "^Type13_Handle[0-9A-F]{4}$") Then
            ConsoleWrite($aSections[$i] & " / " & _
                IniRead($sIni, $aSections[$i], "CurrentLanguage", "") & @CRLF)
        End If
    Next
EndIf
```

Do not include square brackets in the `IniRead` section argument.

