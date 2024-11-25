using System.Runtime.InteropServices;

namespace ProcExp512SysDotNetProxy.Impl;

internal static class NtDll
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_HANDLE_INFORMATION_EX
    {
        public IntPtr NumberOfHandles;
        public IntPtr Reserved;
        public SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Handles; // Single element
    }

    private enum SYSTEM_INFORMATION_CLASS
    {
        SystemExtendedHandleInformation = 64
    }

    private const uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
    private const int NT_SUCCESS = 0;

    [DllImport("ntdll.dll")]
    private static extern uint NtQuerySystemInformation(
        SYSTEM_INFORMATION_CLASS systemInformationClass,
        IntPtr systemInformation,
        int systemInformationLength,
        out int returnLength);

    public static SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] QuerySystemHandleInformation()
    {
        for (var buffSize = 32 * 1024 * 1024 /* 32Mb */; buffSize <= 1024 * 1024 * 1024 /* 1Gb */; buffSize *= 2)
        {
            var buffer = Marshal.AllocHGlobal(buffSize);
            try
            {
                var status = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation, buffer, buffSize, out _);

                if (status == NT_SUCCESS)
                {
                    var handleInfo = Marshal.PtrToStructure<SYSTEM_HANDLE_INFORMATION_EX>(buffer);
                    return GetHandleEntries(handleInfo, buffer);
                }

                if (status != STATUS_INFO_LENGTH_MISMATCH)
                {
                    throw new ProcExp512SysDotNetProxyException($"NtQuerySystemInformation failed with status {status}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        throw new ProcExp512SysDotNetProxyException("NtQuerySystemInformation buffer size is not enough");
    }

    private static SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] GetHandleEntries(SYSTEM_HANDLE_INFORMATION_EX handleInfo, IntPtr buffer)
    {
        var numberOfHandles = handleInfo.NumberOfHandles.ToInt64();
        var handleEntries = new SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[numberOfHandles];

        long handleSize = Marshal.SizeOf(typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));
        var current = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION_EX)) - handleSize);

        for (long i = 0; i < numberOfHandles; i++)
        {
            var handleEntry = Marshal.PtrToStructure<SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>(current);
            handleEntries[i] = handleEntry;
            current = new IntPtr(current.ToInt64() + handleSize);
        }

        return handleEntries;
    }
}
