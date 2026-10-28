# Command-Line Interface

`FirmwareInspector.Cli.exe` is a separate console program. It does not create a
WPF window and shares the same Core parser as the graphical application.

## Help

```powershell
FirmwareInspector.Cli.exe /?
```

Accepted help forms also include `-?`, `/h`, `-h`, `--help`, and `help`.

## List Structures

Write a tab-separated list to standard output:

```powershell
FirmwareInspector.Cli.exe list
```

Write to a file:

```powershell
FirmwareInspector.Cli.exe list .\structures.tsv
```

Columns:

```text
Index  Type  Handle  Offset  Length  Name  Summary
```

## Export the Complete INI

```powershell
FirmwareInspector.Cli.exe export-ini .\smbios.ini
```

The output is UTF-16 LE with BOM for AutoIt `IniRead` compatibility.

Alias:

```powershell
FirmwareInspector.Cli.exe /export-ini:.\smbios.ini
```

## Query One Type

```powershell
FirmwareInspector.Cli.exe type 13
```

Type Number forms:

```text
13
Type 13
0x0D
```

All structures matching the Type are printed. If the Type occurs multiple
times, use `--handle` to select one instance:

```powershell
FirmwareInspector.Cli.exe type 13 --handle 0x0017
```

## Output Formats

Text is the default:

```powershell
FirmwareInspector.Cli.exe type 0 --format text
```

INI:

```powershell
FirmwareInspector.Cli.exe type 13 --format ini --output .\type13.ini
```

JSON:

```powershell
FirmwareInspector.Cli.exe type 17 --format json --output .\type17.json
```

When `--output` is omitted, content is written to standard output. This is
suitable for pipelines and process capture.

## Option Forms

The parser accepts both separated and inline forms:

```text
--format json
--format=json
--handle 0x0017
--handle=0x0017
```

## Exit Codes

| Code | Meaning |
| --- | --- |
| `0` | Success |
| `1` | Read, parse, or write failure |
| `2` | Invalid arguments |
| `3` | Type or Handle match not found |

## Automation Notes

- Use `--output` for deterministic file encoding.
- Use `type ... --format json` when another program needs structured fields.
- Treat text output as human-readable, not as a stable machine contract.
- Capture stderr separately from stdout when integrating with a host tool.

