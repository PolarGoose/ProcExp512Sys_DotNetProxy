using ProcExp512SysDotNetProxy.Impl;

namespace ProcExp512SysDotNetProxy;

public struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
{
    // Pointer to the handle in the kernel virtual address space.
    public IntPtr Object;

    // PID that owns the handle
    public UIntPtr UniqueProcessId;

    // Handle value in the process that owns the handle.
    public IntPtr HandleValue;

    // Access rights associated with the handle.
    // Bit mask consisting of the fields defined in the winnt.h
    // For example: READ_CONTROL|DELETE|SYNCHRONIZE|WRITE_DAC|WRITE_OWNER|EVENT_ALL_ACCESS
    // The exact information that this field contain depends on the type of the handle.
    public uint GrantedAccess;

    // This filed is reserved for debugging purposes
    // For instance, it can store an index to a stack trace that was captured when the handle was created.
    public ushort CreatorBackTraceIndex;

    // Type of object a handle refers to.
    // For instance: file, thread, or process
    public ushort ObjectTypeIndex;

    // Bit mask that provides additional information about the handle.
    // For example: OBJ_INHERIT, OBJ_EXCLUSIVE
    // The attributes are defined in the winternl.h
    public uint HandleAttributes;

    public uint Reserved;
}

public static class SystemHandlesRetriever
{
    public static SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] QuerySystemHandleInformation()
    {
        return NtDll.QuerySystemHandleInformation();
    }
}
