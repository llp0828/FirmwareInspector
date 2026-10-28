# Changelog

All notable changes to this project are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

### Planned

- Additional synthetic fixtures for OEM and architecture-specific structures.
- Localized CLI help and documentation.

## [1.0.0] - 2026-10-27

### Added

- WPF SMBIOS structure browser for Windows.
- Field-level parsing for standard SMBIOS Type 0-46, 126, and 127 structures.
- Raw byte and string-set inspection for every structure.
- Bit-level rendering with value, state, offset, and specification description.
- JSON, UTF-16 LE INI, and raw binary export.
- Standalone `FirmwareInspector.Cli.exe` with `/ ?`, list, export, and type-query commands.
- Stable INI field keys and unique `TypeN_HandleXXXX` sections for AutoIt `IniRead`.
- Synthetic parser and exporter regression tests.
- GitHub Actions build and test workflow.

