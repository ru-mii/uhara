## Syntax
```c#
IntPtr Add(
[in] string _assembly
[in] string _namespace
[in] string _class
[in] string _method
[in] short paramCount
[in] short hookOffset
[in] short overwriteSize
[in] byte[] bytes
);
```   
## Parameters
**[in] _class**   
Class that method belongs to.   
<br>
**[in] paramCount**   
Parameter count of the method.   
<br>
**[in] hookOffset**   
Hook offset for the method for the long jump.   
<br>
**[in] overwriteSize**   
Minimum required byte steal size to preserve correct code execution with long jump, can't be less than 14.   
<br>
**[in] bytes**   
Injected asm code.   
## Return value
Address that points at the injected code minus 8 bytes.
Returns IntPtr.Zero if failed.   
## Remarks
Overload of 
[Add (string, string, string, string, short, short, short, byte[])](https://github.com/ru-mii/uhara/tree/main/doc/uhara2/tools/unitycs/jitsave/Add%20(string%2C%20string%2C%20string%2C%20string%2C%20short%2C%20short%2C%20short%2C%20byte%5B%5D))   
_assembly = "Assembly-CSharp.dll"   
_namespace = ""
