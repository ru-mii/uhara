## Syntax
```c#
IntPtr ScanSingle(
[in] string signature
);
```   
## Parameters
**[in] signature**   
Scan signature in this format "48 8B !3D ?? ?? ?? ?? 48 8B 72 ?? 48 !85 F6".   
## Return value
Returns virtual address at where the byte array of the signature begins.   
Returns IntPtr.Zero if fails.   
## Remarks
Scans game's main module.
