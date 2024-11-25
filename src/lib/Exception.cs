using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ProcExp512SysDotNetProxy;

public class ProcExp512SysDotNetProxyException : Exception
{
    public ProcExp512SysDotNetProxyException(string message) : base(message)
    {
    }
}

public class ProcExp512SysDotNetProxyWinApiException : ProcExp512SysDotNetProxyException
{
    public ProcExp512SysDotNetProxyWinApiException(string functionName, string message) :
        base(@$"{functionName} failed.
{message}.
WinApi error code: {Marshal.GetLastWin32Error()}(0x{Marshal.GetLastWin32Error():X}).
WinApi error message: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.")
    {
    }
}
