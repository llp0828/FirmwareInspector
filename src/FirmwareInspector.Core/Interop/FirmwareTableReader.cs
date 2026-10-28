using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FirmwareInspector.Core.Interop;

public static class FirmwareTableProvider
{
    public const uint RawSmbios = 0x52534D42; // 'RSMB'
}

public static class FirmwareTableReader
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetSystemFirmwareTable(
        uint firmwareTableProviderSignature,
        uint firmwareTableId,
        IntPtr buffer,
        uint bufferSize);

    public static byte[] ReadRawSmbios()
    {
        var required = GetSystemFirmwareTable(
            FirmwareTableProvider.RawSmbios,
            0,
            IntPtr.Zero,
            0);

        if (required == 0)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                "GetSystemFirmwareTable('RSMB') 未返回可用的 SMBIOS 数据。");
        }

        var buffer = new byte[checked((int)required)];
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        try
        {
            var written = GetSystemFirmwareTable(
                FirmwareTableProvider.RawSmbios,
                0,
                handle.AddrOfPinnedObject(),
                required);

            if (written == 0)
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    "读取 SMBIOS 固件表失败。");
            }

            if (written != required)
            {
                throw new InvalidOperationException(
                    $"SMBIOS 数据长度变化：请求 {required} 字节，实际返回 {written} 字节。");
            }

            return buffer;
        }
        finally
        {
            handle.Free();
        }
    }
}
