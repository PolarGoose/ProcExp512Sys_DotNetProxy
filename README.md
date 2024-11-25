# ProcExpSys512_DotNetProxy
.Net library for interfacing with the `ProcExp512.sys` driver.<br>
The following functionality is supported:
* Open protected process handle
* Close handle
* Get handle type and name

The library automatically extracts the `ProcExp512.sys` driver and loads it if needed.

There are also helper classes for:
* Get all handles in the system
* Convert file handles to file names

# Details
`ProcExp512.sys` is a Windows kernel driver that is part of the Sysinternals [Process Explorer](https://learn.microsoft.com/en-us/sysinternals/downloads/process-explorer) and [Handle](https://learn.microsoft.com/en-us/sysinternals/downloads/handle).
It is used to get access to privileged processes and handles. For example, it allows access to handles of a System (pid=4) process, which is not possible by conventional means.<br>
Even though it is possible to use NtDll to get handle names. It has a bug when a method to get the name or type of a handle hangs the calling thread without any way to recover ([issue](https://github.com/giampaolo/psutil/issues/340)). Thus, the  `ProcExp512` driver is the only reliable way to do that.

# How to use
Currently this library is not distributed as a NuGet package. You need to clone the repository and build the library yourself.

# Example of a potential use case for this library
Making utilities like [Backstab](https://github.com/Yaxser/Backstab) in C#.



