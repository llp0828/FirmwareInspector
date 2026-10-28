# Contributing

## Before You Start

- Use a focused branch and keep each pull request scoped to one behavior change.
- Search existing issues before opening a new one.
- Do not include real SMBIOS dumps, UUIDs, serial numbers, OEM identifiers, or
  unredacted screenshots from a physical machine.

## Development Environment

- Windows 10 or Windows 11 x64.
- .NET SDK `10.0.x`.
- PowerShell.

Restore and build:

```powershell
dotnet restore FirmwareInspector.slnx
dotnet build FirmwareInspector.slnx -c Release
```

Run tests:

```powershell
dotnet test tests/FirmwareInspector.Core.Tests/FirmwareInspector.Core.Tests.csproj -c Release
```

Check formatting:

```powershell
dotnet format FirmwareInspector.slnx --verify-no-changes
```

## Parsing Changes

Field-level parsing changes must include:

- The relevant DSP0134 structure and field definition.
- Boundary handling when the structure is shorter than expected.
- A synthetic test covering known, unknown, and reserved values where relevant.
- Raw byte preservation without introducing guessed interpretations.

Do not add vendor-private or OEM parsing without a citable specification and
corresponding synthetic data.

## Pull Request Checklist

- The change is limited to the stated problem.
- `dotnet build` succeeds with zero warnings.
- Tests pass.
- Formatting verification passes.
- New exported data contains no machine-specific values.
- Documentation is updated when CLI, INI schema, or parsing behavior changes.

