using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirmwareInspector.Core.Exporting;
using FirmwareInspector.Core.Interop;
using FirmwareInspector.Core.Models;
using FirmwareInspector.Core.Parsing;
using Microsoft.Win32;

namespace FirmwareInspector.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private SmbiosParseResult? _snapshot;

    public MainViewModel()
    {
        StructureView = CollectionViewSource.GetDefaultView(Structures);
        StructureView.Filter = FilterStructure;
    }

    public ObservableCollection<StructureItemViewModel> Structures { get; } = [];
    public ObservableCollection<FieldItemViewModel> Fields { get; } = [];
    public ObservableCollection<StringItemViewModel> Strings { get; } = [];
    public ObservableCollection<HexLineViewModel> HexLines { get; } = [];
    public ICollectionView StructureView { get; }

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private StructureItemViewModel? _selectedStructure;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusText = "等待读取固件表";

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _versionText = "—";

    [ObservableProperty]
    private string _machineManufacturer = "—";

    [ObservableProperty]
    private string _machineProduct = "—";

    [ObservableProperty]
    private string _biosVersion = "—";

    [ObservableProperty]
    private string _biosDate = "—";

    [ObservableProperty]
    private string _tableLengthText = "—";

    [ObservableProperty]
    private string _structureCountText = "0";

    [ObservableProperty]
    private string _capturedAtText = "—";

    [ObservableProperty]
    private string _warningText = "0";

    [ObservableProperty]
    private string _warningDetails = "无解析警告";

    [ObservableProperty]
    private string _selectedTypeLabel = "—";

    [ObservableProperty]
    private string _selectedTypeName = "未选择结构";

    [ObservableProperty]
    private string _selectedPrimaryText = string.Empty;

    [ObservableProperty]
    private string _selectedHandle = "—";

    [ObservableProperty]
    private string _selectedLength = "—";

    [ObservableProperty]
    private string _selectedOffset = "—";

    [ObservableProperty]
    private string _selectedFieldCount = "0";

    public bool HasWarnings => _snapshot?.Warnings.Count > 0;
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool IsIdle => !IsBusy;

    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusText = "正在通过 GetSystemFirmwareTable('RSMB') 读取固件表…";

        try
        {
            var snapshot = await Task.Run(() =>
            {
                var raw = FirmwareTableReader.ReadRawSmbios();
                return SmbiosParser.Parse(raw);
            });

            ApplySnapshot(snapshot);
            StatusText = $"读取完成 · {snapshot.Structures.Count} 个结构 · {snapshot.StructureTable.Length:N0} 字节";
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusText = "读取失败";
            MessageBox.Show(
                exception.Message,
                "无法读取 SMBIOS",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsIdle));
        }
    }

    [RelayCommand]
    private void CopySelectedHex()
    {
        if (SelectedStructure is null)
        {
            return;
        }

        Clipboard.SetText(
            Convert.ToHexString(SelectedStructure.Structure.RawBytes.Span).Chunk(2)
                .Select(chars => new string(chars))
                .Aggregate(new StringBuilder(), (builder, part) => builder.Append(part).Append(' '))
                .ToString()
                .Trim());
        StatusText = "已复制当前结构原始字节";
    }

    [RelayCommand]
    private void ExportRaw()
    {
        if (_snapshot is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "导出原始 SMBIOS 数据",
            FileName = "smbios-raw.bin",
            Filter = "Binary (*.bin)|*.bin|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllBytes(dialog.FileName, _snapshot.RawData.ToArray());
            StatusText = $"已导出 {dialog.FileName}";
        }
    }

    [RelayCommand]
    private void ExportIni()
    {
        if (_snapshot is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "导出 SMBIOS INI",
            FileName = "smbios.ini",
            Filter = "INI (*.ini)|*.ini|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        SmbiosIniExporter.Write(dialog.FileName, _snapshot);
        StatusText = $"已导出 {dialog.FileName}";
    }

    [RelayCommand]
    private void ExportJson()
    {
        if (_snapshot is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "导出 SMBIOS 解析结果",
            FileName = "smbios-parsed.json",
            Filter = "JSON (*.json)|*.json|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var data = new
        {
            source = _snapshot.Source,
            capturedAt = _snapshot.CapturedAt,
            version = _snapshot.Version.ToString(),
            dmiRevision = _snapshot.Version.DmiRevision,
            declaredTableLength = _snapshot.DeclaredTableLength,
            parsedTableLength = _snapshot.StructureTable.Length,
            warnings = _snapshot.Warnings,
            structures = _snapshot.Structures.Select(structure => new
            {
                index = structure.Index,
                type = structure.Type,
                typeName = structure.TypeName,
                length = structure.Length,
                handle = $"0x{structure.Handle:X4}",
                offset = $"0x{structure.TableOffset:X4}",
                fields = structure.Fields,
                strings = structure.Strings,
                rawHex = Convert.ToHexString(structure.RawBytes.Span)
            })
        };

        File.WriteAllText(
            dialog.FileName,
            JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);
        StatusText = $"已导出 {dialog.FileName}";
    }

    partial void OnSearchTextChanged(string value)
    {
        StructureView.Refresh();
    }

    partial void OnSelectedStructureChanged(StructureItemViewModel? value)
    {
        Fields.Clear();
        Strings.Clear();
        HexLines.Clear();

        if (value is null)
        {
            SelectedTypeLabel = "—";
            SelectedTypeName = "未选择结构";
            SelectedPrimaryText = string.Empty;
            SelectedHandle = "—";
            SelectedLength = "—";
            SelectedOffset = "—";
            SelectedFieldCount = "0";
            return;
        }

        var structure = value.Structure;
        SelectedTypeLabel = value.TypeLabel;
        SelectedTypeName = value.TypeName;
        SelectedPrimaryText = value.Summary;
        SelectedHandle = value.HandleText;
        SelectedLength = $"{value.LengthText} bytes";
        SelectedOffset = $"0x{structure.TableOffset:X4}";
        SelectedFieldCount = structure.Fields.Count.ToString();

        foreach (var item in structure.Fields)
        {
            Fields.Add(new FieldItemViewModel(item));
        }

        foreach (var item in structure.Strings)
        {
            Strings.Add(new StringItemViewModel(item));
        }

        var bytes = structure.RawBytes.Span;
        for (var offset = 0; offset < bytes.Length; offset += 16)
        {
            HexLines.Add(new HexLineViewModel(
                offset,
                bytes.Slice(offset, Math.Min(16, bytes.Length - offset))));
        }
    }

    private bool FilterStructure(object item)
    {
        if (item is not StructureItemViewModel structure)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var query = SearchText.Trim();
        if (TryGetTypeNumber(query, out var typeNumber))
        {
            return structure.Structure.Type == typeNumber;
        }

        return structure.SearchHaystack.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetTypeNumber(string query, out byte typeNumber)
    {
        var value = query;
        if (value.StartsWith("type", StringComparison.OrdinalIgnoreCase))
        {
            value = value[4..].Trim();
        }
        else if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return byte.TryParse(
                value[2..],
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out typeNumber);
        }
        else
        {
            typeNumber = 0;
            return false;
        }

        return byte.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out typeNumber);
    }

    private void ApplySnapshot(SmbiosParseResult snapshot)
    {
        _snapshot = snapshot;
        VersionText = snapshot.Version.ToString();
        TableLengthText = $"{snapshot.StructureTable.Length:N0} B";
        StructureCountText = snapshot.Structures.Count.ToString();
        CapturedAtText = snapshot.CapturedAt.ToString("yyyy-MM-dd HH:mm:ss");
        WarningText = snapshot.Warnings.Count.ToString();
        WarningDetails = snapshot.Warnings.Count == 0
            ? "无解析警告"
            : string.Join(Environment.NewLine, snapshot.Warnings);
        OnPropertyChanged(nameof(HasWarnings));
        OnPropertyChanged(nameof(SelectedStructure));
        OnPropertyChanged(nameof(HasError));

        Structures.Clear();
        foreach (var structure in snapshot.Structures)
        {
            Structures.Add(new StructureItemViewModel(structure));
        }

        var system = snapshot.Structures.FirstOrDefault(item => item.Type == 1);
        var firmware = snapshot.Structures.FirstOrDefault(item => item.Type == 0);

        MachineManufacturer = FieldValue(system, "Manufacturer");
        MachineProduct = FieldValue(system, "Product Name");
        BiosVersion = FieldValue(firmware, "Firmware Version");
        BiosDate = FieldValue(firmware, "Firmware Release Date");

        SelectedStructure =
            Structures.FirstOrDefault(item => item.Structure.Type == 1) ??
            Structures.FirstOrDefault();
    }

    private static string FieldValue(SmbiosStructure? structure, string name) =>
        structure?.Fields.FirstOrDefault(item => item.Name == name)?.Value ?? "—";
}
