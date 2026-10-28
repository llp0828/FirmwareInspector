## Summary

Describe the problem and the focused change made by this pull request.

## Validation

- [ ] `dotnet build FirmwareInspector.slnx -c Release` passes.
- [ ] `dotnet test tests/FirmwareInspector.Core.Tests/FirmwareInspector.Core.Tests.csproj -c Release` passes.
- [ ] `dotnet format FirmwareInspector.slnx --verify-no-changes` passes.
- [ ] New tests use synthetic data only.
- [ ] No real UUID, serial number, asset tag, firmware dump, or unredacted screenshot is included.
- [ ] Documentation is updated for CLI, INI schema, or parsing changes.

## Parsing Accuracy

If this pull request changes field interpretation, include:

- DSP0134 Type and field reference.
- Behavior for shorter structures or missing fields.
- Handling of unknown and reserved values.

