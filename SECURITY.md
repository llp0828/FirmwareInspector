# Security Policy

## Supported Versions

Security fixes are applied to the latest release on the `main` branch.

## Reporting a Vulnerability

Please report vulnerabilities through the repository's private
**Security > Report a vulnerability** flow. Do not open a public issue for
security-sensitive reports.

Include:

- Affected version or commit.
- Windows version.
- Reproduction steps.
- Impact and any known mitigation.

## Sensitive SMBIOS Data

SMBIOS output can contain UUIDs, serial numbers, asset tags, OEM identifiers,
and firmware inventory data. Do not attach real SMBIOS exports or memory dumps
to public issues. Use synthetic data or redact machine-specific values first.

