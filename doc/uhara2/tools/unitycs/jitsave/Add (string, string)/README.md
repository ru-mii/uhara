## Syntax
```c#
IntPtr Add(
[in] string _class
[in] string _method
);
```   
## Parameters
**[in] _class**   
Class that method belongs to.   
<br>
**[in] _method**   
Method name.   
## Return value
Address that points at the injected code minus 8 bytes.   
Returns IntPtr.Zero if failed.   
## Remarks
Overload for 
[Add (string, string, string, string, short, short, short, byte[])](https://github.com/ru-mii/uhara/tree/main/doc/uhara2/tools/unitycs/jitsave/Add%20(string%2C%20string%2C%20string%2C%20string%2C%20short%2C%20short%2C%20short%2C%20byte%5B%5D))   
Default parameter values:   
_assembly = "Assembly-CSharp.dll"   
_namespace = ""   
paramCount = 0   
hookOffset = 0   
overwriteSize = 15   
bytes = new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 } (mov [rip-8], rdi; nop)
