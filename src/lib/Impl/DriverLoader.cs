using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security.Principal;

namespace ProcExp512SysDotNetProxy.Impl;

internal static class DriverLoader_ConnectToDriver
{
    public const uint GENERIC_ALL = 0x10000000;
    public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    public const uint OPEN_EXISTING = 3;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    public static SafeFileHandle ConnectToDriver(string driverServiceName)
    {
        var driverFile = CreateFile(@$"\\.\{driverServiceName}",
            GENERIC_ALL,
            0,
            IntPtr.Zero,
            OPEN_EXISTING,
            FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero);
        if (driverFile.IsInvalid)
        {
            throw new ProcExp512SysDotNetProxyWinApiException("CreateFile", "Failed to open the driver file");
        }

        return driverFile;
    }
}

internal static class DriverLoader_CreateOrOpenService
{
    private const uint SERVICE_KERNEL_DRIVER = 0x00000001;
    private const uint SERVICE_DEMAND_START = 0x00000003;
    private const uint SERVICE_ERROR_NORMAL = 0x00000001;
    private const uint SERVICE_ALL_ACCESS = 0xF01FF;
    private const int ERROR_SERVICE_EXISTS = 1073;

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern SafeServiceHandle CreateService(
        SafeServiceHandle hSCManager,
        string lpServiceName,
        string lpDisplayName,
        uint dwDesiredAccess,
        uint dwServiceType,
        uint dwStartType,
        uint dwErrorControl,
        string lpBinaryPathName,
        string? lpLoadOrderGroup,
        IntPtr lpdwTagId,
        string? lpDependencies,
        string? lpServiceStartName,
        string? lpPassword);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern SafeServiceHandle OpenService(
        SafeServiceHandle hSCManager,
        string lpServiceName,
        uint dwDesiredAccess);


    public static SafeServiceHandle CreateOrOpenService(SafeServiceHandle serviceManager, string driverServiceName, string driverFileFullName)
    {
        var service = CreateService(serviceManager,
            driverServiceName,
            "Process Explorer",
            SERVICE_ALL_ACCESS,
            SERVICE_KERNEL_DRIVER,
            SERVICE_DEMAND_START,
            SERVICE_ERROR_NORMAL,
            driverFileFullName,
            null,
            IntPtr.Zero,
            null,
            null,
            null);

        if (!service.IsInvalid)
        {
            return service;
        }

        if (Marshal.GetLastWin32Error() != ERROR_SERVICE_EXISTS)
        {
            throw new ProcExp512SysDotNetProxyWinApiException("CreateService", "Failed to create the driver service");
        }

        service = OpenService(serviceManager, driverServiceName, SERVICE_ALL_ACCESS);
        if (service.IsInvalid)
        {
            throw new ProcExp512SysDotNetProxyWinApiException("OpenService", "Failed to open existing driver service");
        }

        return service;
    }
}

internal static class DriverLoader_LoadDriver
{
    private const uint SC_MANAGER_CREATE_SERVICE = 0x0002;
    private const int ERROR_SERVICE_ALREADY_RUNNING = 1056;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern SafeServiceHandle OpenSCManager(
        string? lpMachineName,
        string? lpDatabaseName,
        uint dwDesiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool StartService(
        SafeServiceHandle hService,
        uint dwNumServiceArgs,
        IntPtr lpServiceArgVectors);

    public static void LoadDriver(string driverServiceName, string driverFileFullName)
    {
        using var serviceManager = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
        if (serviceManager.IsInvalid)
        {
            throw new ProcExp512SysDotNetProxyWinApiException("OpenSCManager", "Failed to open Service Manager");
        }

        using var service = DriverLoader_CreateOrOpenService.CreateOrOpenService(serviceManager, driverServiceName, driverFileFullName);

        var res = StartService(service, 0, IntPtr.Zero);
        if (!res && Marshal.GetLastWin32Error() != ERROR_SERVICE_ALREADY_RUNNING)
        {
            throw new ProcExp512SysDotNetProxyWinApiException("StartService", "Failed to start the service");
        }
    }
}

internal static class DriverLoader
{
    private static string driverFileFullName = Environment.ExpandEnvironmentVariables(@"%WinDir%\System32\drivers\PROCEXP152.SYS");
    private static string driverServiceName = "PROCEXP152";

    public static SafeFileHandle LoadDriverAndOpenTheDriverFile()
    {
        EnsureRunAsAdmin();
        CopyDriverFileToSystem32();
        DriverLoader_LoadDriver.LoadDriver(driverServiceName, driverFileFullName);
        return DriverLoader_ConnectToDriver.ConnectToDriver(driverServiceName);
    }

    private static void EnsureRunAsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            throw new ProcExp512SysDotNetProxyException("The process needs to be run from an admin");
        }
    }

    private static void CopyDriverFileToSystem32()
    {
        if (File.Exists(driverFileFullName))
        {
            return;
        }
        using var driverFile = Assembly.GetExecutingAssembly().GetManifestResourceStream("ProcExp512SysDotNetProxy.PROCEXP152.SYS");
        using var outputFile = new FileStream(driverFileFullName, FileMode.Create);
        driverFile.CopyTo(outputFile);
    }
}
