# Architecture

## Goals

Firmware Inspector separates data acquisition, parsing, presentation, and
serialization so that the WPF and CLI programs produce the same results.

## Layers

### FirmwareInspector.Core

Responsibilities:

- Call `GetSystemFirmwareTable('RSMB')`.
- Parse the `RawSMBIOSData` header.
- Walk the SMBIOS structure table safely.
- Decode standard fields and bit fields.
- Preserve formatted bytes, string-set bytes, and complete raw structure bytes.
- Export structured INI content.

The Core project has no WPF dependency.

### FirmwareInspector.App

Responsibilities:

- Present structures, fields, raw bytes, and strings.
- Filter the structure list.
- Provide JSON, INI, and raw binary save dialogs.
- Display parser warnings and source metadata.

The UI does not implement its own parsing rules.

### FirmwareInspector.Cli

Responsibilities:

- Parse command-line arguments.
- Read live SMBIOS data through Core.
- Emit text, JSON, or INI.
- Write to standard output or a file without creating a window.

The CLI does not reference WPF.

## Parsing Flow

```text
FirmwareTableReader.ReadRawSmbios()
        |
        v
SmbiosParser.Parse()
        |
        +-- validate RawSMBIOSData header
        +-- enumerate formatted structures and string sets
        +-- create SmbiosStructure models
        |
        v
SmbiosFieldParser.Parse()
        |
        +-- common Type/Length/Handle fields
        +-- type-specific field branches
        +-- bit field expansion
        |
        v
Immutable parse result
        |
        +-- WPF view models
        +-- JSON exporter
        +-- INI exporter
        `-- CLI formatter
```

## Modeling Rules

- A structure keeps both decoded fields and complete raw bytes.
- A field may carry a note, bit index, and bit value.
- Unknown or unsupported data remains visible as raw bytes.
- String references are resolved only when the index is valid.
- Parsing never reads beyond the declared structure length.

## Error Strategy

- A malformed structure boundary adds a warning and stops table traversal.
- Unknown enum values retain the numeric representation.
- Export failures are surfaced to the caller or UI.
- The CLI uses distinct exit codes for arguments, runtime failures, and missing
  Type matches.

## Extension Points

- Add a standard Type branch in `SmbiosFieldParser`.
- Add enum labels in `SmbiosEnumCatalog`.
- Add a name in `SmbiosTypeCatalog`.
- Add synthetic tests under `FirmwareInspector.Core.Tests`.
- Add export-specific formatting without changing the parser model.

