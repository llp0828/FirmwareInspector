# Firmware Inspector

A Windows SMBIOS inspection and automation toolkit built with .NET 10, WPF,
and `GetSystemFirmwareTable('RSMB')`.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D4)
[![License](https://img.shields.io/badge/license-MIT-2E7D32)](LICENSE)
[![SMBIOS](https://img.shields.io/badge/SMBIOS-3.8-376AD4)](https://www.dmtf.org/standards/smbios)

Firmware Inspector 面向固件工程师、驱动开发者和自动化工具开发者，用于查看、
校验和导出本机 SMBIOS 结构。项目提供 WPF 图形界面和独立命令行程序，不读取
ACPI、不使用 WMI、不访问网络。

## 功能

- 读取 Windows `RawSMBIOSData`，解析结构头、字段、字符串集和原始字节。
- 字段级解析 SMBIOS Type `0-46`、`126`、`127`。
- Type `47-125` 标记为 DSP0134 未分配类型。
- Type `128-255` 保留 OEM 原始数据，不进行猜测性解析。
- Bit field 按 `Bit n`、`0/1 (No/Yes)`、偏移和规范描述逐项展示。
- 支持 JSON、原始二进制和 AutoIt 友好的 UTF-16 LE INI 导出。
- 提供不启动 UI 的命令行程序，支持完整导出、结构列表和 Type/Handle 查询。
- 自带合成 SMBIOS 测试数据，可离线运行解析与导出测试。

## 项目组成

| 项目 | 说明 |
| --- | --- |
| `FirmwareInspector.App` | WPF 图形界面 |
| `FirmwareInspector.Cli` | 独立控制台程序，不加载 WPF |
| `FirmwareInspector.Core` | SMBIOS 读取、解析、模型和导出逻辑 |
| `FirmwareInspector.Core.Tests` | 基于合成数据的回归测试 |

数据流：

```text
GetSystemFirmwareTable('RSMB')
        |
        v
FirmwareTableReader
        |
        v
SmbiosParser + SmbiosFieldParser
        |
        +----> WPF structure browser
        |
        +----> JSON / INI / raw export
        |
        +----> CLI text / JSON / INI output
```

## 系统要求

运行发布版：

- Windows 10 或 Windows 11，x64。
- 无需单独安装 .NET Runtime，发布配置为自包含。

从源码构建：

- Windows 10 或 Windows 11，x64。
- .NET SDK `10.0.x`。
- PowerShell 7 或 Windows PowerShell 5.1。

## 快速开始

```powershell
dotnet restore FirmwareInspector.slnx
dotnet build FirmwareInspector.slnx -c Release
dotnet run --project src/FirmwareInspector.App/FirmwareInspector.App.csproj
```

界面左侧按 `Type 0`、`Type 13` 等 Type Number 浏览结构。中间区域提供：

- 解析字段
- 原始字节
- 字符串集

顶部搜索支持结构名称、字段名称、字段值、句柄和字符串内容。

## 命令行

命令行程序不会启动 WPF UI。

```powershell
FirmwareInspector.Cli.exe /?
FirmwareInspector.Cli.exe list
FirmwareInspector.Cli.exe export-ini .\smbios.ini
FirmwareInspector.Cli.exe type 13
FirmwareInspector.Cli.exe type 0 --format ini
```

按 Handle 查询并输出 JSON：

```powershell
FirmwareInspector.Cli.exe type 13 --handle 0x006A --format json --output .\type13.json
```

返回值：

| 退出码 | 含义 |
| --- | --- |
| `0` | 成功 |
| `1` | 读取或写入失败 |
| `2` | 命令参数错误 |
| `3` | 未找到指定 Type |

完整说明：[docs/cli.md](docs/cli.md)

## INI 与 AutoIt

INI 使用唯一结构段名：

```ini
[Type13_Handle0017]
Type=13
Handle=0x0017
CurrentLanguage=en|US|iso8859-1
CurrentLanguage.Offset=0x15
CurrentLanguage.Length=1
CurrentLanguage.Note=String index 1
```

AutoIt 读取：

```autoit
Local $sIni = @ScriptDir & "\smbios.ini"
Local $sLanguage = IniRead($sIni, "Type13_Handle0017", "CurrentLanguage", "")

If @error Then
    ConsoleWrite("CurrentLanguage not found" & @CRLF)
Else
    ConsoleWrite($sLanguage & @CRLF)
EndIf
```

INI 使用带 BOM 的 UTF-16 LE 编码，以兼容 AutoIt `IniRead` 对中文值的读取。
完整格式：[docs/ini-format.md](docs/ini-format.md)

## 解析范围

| SMBIOS Type | 处理方式 |
| --- | --- |
| `0-46` | DSP0134 标准字段级解析 |
| `47-125` | DSP0134 未分配类型，仅展示通用结构信息 |
| `126` | Inactive 结构 |
| `127` | End-of-Table |
| `128-255` | OEM 自定义，保留通用头和原始字节 |

标准结构遵循 DMTF DSP0134 3.8.0 语义。工具不会为未知枚举、OEM 字段或外部规范
字段编造解释。

## 数据准确性

- 每个字段读取前检查结构 `Length`，不跨结构边界读取。
- 字符串索引 `0`、越界索引、未知枚举和保留值会显式标记。
- UUID 按 SMBIOS wire format 处理前三字段的 little-endian 字节序。
- Bit field 保留 Bit 编号、取值、描述和原始偏移。
- 所有结构保留完整 `RawBytes`，包括格式化区、字符串区和双 NUL 终止符。
- 回归测试只使用合成数据，避免公开真实设备标识。

验证方法：[docs/validation.md](docs/validation.md)

## 隐私提示

真实 SMBIOS 数据通常包含：

- 系统 UUID
- 序列号
- 资产标签
- OEM 标识
- 固件版本和设备清单

不要将真实 INI、JSON、原始固件表或未脱敏截图提交到公开 Issue。发布日志前应
检查并脱敏。项目仓库不包含任何真实 HE 导出或本机固件表。

## 构建与测试

```powershell
dotnet build FirmwareInspector.slnx -c Release
dotnet test tests/FirmwareInspector.Core.Tests/FirmwareInspector.Core.Tests.csproj -c Release
dotnet format FirmwareInspector.slnx --verify-no-changes
```

发布两个自包含单文件程序：

```powershell
dotnet publish src/FirmwareInspector.App/FirmwareInspector.App.csproj -c Release -o artifacts/publish
dotnet publish src/FirmwareInspector.Cli/FirmwareInspector.Cli.csproj -c Release -o artifacts/publish
```

生成文件：

```text
artifacts/publish/FirmwareInspector.exe
artifacts/publish/FirmwareInspector.Cli.exe
```

## 目录结构

```text
.
|-- .github/
|   |-- ISSUE_TEMPLATE/
|   |-- workflows/
|   `-- PULL_REQUEST_TEMPLATE.md
|-- docs/
|   |-- architecture.md
|   |-- cli.md
|   |-- ini-format.md
|   `-- validation.md
|-- src/
|   |-- FirmwareInspector.App/
|   |-- FirmwareInspector.Cli/
|   `-- FirmwareInspector.Core/
|-- tests/
|   `-- FirmwareInspector.Core.Tests/
|-- CHANGELOG.md
|-- CONTRIBUTING.md
|-- Directory.Build.props
|-- FirmwareInspector.slnx
|-- LICENSE
|-- README.md
|-- SECURITY.md
`-- THIRD_PARTY_NOTICES.md
```

## 当前边界

- 不读取、不解析、不生成 ACPI 数据。
- `GetSystemFirmwareTable('RSMB')` 不返回 `_SM2_` / `_SM3_` Entry Point
  Structure，因此不显示 EPS 校验和、Revision 或结构表物理地址。
- Type 31 的 BIS 字段和 Type 44 的架构专用块依赖外部规范，当前只解析可确认字段。
- OEM Type `128-255` 不做厂商私有格式推断。
- 当前发布目标为 Windows x64。

## 贡献

提交代码前请阅读 [CONTRIBUTING.md](CONTRIBUTING.md)。新增字段解析必须提供：

- DSP0134 字段依据。
- 合成结构测试。
- 对边界长度、未知值和保留值的处理。
- 不包含真实序列号、UUID 或厂商机器转储的测试数据。

## 安全

安全问题请通过 GitHub 私有漏洞报告流程提交，参见 [SECURITY.md](SECURITY.md)。

## 许可证

本项目使用 [MIT License](LICENSE)。

