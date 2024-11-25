using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace ProcExp512SysDotNetProxy.Impl;

internal sealed class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CloseServiceHandle(IntPtr hSCObject);

    public SafeServiceHandle() : base(true) { }

    protected override bool ReleaseHandle()
    {
        return CloseServiceHandle(handle);
    }
}
