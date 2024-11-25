using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace ProcExp512SysDotNetProxy.Impl;

internal enum DriverCommand_IoctlCommand : uint
{
    OpenProtectedProcessHandle = 2201288764, // 0x8335003C
    CloseHandle = 2201288708, // 0x83350004
    GetHandleName = 2201288776, // 0x83350048
    GetHandleType = 2201288780 // 0x8335004C
}

internal struct DriverCommand_ProcExpDataExchange
{
    public ulong Pid;
    public IntPtr ObjectAddress;
    public ulong Size;
    public IntPtr Handle;
}

internal static class DriverCommand_OpenProcessHandle
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool DeviceIoControl(
       SafeFileHandle hDevice,
       uint dwIoControlCode,
       ref ulong lpInBuffer,
       int nInBufferSize,
       out SafeFileHandle lpOutBuffer,
       int nOutBufferSize,
       out uint lpBytesReturned,
       IntPtr lpOverlapped);

    public static SafeFileHandle OpenProtectedProcessHandle(SafeFileHandle openedDriverFile, ulong pid)
    {
        var inPid = pid;
        var result = DeviceIoControl(openedDriverFile,
            (uint)DriverCommand_IoctlCommand.OpenProtectedProcessHandle,
            ref inPid,
            Marshal.SizeOf(inPid),
            out var openedProcessHandle,
            Marshal.SizeOf(typeof(IntPtr)),
            out var bytesReturned,
            IntPtr.Zero);
        if (!result)
        {
            throw new ProcExp512SysDotNetProxyWinApiException("DeviceIoControl", $"Failed to open process with pid={pid}");
        }

        return openedProcessHandle;
    }
}

internal static class DriverCommand_GetHandleNameOrType
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        ref DriverCommand_ProcExpDataExchange lpInBuffer,
        int nInBufferSize,
        [Out] byte[] lpOutBuffer,
        int nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped);

    public static string? GetHandleName(SafeFileHandle openedDriverFile, SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        return GetHandleNameOrType(openedDriverFile, DriverCommand_IoctlCommand.GetHandleName, handleInfo);
    }

    public static string GetHandleType(SafeFileHandle openedDriverFile, SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        return GetHandleNameOrType(openedDriverFile, DriverCommand_IoctlCommand.GetHandleType, handleInfo)
            ?? throw new ProcExp512SysDotNetProxyException("Failed to retrieve handle type");
    }

    private static string? GetHandleNameOrType(SafeFileHandle openedDriverFile, DriverCommand_IoctlCommand ioctlCommand, SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        var data = new DriverCommand_ProcExpDataExchange
        {
            Pid = handleInfo.UniqueProcessId.ToUInt64(),
            ObjectAddress = handleInfo.Object,
            Size = 0,
            Handle = handleInfo.HandleValue
        };

        byte[] outBuffer = new byte[40000];
        bool result = DeviceIoControl(
            openedDriverFile,
            (uint)ioctlCommand,
            ref data,
            Marshal.SizeOf(data),
            outBuffer,
            outBuffer.Length,
            out var bytesReturned,
            IntPtr.Zero);

        if (!result)
        {
            throw new ProcExp512SysDotNetProxyWinApiException("DeviceIoControl", $"Failed to get handle name or type. IoctlCommand={ioctlCommand}");
        }

        if (bytesReturned == 8)
        {
            return null;
        }

        return Encoding.Unicode.GetString(outBuffer, 4, (int)bytesReturned - 10);
    }
}

internal static class DriverCommand_CloseHandle
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        ref DriverCommand_ProcExpDataExchange lpInBuffer,
        int nInBufferSize,
        IntPtr lpOutBuffer,
        int nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped);

    public static void CloseHandle(SafeFileHandle openedDriverFile, SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        var data = new DriverCommand_ProcExpDataExchange
        {
            Pid = handleInfo.UniqueProcessId.ToUInt64(),
            ObjectAddress = handleInfo.Object,
            Size = 0,
            Handle = handleInfo.HandleValue
        };

        var res = DeviceIoControl(
            openedDriverFile,
            (uint)DriverCommand_IoctlCommand.CloseHandle,
            ref data,
            Marshal.SizeOf(typeof(DriverCommand_ProcExpDataExchange)),
            IntPtr.Zero,
            0,
            out var bytesReturned,
            IntPtr.Zero);

        if(!res)
        {
            throw new ProcExp512SysDotNetProxyWinApiException("DeviceIoControl", $"Failed to close handle");
        }
    }
}
