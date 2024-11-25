using NUnit.Framework;
using ProcExp512SysDotNetProxy;

namespace Test;

[TestFixture]
internal class Tests
{
    [Test]
    public void Open_process()
    {
        var io = new ProcExp512Sys();

        var handle = io.OpenProtectedProcessHandle(4);
        Assert.That(handle.IsInvalid, Is.False);
    }

    [Test]
    public void Get_handle_type_and_name()
    {
        var io = new ProcExp512Sys();

        var info = SystemHandlesRetriever.QuerySystemHandleInformation();
        var handleTypes = new HashSet<string>();
        var handleNames = new List<string?>();

        foreach (var h in info)
        {
            try
            {
                handleTypes.Add(io.GetHandleType(h));
                var name = io.GetHandleName(h);
                if (name != null)
                {
                    handleNames.Add(name);
                }
            }
            catch (ProcExp512SysDotNetProxyWinApiException)
            {
            }
        }

        Assert.That(handleTypes.Count, Is.GreaterThan(0));
        Assert.That(handleTypes, Does.Contain("Process"));
        Assert.That(handleTypes, Does.Contain("Event"));
        Assert.That(handleTypes, Does.Contain("File"));

        Assert.That(handleNames.Count, Is.GreaterThan(0));
        Assert.That(handleNames, Has.Some.Contains("System32"));
        Assert.That(handleNames, Has.Some.Contains(@"\Device"));
    }

    [Test]
    public void Convert_handle_name_to_disk_letter_based_path()
    {
        var converter = new FileHandleNameConverter();

        Assert.That(converter.ToDriveLetterBasedFullName(@"\Device\HarddiskVolume3\Windows\System32\en-US\KernelBase.dll.mui"),
            Is.EqualTo(@"C:\Windows\System32\en-US\KernelBase.dll.mui"));

        Assert.Throws<ProcExp512SysDotNetProxyException>(() => converter.ToDriveLetterBasedFullName(@"\Device\HarddiskVolume2\Windows"));
    }

}
