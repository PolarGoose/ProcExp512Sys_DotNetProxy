using System.Runtime.InteropServices;
using System.Text;

namespace ProcExp512SysDotNetProxy;

public sealed class FileHandleNameConverter
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, uint ucchMax);

    private static readonly Dictionary<string, char> deviceNameToDriveLetterConversionMap = CreateDeviceNameToDriveLetterConversionMap();

    // Creates a conversion map {device_name, drive_letter}, consisting of all
    // available logical drives on the machine. Example:
    //   { {"\\Device\\HardDiskVolume2\\", 'C'},
    //     {"\\Device\\HardDiskVolume15\\", 'D'},
    //     {"\\Device\\VBoxMiniRdr\\;H:\\VBoxSvr\\My-H\\", 'H'},
    //     {"\\Device\\LanmanRedirector\\;I:000215d7\\10.22.3.84\\i\\", 'I'},
    //     {"\\Device\\LanmanRedirector\\;S:000215d7\\10.22.3.84\\devshare\\", 'S'},
    //     {"\\Device\\LanmanRedirector\\;U:000215d7\\10.22.3.190\\d$\\", 'U'},
    //     {"\\Device\\LanmanRedirector\\;V:000215d7\\10.22.3.153\\d$\\", 'V'},
    //     {"\\Device\\CdRom0\\", 'X'} }
    private static Dictionary<string, char> CreateDeviceNameToDriveLetterConversionMap()
    {
        var conversionMap = new Dictionary<string, char>();

        for (var driveLetter = 'A'; driveLetter <= 'Z'; driveLetter++)
        {
            var deviceNameBuffer = new StringBuilder(1024);
            string drive = $"{driveLetter}:";

            uint length = QueryDosDevice(drive, deviceNameBuffer, (uint)deviceNameBuffer.Capacity);
            if (length == 0)
            {
                continue;
            }

            // The returned from QueryDosDevice device name doesn't have a '\' at the end.
            // We add it to distinguish between similar device names.
            deviceNameBuffer.Append(@"\");
            conversionMap[deviceNameBuffer.ToString()] = driveLetter;
        }

        return conversionMap;
    }

    // Converts a device-based file path to a drive letter-based full path.
    // For example:
    //   From: "\\Device\\HardDiskVolume3\\Windows\\System32\\en-US\\KernelBase.dll.mui"
    //   To:   "C:\\Windows\\System32\\en-US\\KernelBase.dll.mui"
    public string ToDriveLetterBasedFullName(string deviceBasedFileFullName)
    {
        foreach (var kvp in deviceNameToDriveLetterConversionMap)
        {
            string deviceName = kvp.Key;
            char driveLetter = kvp.Value;

            if (deviceBasedFileFullName.StartsWith(deviceName, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = deviceBasedFileFullName.Substring(deviceName.Length);
                return $@"{driveLetter}:\{relativePath}";
            }
        }

        throw new ProcExp512SysDotNetProxyException($"Couldn't convert '{deviceBasedFileFullName}' to the drive letter based path");
    }
}
