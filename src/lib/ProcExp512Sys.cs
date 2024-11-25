using Microsoft.Win32.SafeHandles;
using ProcExp512SysDotNetProxy.Impl;

namespace ProcExp512SysDotNetProxy;

public sealed class ProcExp512Sys : IDisposable
{
    private SafeFileHandle openedDriverFile;

    public ProcExp512Sys()
    {
        openedDriverFile = DriverLoader.LoadDriverAndOpenTheDriverFile();
    }

    public SafeFileHandle OpenProtectedProcessHandle(ulong pid)
    {
        return DriverCommand_OpenProcessHandle.OpenProtectedProcessHandle(openedDriverFile, pid);
    }

    public void CloseHandle(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        DriverCommand_CloseHandle.CloseHandle(openedDriverFile, handleInfo);
    }

    public string GetHandleType(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        return DriverCommand_GetHandleNameOrType.GetHandleType(openedDriverFile, handleInfo);
    }

    public string? GetHandleName(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handleInfo)
    {
        return DriverCommand_GetHandleNameOrType.GetHandleName(openedDriverFile, handleInfo);
    }

    void IDisposable.Dispose()
    {
        openedDriverFile.Dispose();
    }
}
