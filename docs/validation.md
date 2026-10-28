# Validation

## Principles

Firmware Inspector treats raw bytes as the source of truth. Field decoding is
additive and must never make an unknown value look known.

Validation covers:

- Raw header and structure-table boundaries.
- Formatted structure length handling.
- String-set termination and invalid string indexes.
- Field offsets and little-endian integers.
- Bit-field expansion.
- Standard and reserved enum values.
- Type `0-46`, `126`, and `127` field branches.
- OEM and unallocated Type fallback behavior.
- JSON, INI, and raw export contracts.

## Synthetic Fixtures

Public tests generate SMBIOS structures in code. This provides deterministic
coverage without exposing real UUIDs, serial numbers, asset tags, or firmware
tables.

Synthetic fixtures verify:

- Type 0 firmware fields and bit fields.
- Type 1 system UUID byte order.
- Type 4 processor information.
- Type 7 cache size decoding.
- Type 13 language fields.
- Type 17 memory fields.
- Type 19 and 20 address mapping.
- Additional standard types.
- INI direct keys, unique sections, UTF-16 BOM, and raw output.

## Local Cross-Validation

During development, output was compared against an independently captured
vendor utility export on a physical machine. The comparison included:

- Structure order
- Type and Handle
- Formatted structure length
- Raw block bytes after accounting for omitted final double NULs
- Parser output for selected standard fields

The real capture is intentionally not included in this repository because it
contains machine-specific identifiers.

## CI Policy

GitHub Actions runs only deterministic, synthetic tests. Tests must not depend
on:

- A physical machine's SMBIOS payload.
- A specific OEM, CPU, memory vendor, or firmware version.
- Administrator-only interfaces.
- Network access.

Local live-read checks may be performed manually before a release.

## Limitations

- DSP0134 does not define OEM Type `128-255` payloads.
- Type 31 BIS fields and Type 44 architecture-specific content reference
  external specifications.
- `GetSystemFirmwareTable('RSMB')` does not return the SMBIOS Entry Point
  Structure.
- Unknown field values remain numeric and are not inferred from neighboring
  data.

