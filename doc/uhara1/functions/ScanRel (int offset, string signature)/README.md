## Syntax
```c#
IntPtr ScanSingle(
[in] int offset
[in] string signature
);
```   
## Parameters
**[in] offset**   
Offset in bytes to the relative value in the assembly instruction.   
&nbsp;
**[in] signature**   
Scan signature in this format "48 8B !3D ?? ?? ?? ?? 48 8B 72 ?? 48 !85 F6".   
## Return value
Returns virtual address of the relative referenced value in the assembly instruction.
Returns IntPtr.Zero if failed or not found.   
## Remarks
Scans game's main module, relative address is then read and retrieved from the assembly instruction
